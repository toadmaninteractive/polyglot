; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Polyglot"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Toadman Interactive"
#define MyAppExeName "Polyglot.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{06FA506B-AEBF-4C1F-AB5C-1922103F3ACC}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppPublisher}\{#MyAppName}
OutputBaseFilename=polyglot_setup
SetupIconFile=..\Polyglot\Polyglot.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\Artplant.CouchDB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\Artplant.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\Polyglot.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\rev.conf"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall
