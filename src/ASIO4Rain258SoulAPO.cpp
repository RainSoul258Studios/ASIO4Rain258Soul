#include "ASIO4Rain258SoulAPO.h"
#include <new>

static HMODULE g_hModule = nullptr;
static std::atomic<long> g_serverLocks{ 0 };

CASIO4Rain258SoulAPO::CASIO4Rain258SoulAPO() {
    g_serverLocks.fetch_add(1, std::memory_order_relaxed);
    LoadDefaultState();
    m_ipcClient.Initialize(false);
}
CASIO4Rain258SoulAPO::~CASIO4Rain258SoulAPO() { g_serverLocks.fetch_sub(1, std::memory_order_relaxed); }

STDMETHODIMP CASIO4Rain258SoulAPO::QueryInterface(REFIID riid, void** ppvObject) {
    if (!ppvObject) return E_POINTER;
    if (riid == IID_IUnknown || riid == CLSID_ASIO4Rain258SoulAPO) {
        *ppvObject = static_cast<IUnknown*>(this);
        AddRef();
        return S_OK;
    }
    *ppvObject = nullptr;
    return E_NOINTERFACE;
}
STDMETHODIMP_(ULONG) CASIO4Rain258SoulAPO::AddRef() { return InterlockedIncrement(&m_refCount); }
STDMETHODIMP_(ULONG) CASIO4Rain258SoulAPO::Release() {
    ULONG count = InterlockedDecrement(&m_refCount);
    if (count == 0) delete this;
    return count;
}

void CASIO4Rain258SoulAPO::LoadDefaultState() noexcept {
    m_localCache.asioBufferSamples = 256;
    m_localCache.boom3DSurroundActive = true;
    m_localCache.boom3DZeroLatencyBypass = false;
    m_localCache.phaseShieldEnabled = true;
    m_localCache.endpointCount = 12;

    for (uint32_t e = 0; e < m_localCache.endpointCount; ++e) {
        auto& ep = m_localCache.endpoints[e];
        ep.endpointId = e;
        ep.isInput = (e < 6);
        ep.isVirtual = (e % 2 == 1);
        ep.asio258MasterEnabled = true;
        ep.customBufferSamples = 256;
        ep.gainLinear = 1.0f;
        ep.enable3DHRTF = true;
        ep.enablePhaseShield = true;
        ep.enableLA2ACompressor = true;
        ep.oggBitrateKbps = 320;
        ep.azimuthDeg = -120.0f + (static_cast<float>(e) * 20.0f);
        ep.distanceMeters = 1.2f;
    }

    for (int ch = 0; ch < OMNI_MAX_SPATIAL_CHANNELS; ++ch) {
        m_localCache.channels[ch].azimuthDeg = -165.0f + (static_cast<float>(ch) * (330.0f / 23.0f));
        m_localCache.channels[ch].elevationDeg = (ch % 2 == 0) ? 12.0f : -8.0f;
        m_localCache.channels[ch].distanceMeters = 1.2f + (static_cast<float>(ch % 4) * 0.35f);
        m_localCache.channels[ch].gainLinear = 1.0f;
        m_localCache.channels[ch].hrtfEnabled = true;
    }

    for (int i = 0; i < 64; ++i) m_localCache.eq64GainsLinear[i] = 1.0f;
    for (int i = 16; i <= 21; ++i) m_localCache.eq64GainsLinear[i] = 0.749f;
    for (int i = 54; i <= 59; ++i) m_localCache.eq64GainsLinear[i] = 1.412f;

    m_localCache.la2aLimitMode = false;
    m_localCache.la2aPeakReduction = 42.0f;
    m_localCache.la2aMakeupLinear = 1.412f;
    m_localCache.la2aR37HFEmphasis = 0.35f;
    m_localCache.la2aTubeWarmth = 0.28f;
}

void CASIO4Rain258SoulAPO::SyncFromSharedMemory() noexcept {
    OmniIPCControlBlock* pShared = m_ipcClient.GetBlock();
    if (!pShared) return;
    uint32_t currentVer = pShared->sequenceVersion.load(std::memory_order_acquire);
    if (currentVer != m_lastSyncVersion) {
        m_localCache.asioBufferSamples = pShared->asioBufferSamples;
        m_localCache.boom3DSurroundActive = pShared->boom3DSurroundActive;
        m_localCache.boom3DZeroLatencyBypass = pShared->boom3DZeroLatencyBypass;
        m_localCache.phaseShieldEnabled = pShared->phaseShieldEnabled;
        m_lastSyncVersion = currentVer;
    }
}

STDMETHODIMP CASIO4Rain258SoulAPO::Initialize(UINT32 cbDataSize, BYTE* pbyData) {
    if (pbyData == nullptr && cbDataSize != 0) return E_INVALIDARG;
    return S_OK;
}

inline void CASIO4Rain258SoulAPO::ProcessUniversalEndpointsAnd24Ch(float& L, float& R) noexcept {
    if (m_localCache.boom3DSurroundActive && m_localCache.phaseShieldEnabled && !m_localCache.boom3DZeroLatencyBypass) {
        float subMono = (L + R) * 0.5f;
        float subHarmonic = std::tanh(subMono * 2.2f) * 0.12f;
        L += subHarmonic;
        R += subHarmonic;
    }
}

inline void CASIO4Rain258SoulAPO::ProcessLA2AOptical(float& L, float& R) noexcept {
    float sidechain = (std::fabs(L) + std::fabs(R)) * 0.5f * (1.0f + m_localCache.la2aR37HFEmphasis * 0.4f);
    float drive = sidechain * (m_localCache.la2aPeakReduction / 35.0f);
    if (drive > m_t4bFastEnv) {
        m_t4bFastEnv = 0.992f * m_t4bFastEnv + 0.008f * drive;
        m_t4bSlowMemory = 0.9995f * m_t4bSlowMemory + 0.0005f * drive;
    } else {
        m_t4bFastEnv *= 0.9992f;
        m_t4bSlowMemory *= 0.99992f;
    }
    float envTotal = (m_t4bFastEnv * 0.7f) + (m_t4bSlowMemory * 0.3f);
    float ratio = m_localCache.la2aLimitMode ? 100.0f : 3.0f;
    float gainReductiondB = (envTotal > 0.15f) ? (envTotal - 0.15f) * (1.0f - (1.0f / ratio)) * 18.0f : 0.0f;
    float grLinear = std::pow(10.0f, -gainReductiondB / 20.0f) * m_localCache.la2aMakeupLinear;
    L *= grLinear; R *= grLinear;
    const float warmth = 1.0f + m_localCache.la2aTubeWarmth;
    L = std::tanh(L * warmth) / std::tanh(warmth);
    R = std::tanh(R * warmth) / std::tanh(warmth);
}

void CASIO4Rain258SoulAPO::APOProcessRT(UINT32 frames, float* pInBuf, float* pOutBuf) noexcept {
    if (!pInBuf || !pOutBuf || frames == 0) return;
    SyncFromSharedMemory();
    const float compositeEQ = (m_localCache.eq64GainsLinear[18] * 0.45f) + (m_localCache.eq64GainsLinear[56] * 0.55f);
    for (UINT32 i = 0; i < frames; ++i) {
        float L = pInBuf[i * m_channels];
        float R = (m_channels > 1) ? pInBuf[i * m_channels + 1] : L;
        ProcessUniversalEndpointsAnd24Ch(L, R);
        L *= compositeEQ; R *= compositeEQ;
        ProcessLA2AOptical(L, R);
        pOutBuf[i * m_channels] = L;
        if (m_channels > 1) pOutBuf[i * m_channels + 1] = R;
    }
}

extern "C" HRESULT __stdcall DllCanUnloadNow(void) { return S_OK; }
extern "C" HRESULT __stdcall DllGetClassObject(REFCLSID, REFIID, LPVOID* ppv) { if (ppv) *ppv = nullptr; return CLASS_E_CLASSNOTAVAILABLE; }
extern "C" HRESULT __stdcall DllRegisterServer(void) { return S_OK; }
extern "C" HRESULT __stdcall DllUnregisterServer(void) { return S_OK; }