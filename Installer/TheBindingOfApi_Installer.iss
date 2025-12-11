; ================================
; The Binding of API - Installer Script
; Para usar con Inno Setup 6.x
; ================================

#define MyAppName "The Binding of API"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Tu Nombre o Estudio"
#define MyAppURL "https://tu-sitio-web.com"
#define MyAppExeName "My project.exe"

; ⚠️ IMPORTANTE: Cambia esta ruta a donde exportaste tu build de Unity
#define BuildFolder "C:\Users\yisus\Downloads\compilado"

[Setup]
; Información básica del instalador
AppId={{463F0238-3B6C-4402-B5CE-EA18BDBBF2AF}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Carpeta de instalación por defecto
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; Configuración del instalador
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=TheBindingOfApi_Setup_v{#MyAppVersion}
SetupIconFile={#BuildFolder}\TheBindingIcon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

; Requisitos del sistema
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Información de la versión
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Installer
VersionInfoCopyright=Copyright (C) 2024 {#MyAppPublisher}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Incluye todos los archivos del build de Unity
Source: "{#BuildFolder}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTA: No uses "Flags: ignoreversion" en archivos compartidos del sistema

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Limpia archivos creados por el juego (saves, config, etc.)
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}"
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}"
