#define MyAppName "Desklet"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Desklet"
#define MyAppExeName "Desklet.exe"

[Setup]
AppId={{E46D7B23-5D7D-4F2D-8B30-6F4AAB6B7B14}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Desklet
DefaultGroupName=Desklet
OutputDir=..\installer-output
OutputBaseFilename=Desklet-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\Desklet.exe

[Files]
Source: "..\publish\Desklet.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Desklet"; Filename: "{app}\Desklet.exe"; AppUserModelID: "Desklet.Desktop"
Name: "{autodesktop}\Desklet"; Filename: "{app}\Desklet.exe"; Tasks: desktopicon; AppUserModelID: "Desklet.Desktop"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Run]
Filename: "{app}\Desklet.exe"; Description: "Launch Desklet"; Flags: postinstall nowait skipifsilent