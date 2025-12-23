; Script Inno Setup per TaoGEST - Gestione Terapia Anticoagulante Orale
; Versione 1.2.3.1

#define MyAppName "TaoGEST"
#define MyAppVersion "1.2.4.0"
#define MyAppPublisher "dr Dario Giorgio Zani"
#define MyAppURL "https://dottorgiorgio.it"
#define MyAppExeName "WarfarinManager.UI.exe"
#define MyAppDescription "Gestione Terapia Anticoagulante Orale"

[Setup]
; NOTE: Il valore di AppId identifica univocamente questa applicazione
AppId={{7F8C9D2E-4B5A-6C3D-8E9F-1A2B3C4D5E6F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE.txt
InfoBeforeFile=..\docs\ReleaseNotes.txt
OutputDir=..\publish
OutputBaseFilename=TaoGEST-Setup-v{#MyAppVersion}
SetupIconFile=..\src\WarfarinManager.UI\Resources\Icons\taogest.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Eseguibile principale e tutte le DLL
Source: "..\src\WarfarinManager.UI\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Non usare "Flags: ignoreversion" su file condivisi di sistema

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Rimosso il controllo .NET Runtime perché l'applicazione è pubblicata come self-contained
// e include già tutti i runtime necessari

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Qui si potrebbero eseguire operazioni post-installazione
    // Es: inizializzazione database, creazione cartelle utente, etc.
  end;
end;

function InitializeUninstall(): Boolean;
var
  Response: Integer;
begin
  Response := MsgBox('Vuoi mantenere il database e i dati dell''applicazione?' + #13#10 +
                     'Seleziona Sì per mantenere i dati, No per eliminarli completamente.',
                     mbConfirmation, MB_YESNO);

  if Response = IDNO then
  begin
    // Elimina il database utente
    DelTree(ExpandConstant('{localappdata}\WarfarinManager'), True, True, True);
  end;

  Result := True;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
