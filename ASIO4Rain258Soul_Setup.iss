[Setup]
AppId={{8F258A10-4D3E-4B9A-9C12-FA0258003D01}
AppName=ASIO4Rain258Soul
AppVersion=1.0.258
DefaultDirName={autopf}\ASIO4Rain258Soul
OutputDir=installer_out
OutputBaseFilename=ASIO4Rain258Soul_Win11_x64_Setup_v1.0.258
WizardStyle=modern
LicenseFile=LICENSE
PrivilegesRequired=admin

[Files]
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs
Source: "web\*"; DestDir: "{app}\web"; Flags: ignoreversion recursesubdirs
Source: "bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs
Source: "driver\*"; DestDir: "{app}\driver"; Flags: ignoreversion recursesubdirs
Source: "scripts\*"; DestDir: "{app}\scripts"; Flags: ignoreversion recursesubdirs

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM asio258.exe /IM ASIO4Rain258SoulBridge.exe /IM ASIO4Rain258Soul_Setup.exe"; Flags: runhidden; RunOnceId: "KillAllAsio258Tasks"
Filename: "{app}\Uninstall-ASIO4Rain258Soul.bat"; Parameters: "--silent"; Flags: runhidden waituntilterminated; RunOnceId: "CleanRestoreWin11Audio"