#pragma once
#include <windows.h>
#include <atomic>
#include <cstdint>

static constexpr wchar_t OMNI_IPC_MAP_NAME[] = L"Global\\ASIO4Rain258Soul_IPC_v1";
static constexpr int OMNI_MAX_SPATIAL_CHANNELS = 24;
static constexpr int OMNI_MAX_AUDIO_ENDPOINTS  = 32;

#pragma pack(push, 8)
struct OmniEndpointDSPConfig {
    uint32_t endpointId;
    wchar_t  friendlyName[64];
    bool     isInput;                  // true = Entrada (Capture), false = Salida (Render)
    bool     isVirtual;                // false = HARDWARE, true = VIRTUAL
    bool     asio258MasterEnabled;     // Procesamiento integral asio258 activo
    uint32_t customBufferSamples;      // 64 a 2048 muestras
    float    gainLinear;               // 0.0f a 2.0f
    uint32_t eq64PresetId;             // 0: Suno AI Master Polish (0.749x / 1.412x)
    bool     enable3DHRTF;
    bool     enablePhaseShield;
    bool     enableLA2ACompressor;
    bool     enableZeroLatencyBypass;
    bool     enableSidechainDucking;
    bool     enableSpectralScanner;
    bool     phaseInvert;
    bool     mute;
    bool     recordOggActive;          // Grabacion .OGG en curso en este dispositivo E/S
    uint32_t oggBitrateKbps;           // 256, 320 o 510 kbps (VBR Q10 48 kHz)
    float    azimuthDeg;
    float    distanceMeters;
};

struct OmniIPCControlBlock {
    std::atomic<uint32_t> sequenceVersion;
    uint32_t asioBufferSamples;
    bool     wdmDeviceActive[4];
    bool     boom3DSurroundActive;
    bool     boom3DZeroLatencyBypass;
    bool     phaseShieldEnabled;
    bool     systemTrayActive;
    bool     masterOutDOggRecActive;   // Grabadora Maestra .OGG en Bus OUT D
    uint32_t masterOggBitrateKbps;     // Por defecto 320 kbps VBR Q10

    uint32_t endpointCount;
    OmniEndpointDSPConfig endpoints[OMNI_MAX_AUDIO_ENDPOINTS];

    struct Node3D {
        float azimuthDeg;
        float elevationDeg;
        float distanceMeters;
        float gainLinear;
        float eq3BanddB[3];
        bool  phaseInvert;
        bool  mute;
        bool  solo;
        bool  hrtfEnabled;
        bool  routeBus[4];             // OUT A (ASIO), OUT B (HRTF), OUT C (OBS), OUT D (DAW .OGG)
    } channels[OMNI_MAX_SPATIAL_CHANNELS];

    float eq64GainsLinear[64];
    bool  la2aLimitMode;
    float la2aPeakReduction;
    float la2aMakeupLinear;
    float la2aR37HFEmphasis;
    float la2aTubeWarmth;

    std::atomic<float> telemetryLA2AGainReductiondB;
    std::atomic<float> telemetryCutoffHz;
    std::atomic<float> telemetryEnvelopeRMS;
    std::atomic<float> telemetryLFORateHz;
    std::atomic<float> telemetrySunoTotalOffsetMs;
};
#pragma pack(pop)

class OmniSharedMemoryManager {
private:
    HANDLE m_hMapFile = nullptr;
    OmniIPCControlBlock* m_pBlock = nullptr;
public:
    bool Initialize(bool createAsServer) {
        if (createAsServer) {
            m_hMapFile = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, sizeof(OmniIPCControlBlock), OMNI_IPC_MAP_NAME);
            if (!m_hMapFile) m_hMapFile = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, sizeof(OmniIPCControlBlock), L"Local\\ASIO4Rain258Soul_IPC_v1");
        } else {
            m_hMapFile = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, OMNI_IPC_MAP_NAME);
            if (!m_hMapFile) m_hMapFile = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, L"Local\\ASIO4Rain258Soul_IPC_v1");
        }
        if (!m_hMapFile) return false;
        m_pBlock = static_cast<OmniIPCControlBlock*>(MapViewOfFile(m_hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(OmniIPCControlBlock)));
        return (m_pBlock != nullptr);
    }
    OmniIPCControlBlock* GetBlock() const noexcept { return m_pBlock; }
    ~OmniSharedMemoryManager() {
        if (m_pBlock) UnmapViewOfFile(m_pBlock);
        if (m_hMapFile) CloseHandle(m_hMapFile);
    }
};