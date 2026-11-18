#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>

#define WM_TRAYICON (WM_USER + 1)
#define ID_TRAY_OPEN  1001
#define ID_TRAY_MODE1 1002
#define ID_TRAY_MODE2 1003
#define ID_TRAY_MODE3 1004
#define ID_TRAY_EXIT  1005

static NOTIFYICONDATAW g_nid = {};
