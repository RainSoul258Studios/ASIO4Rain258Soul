#define MyAppName "ASIO4Rain258Soul"
#define MyAppFullTitle "ASIO4Rain258Soul // CYBERPUNK OVERLORD MASTER ADMIN"
#define MyAppVersion "2.0.258"
#define MyAppPublisher "RainSoul258Studios / ASIO4Rain258Soul Audio Lab (Nilton258)"
#define MyApoClsid "{8F258A10-4D3E-4B9A-9C12-FA0258003D01}"

[Setup]
AppId={{8F258A10-4D3E-4B9A-9C12-FA0258003D01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppFullTitle} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName=D:\{#MyAppName}
DefaultGroupName={#MyAppName}
MinVersion=10.0.22000
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
OutputDir=installer_out
OutputBaseFilename=ASIO4Rain258Soul_InnoWizard_Win11_x64_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
LicenseFile=LICENSE

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs
Source: "web\*"; DestDir: "{app}\web"; Flags: ignoreversion recursesubdirs
Source: "bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs
Source: "driver\*"; DestDir: "{app}\driver"; Flags: ignoreversion recursesubdirs
Source: "scripts\*"; DestDir: "{app}\scripts"; Flags: ignoreversion recursesubdirs
Source: "installer_out\ASIO4Rain258Soul_Setup.exe"; DestDir: "{app}\installer_out"; Flags: ignoreversion

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM asio258.exe /IM ASIO4Rain258SoulBridge.exe /IM ASIO4Rain258Soul_Setup.exe"; Flags: runhidden; RunOnceId: "KillAllAsio258Tasks"