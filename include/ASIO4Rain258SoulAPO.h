#pragma once
#include <windows.h>
#include <unknwn.h>
#include <cmath>
#include <algorithm>
#include "OmniSharedMemory.h"

static const GUID CLSID_ASIO4Rain258SoulAPO =
    { 0x8f258a10, 0x4d3e, 0x4b9a, { 0x9c, 0x12, 0xfa, 0x02, 0x58, 0x00, 0x3d, 0x01 } };

class CASIO4Rain258SoulAPO : public IUnknown {
public:
    CASIO4Rain258SoulAPO();
    virtual ~CASIO4Rain258SoulAPO();
    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObject) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;
    STDMETHODIMP Initialize(UINT32 cbDataSize, BYTE* pbyData);
    void APOProcessRT(UINT32 frames, float* pInBuf, float* pOutBuf) noexcept;
private:
    std::atomic<ULONG> m_refCount{ 1 };
    float    m_sampleRate = 48000.0f;
    UINT32   m_channels = 2;
    uint32_t m_lastSyncVersion = 0;
    float    m_t4bFastEnv = 0.0f;
    float    m_t4bSlowMemory = 0.0f;
    static constexpr int MAX_ITD_SAMPLES = 64;
    float    m_delayLineL[MAX_ITD_SAMPLES] = { 0.0f };
    float    m_delayLineR[MAX_ITD_SAMPLES] = { 0.0f };
    int      m_delayWriteIdx = 0;
    OmniSharedMemoryManager m_ipcClient;
    OmniIPCControlBlock     m_localCache{};
    void LoadDefaultState() noexcept;
    void SyncFromSharedMemory() noexcept;
    inline void ProcessUniversalEndpointsAnd24Ch(float& L, float& R) noexcept;
    inline void ProcessLA2AOptical(float& L, float& R) noexcept;
};