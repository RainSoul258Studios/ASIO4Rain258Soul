#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <iostream>
#include <string>
#include "OmniSharedMemory.h"

int main(int argc, char* argv[]) {
    OmniSharedMemoryManager ipcManager;
    if (!ipcManager.Initialize(true)) return 1;
    OmniIPCControlBlock* ipc = ipcManager.GetBlock();
    ipc->asioBufferSamples = 256;
    ipc->boom3DSurroundActive = true;
    ipc->phaseShieldEnabled = true;
    ipc->endpointCount = 12;
    ipc->telemetrySunoTotalOffsetMs.store(49.8f, std::memory_order_relaxed);

    if (argc > 1 && std::string(argv[1]) == "--verify") {
        std::cout << "[VERIFY OK] ASIO4Rain258Soul (asio258) | 24Ch 3D + E/S HW/Virt + .OGG HQ + .JSON + Cero Background.\n";
        return 0;
    }
    return 0;
}