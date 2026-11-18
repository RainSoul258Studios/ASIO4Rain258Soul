@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion
set "PROJECT_DIR=%~dp0"
cd /d "%PROJECT_DIR%"
cmake -B "%PROJECT_DIR%build" -S "%PROJECT_DIR%" -G "Visual Studio 17 2022" -A x64
cmake --build "%PROJECT_DIR%build" --config Release --parallel
if exist "%PROJECT_DIR%build\Release\ASIO4Rain258SoulAPO.dll" (
    copy /Y "%PROJECT_DIR%build\Release\ASIO4Rain258SoulAPO.dll" "%PROJECT_DIR%bin\ASIO4Rain258SoulAPO.dll" >nul
    copy /Y "%PROJECT_DIR%build\Release\ASIO4Rain258SoulBridge.exe" "%PROJECT_DIR%bin\ASIO4Rain258SoulBridge.exe" >nul
    regsvr32 /s "%PROJECT_DIR%bin\ASIO4Rain258SoulAPO.dll"
)
echo [OK] Compilacion finalizada.
