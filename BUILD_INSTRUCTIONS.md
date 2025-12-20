# TaoGEST - Build e Packaging per Distribuzione

## 🎯 Obiettivo

Creare un pacchetto distribuibile di TaoGEST per installazione su PC Windows.

---

## ⚙️ Prerequisiti

- .NET 8.0 SDK installato
- PowerShell 5.1+ (già presente in Windows)
- Spazio disco: ~500 MB

---

## 📦 Opzioni di Distribuzione

### Opzione 1: Self-Contained (Consigliato per Beta)

**Vantaggi**:
- Include .NET runtime → utente NON deve installare .NET
- Funziona su qualsiasi Windows 10/11

**Svantaggio**:
- Dimensione ~150 MB

### Opzione 2: Framework-Dependent

**Vantaggi**:
- Pacchetto piccolo ~5 MB

**Svantaggio**:
- Richiede .NET 8.0 runtime sul PC target

---

## 🚀 Procedura Build Release

### Step 1: Pulizia

```powershell
cd D:\Claude\winTaoGest

# Pulisci build cache
dotnet clean

# Rimuovi bin/obj (opzionale)
Get-ChildItem -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

### Step 2: Restore

```powershell
dotnet restore
```

### Step 3: Build Release

```powershell
dotnet build -c Release --no-restore
```

Verifica: File EXE in `src\WarfarinManager.UI\bin\Release\net8.0-windows\`

---

## 📦 Publish Self-Contained

### Comando Completo

```powershell
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o publish/TaoGEST-v1.0.0-beta
```

### Parametri Spiegati

| Parametro | Significato |
|-----------|-------------|
| `-c Release` | Build ottimizzata |
| `-r win-x64` | Target Windows 64-bit |
| `--self-contained true` | Include .NET runtime |
| `-p:PublishSingleFile=true` | File EXE unico |
| `-p:EnableCompressionInSingleFile=true` | Comprimi file |
| `-o publish/...` | Directory output |

### Output

```
publish/TaoGEST-v1.0.0-beta/
├── WarfarinManager.UI.exe  (~150 MB)
└── WarfarinManager.UI.pdb  (debug symbols, opzionale)
```

### Test

```powershell
.\publish\TaoGEST-v1.0.0-beta\WarfarinManager.UI.exe
```

Verifica:
- Applicazione si avvia
- Status bar: "Versione 1.0.0.0 - Beta"
- Nessun errore

---

## 📋 Creazione Pacchetto Distribuzione

### Struttura Consigliata

```
TaoGEST-v1.0.0-beta/
├── WarfarinManager.UI.exe
├── README.txt
├── LICENSE.txt
└── database/
    └── warfarin.db (opzionale)
```

### Script Automatico

```powershell
# Crea struttura
$packageDir = ".\publish\TaoGEST-v1.0.0-beta-package"
New-Item -ItemType Directory -Force -Path $packageDir
New-Item -ItemType Directory -Force -Path "$packageDir\database"

# Copia EXE
Copy-Item ".\publish\TaoGEST-v1.0.0-beta\WarfarinManager.UI.exe" $packageDir

# Crea README.txt
@"
TaoGEST - Gestione Terapia Anticoagulante Orale
Versione: 1.0.0.0 Beta

REQUISITI:
- Windows 10 (1809+) o Windows 11
- 2 GB RAM
- 500 MB spazio disco

INSTALLAZIONE:
1. Estrarre tutti i file in C:\TaoGEST
2. Eseguire WarfarinManager.UI.exe
3. Al primo avvio verrà creato il database

SUPPORTO:
Email: [da definire]

© 2025 Studio Medico
"@ | Out-File -FilePath "$packageDir\README.txt" -Encoding UTF8

Write-Host "✓ Pacchetto creato in: $packageDir" -ForegroundColor Green
```

---

## 🗜️ Creazione ZIP

### PowerShell

```powershell
$version = "1.0.0-beta"
$source = ".\publish\TaoGEST-v1.0.0-beta-package"
$destination = ".\TaoGEST-v$version-win-x64.zip"

Compress-Archive -Path "$source\*" -DestinationPath $destination -Force

Write-Host "✓ ZIP creato: $destination" -ForegroundColor Green
```

### 7-Zip (migliore compressione)

```powershell
& "C:\Program Files\7-Zip\7z.exe" a -tzip `
    "TaoGEST-v1.0.0-beta-win-x64.zip" `
    ".\publish\TaoGEST-v1.0.0-beta-package\*"
```

---

## ✅ Checklist Pre-Distribuzione

### Build
- [ ] `dotnet clean` eseguito
- [ ] `dotnet build -c Release` senza errori
- [ ] `dotnet publish` completato
- [ ] File EXE funzionante

### Test
- [ ] Applicazione si avvia
- [ ] Database si crea al primo avvio
- [ ] Funzionalità principali OK:
  - [ ] Creazione paziente
  - [ ] Inserimento controllo INR
  - [ ] Calcolo dosaggio
  - [ ] Grafico trend
  - [ ] Export TXT

### Packaging
- [ ] README.txt incluso
- [ ] LICENSE.txt incluso
- [ ] ZIP creato
- [ ] Dimensione verificata (~150 MB)

### Distribuzione
- [ ] Test su Windows 10 pulito
- [ ] Test su Windows 11 pulito
- [ ] Antivirus non blocca EXE

---

## 🎯 Distribuzione Beta

### 1. Crea ZIP finale
```powershell
.\publish\TaoGEST-v1.0.0-beta-win-x64.zip
```

### 2. Upload su cloud
- Google Drive
- OneDrive
- Dropbox

### 3. Istruzioni per beta tester

```
Link download: [URL]

Istruzioni:
1. Scarica ZIP
2. Estrai in C:\TaoGEST
3. Esegui WarfarinManager.UI.exe
4. Segui wizard primo avvio
```

---

## 🐛 Troubleshooting

### Errore: SDK non trovato

```powershell
# Verifica versione
dotnet --version

# Deve essere 8.0.x+
# Download: https://dotnet.microsoft.com/download
```

### Errore: NuGet packages

```powershell
# Pulisci cache
dotnet nuget locals all --clear

# Restore
dotnet restore --force
```

### Errore: File in uso

```powershell
# Chiudi Visual Studio
Get-Process | Where-Object {$_.ProcessName -like "*Warfarin*"} | Stop-Process -Force

# Riprova
dotnet build -c Release
```

---

## 📊 Dimensioni File Attese

| Tipo | Dimensione |
|------|-----------|
| Self-Contained EXE | ~150 MB |
| ZIP Self-Contained | ~80 MB (compresso) |
| Framework-Dependent | ~25 MB |

---

**Versione documento**: 1.0  
**Ultimo aggiornamento**: 26 Novembre 2025
