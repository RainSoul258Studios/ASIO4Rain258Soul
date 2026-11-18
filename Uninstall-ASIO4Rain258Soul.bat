@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

echo ===============================================================================
echo   ASIO4Rain258Soul (asio258) - AGENTE DE DESINSTALACION Y CIERRE DE TAREAS
echo ===============================================================================

set "PROJECT_DIR=%~dp0"
set "APO_CLSID={8F258A10-4D3E-4B9A-9C12-FA0258003D01}"

echo [1/6] Finalizando todas las tareas e instancias activas (Cero Background)...
taskkill /F /IM asio258.exe /T >nul 2>&1
taskkill /F /IM ASIO4Rain258SoulBridge.exe /T >nul 2>&1
taskkill /F /IM ASIO4Rain258Soul_Setup.exe /T >nul 2>&1

echo [2/6] Eliminando cualquier registro de auto-arranque en Windows 11...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "ASIO4Rain258SoulBridge" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "ASIO4Rain258SoulBridge" /f >nul 2>&1

echo [3/6] Liberando audiodg.exe y desregistrando APO COM %APO_CLSID%...
net stop Audiosrv /y >nul 2>&1
net stop AudioEndpointBuilder /y >nul 2>&1
taskkill /F /IM audiodg.exe >nul 2>&1

if exist "%PROJECT_DIR%bin\ASIO4Rain258SoulAPO.dll" regsvr32 /u /s "%PROJECT_DIR%bin\ASIO4Rain258SoulAPO.dll"
if exist "%PROJECT_DIR%build\Release\ASIO4Rain258SoulAPO.dll" regsvr32 /u /s "%PROJECT_DIR%build\Release\ASIO4Rain258SoulAPO.dll"

reg delete "HKCR\CLSID\%APO_CLSID%" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Classes\CLSID\%APO_CLSID%" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Audio\APO\%APO_CLSID%" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\ASIO4Rain258Soul" /f >nul 2>&1

echo [4/6] Limpiando asociaciones FxProperties en Endpoints Hardware y Virtuales...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$g = '%APO_CLSID%'; " ^
    "@('Render','Capture') | ForEach-Object { " ^
    "  $base = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\' + $_; " ^
    "  Get-ChildItem $base -ErrorAction SilentlyContinue | ForEach-Object { " ^
    "    $fx = Join-Path $_.PSPath 'FxProperties'; " ^
    "    if (Test-Path $fx) { " ^
    "      (Get-ItemProperty $fx -ErrorAction SilentlyContinue).PSObject.Properties | Where-Object { $_.Value -match $g } | ForEach-Object { " ^
    "        Remove-ItemProperty -Path $fx -Name $_.Name -Force -ErrorAction SilentlyContinue " ^
    "      } " ^
    "    } " ^
    "  } " ^
    "}"

echo [5/6] Reiniciando servicios nativos de audio de Windows 11...
net start AudioEndpointBuilder >nul 2>&1
net start Audiosrv >nul 2>&1

echo [6/6] Audio nativo de Windows 11 restaurado y 0 procesos en segundo plano.
if /i not "%~1"=="--silent" pause