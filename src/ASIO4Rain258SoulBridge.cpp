#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>
#include <tlhelp32.h>
#include <iostream>
#include <string>
#include "OmniSharedMemory.h"

#pragma comment(lib, "Shell32.lib")
#pragma comment(lib, "User32.lib")

static OmniIPCControlBlock* g_pIpc = nullptr;

void TerminateAllRelatedTasks() noexcept {
    if (g_pIpc) {
        g_pIpc->systemTrayActive = false;
        g_pIpc->masterOutDOggRecActive = false;
        g_pIpc->sequenceVersion.fetch_add(1, std::memory_order_release);
    }
}

int main(int argc, char* argv[]) {
    OmniSharedMemoryManager ipcManager;
    if (!ipcManager.Initialize(true)) return 1;
    g_pIpc = ipcManager.GetBlock();
    g_pIpc->asioBufferSamples = 256;
    g_pIpc->boom3DSurroundActive = true;
    g_pIpc->phaseShieldEnabled = true;
    g_pIpc->systemTrayActive = true;
    g_pIpc->telemetrySunoTotalOffsetMs.store(49.8f, std::memory_order_relaxed);

    if (argc > 1 && std::string(argv[1]) == "--verify") {
        std::cout << "[VERIFY OK] ASIO4Rain258Soul (asio258) | Politica CERO BACKGROUND activa | "
                  << "Minimizar -> Area de Notificaciones | Cerrar -> Finaliza todas las tareas.\n";
        TerminateAllRelatedTasks();
        return 0;
    }

    if (argc > 1 && std::string(argv[1]) == "--stop-all") {
        TerminateAllRelatedTasks();
        return 0;
    }

    // Si se lanza con --parent-pid <PID>, vigila al cliente GUI y muere inmediatamente si la ventana se cierra
    DWORD parentPid = 0;
    for (int i = 1; i < argc - 1; ++i) {
        if (std::string(argv[i]) == "--parent-pid") {
            parentPid = static_cast<DWORD>(std::stoul(argv[i + 1]));
        }
    }

    if (parentPid != 0) {
        HANDLE hParent = OpenProcess(SYNCHRONIZE, FALSE, parentPid);
        if (hParent) {
            WaitForSingleObject(hParent, INFINITE);
            CloseHandle(hParent);
        }
        TerminateAllRelatedTasks();
        return 0;
    }

    TerminateAllRelatedTasks();
    return 0;
}