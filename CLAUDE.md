# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TaoGEST (Gestione Terapia Anticoagulante Orale) is a Windows desktop application for Italian general practitioners to manage Warfarin-based oral anticoagulant therapy. Built with .NET 8, WPF, and Entity Framework Core with SQLite.

## Build Commands

```bash
# Restore and build
dotnet restore
dotnet build

# Run the application
dotnet run --project src/WarfarinManager.UI/WarfarinManager.UI.csproj

# Build release
dotnet build -c Release

# Run tests
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~DosageCalculatorServiceTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~DosageCalculatorServiceTests.CalculateDosage_WithStableINR_ReturnsStableDose"

# Publish self-contained executable
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Architecture

### Project Structure

```
src/
├── WarfarinManager.UI/        # WPF Application (entry point)
├── WarfarinManager.Core/      # Business logic services
├── WarfarinManager.Data/      # Data access layer (EF Core + SQLite)
└── WarfarinManager.Shared/    # Shared enums, constants, models
tests/
└── WarfarinManager.Tests/     # xUnit tests with FluentAssertions, Moq
```

### Layer Dependencies

- **UI** → Core, Data, Shared
- **Core** → Data, Shared
- **Data** → Shared
- **Shared** → (no dependencies)

### Core Services (WarfarinManager.Core/Services/)

- `DosageCalculatorService` - Calculates warfarin dosage based on INR values
- `TTRCalculatorService` - Time in Therapeutic Range calculations
- `InteractionCheckerService` - Drug interaction warnings
- `BridgeTherapyService` - Bridge therapy planning for surgery
- `SwitchCalculatorService` - DOAC-Warfarin therapy switching

### Data Layer (WarfarinManager.Data/)

- `WarfarinDbContext` - Main EF Core context with SQLite
- Entities: `Patient`, `INRControl`, `Medication`, `Indication`, `AdverseEvent`, `BridgeTherapyPlan`, `TherapySwitch`, `PreTaoAssessment`
- Uses soft delete for patients (`IsDeleted` flag with global query filter)
- Database stored at: `%LocalAppData%/WarfarinManager/warfarin.db`

### UI Pattern (WarfarinManager.UI/)

- MVVM with CommunityToolkit.Mvvm
- Dependency injection via Microsoft.Extensions.Hosting
- Views in `Views/` folder, ViewModels in `ViewModels/` folder
- Charts: LiveCharts2 for INR trend visualization
- PDF export: QuestPDF

## Versioning

All projects import `Version.props` for centralized version management. Use the update script:

```powershell
.\Update-Version.ps1 -NewVersion "1.2.0.0"
```

## 🚀 PROCESSO DI RELEASE - GUIDA DEFINITIVA

> 📖 **Per Claude Code**: Leggi `docs/RELEASE-PROCESS-CLAUDE.md` per istruzioni dettagliate e troubleshooting completo.

### ⚠️ REGOLA FONDAMENTALE

**USARE SEMPRE E SOLO LO SCRIPT AUTOMATICO `Prepare-Release.ps1`**

Non fare MAI release manuali. Lo script gestisce automaticamente tutti i passaggi critici e previene errori.

---

### 📋 PROCEDURA STANDARD (3 PASSI)

#### PASSO 1: Esegui lo script automatico

```powershell
.\scripts\Prepare-Release.ps1 -NewVersion "1.3.0.0"
```

Lo script chiederà interattivamente le release notes, oppure passale direttamente:

```powershell
.\scripts\Prepare-Release.ps1 -NewVersion "1.3.0.0" -ReleaseNotes "Nuove funzionalità..."
```

#### PASSO 2: Verifica e Commit

```bash
# Verifica modifiche
git status

# Commit e push
git add -A
git commit -m "chore: Preparazione release vX.X.X.X"
git push
```

#### PASSO 3: Pubblica su GitHub

```bash
# Crea tag
git tag -a vX.X.X.X -m "Release vX.X.X.X"
git push origin vX.X.X.X

# Crea release GitHub (usa il comando fornito dallo script)
gh release create vX.X.X.X publish/TaoGEST-Setup-vX.X.X.X.exe --title "TaoGEST vX.X.X.X" --notes "..."
```

#### PASSO 4: Verifica Auto-Updater

- Apri TaoGEST
- Vai in Strumenti > Verifica aggiornamenti
- Controlla che rilevi la nuova versione

---

### ✅ Cosa fa automaticamente lo script

Lo script `Prepare-Release.ps1` esegue **TUTTI** i seguenti passaggi in modo automatico e verificato:

1. ✅ Aggiorna `Version.props` (AssemblyVersion, FileVersion, Version)
2. ✅ Aggiorna `installer/TaoGEST-Setup.iss` (MyAppVersion)
3. ✅ Aggiorna `docs/ReleaseNotes.txt` con la nuova sezione
4. ✅ Aggiorna `CHANGELOG.md` con la nuova sezione
5. ✅ Esegue `dotnet clean -c Release`
6. ✅ Esegue `dotnet restore`
7. ✅ Esegue `dotnet build -c Release`
8. ✅ **CRITICO**: Esegue `dotnet publish` nella cartella predefinita (NON in una custom!)
9. ✅ **VERIFICA**: Controlla che il binario pubblicato abbia la versione corretta
10. ✅ Compila l'installer con Inno Setup
11. ✅ Calcola SHA256 hash dell'installer
12. ✅ Aggiorna `version.json` con versione, URL, data, hash, dimensione e note
13. ✅ Fornisce i comandi git pronti per copia-incolla

---

### 🔴 ERRORI COMUNI DA EVITARE

#### ❌ ERRORE #1: Pubblicare in una cartella custom

**SBAGLIATO:**
```powershell
dotnet publish -o publish/custom/path
```

**CONSEGUENZA**: L'installer Inno Setup leggerà i binari dalla cartella predefinita `src/WarfarinManager.UI/bin/Release/net8.0-windows/win-x64/publish/` e troverà binari vecchi o inesistenti!

**CORRETTO:**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true
# NON specificare -o !
```

#### ❌ ERRORE #2: Non verificare la versione dei binari

Dopo il publish, verificare SEMPRE:

```powershell
(Get-Item 'src\WarfarinManager.UI\bin\Release\net8.0-windows\win-x64\publish\WarfarinManager.UI.exe').VersionInfo.FileVersion
```

Deve corrispondere alla versione target!

#### ❌ ERRORE #3: Ricompilare l'installer senza rifare il publish

Se modifichi il codice, devi:
1. `dotnet clean -c Release`
2. `dotnet publish` (sempre!)
3. Compilare l'installer

Mai saltare il paso 2!

#### ❌ ERRORE #4: Dimenticare di fare clean prima del publish

I binari vecchi potrebbero rimanere nella cartella di output. Sempre fare:

```powershell
dotnet clean -c Release
```

---

### 📁 STRUTTURA FILE DI VERSIONING

**File aggiornati automaticamente:**

1. **`Version.props`** - Versione centralizzata .NET
   - Percorso: root del progetto
   - Proprietà: `AssemblyVersion`, `FileVersion`, `VersionPrefix`

2. **`installer/TaoGEST-Setup.iss`** - Script Inno Setup
   - Riga 5: `#define MyAppVersion "X.X.X.X"`

3. **`version.json`** - Metadata per auto-updater
   - Campi: Version, DownloadUrl, ReleaseDate, FileSize, Sha256Hash, ReleaseNotes
   - URL GitHub: `https://github.com/zndr/claudeWarfarinManager/releases/download/vX.X.X.X/TaoGEST-Setup-vX.X.X.X.exe`

4. **`docs/ReleaseNotes.txt`** - Note mostrate durante installazione
   - Encoding: Windows-1252 (ANSI) per compatibilità Inno Setup

5. **`CHANGELOG.md`** - Storico versioni
   - Encoding: UTF-8

**File con versione auto-aggiornata (binding):**

6. **`src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml`**
   - Legge versione da: `Assembly.GetExecutingAssembly().GetName().Version`

7. **`src/WarfarinManager.UI/MainWindow.xaml`**
   - Binding dal ViewModel

---

### 🔍 VERIFICA POST-RELEASE

Dopo aver pubblicato su GitHub, verifica:

1. ✅ Download dell'installer funzionante
2. ✅ Installazione corretta
3. ✅ Versione visualizzata nell'app corrisponde
4. ✅ Auto-updater rileva la nuova versione
5. ✅ Hash SHA256 corrisponde a quello in `version.json`

**Comando per verificare hash:**

```powershell
(Get-FileHash -Path "TaoGEST-Setup-vX.X.X.X.exe" -Algorithm SHA256).Hash
```

## CHECKLIST RELEASE (RIFERIMENTO MANUALE)

<details>
<summary>File aggiornati automaticamente dallo script Prepare-Release.ps1</summary>

**Quando si crea una nuova release, questi file vengono aggiornati automaticamente:**

1. **`Version.props`** - Versione centralizzata .NET
   - `AssemblyVersion`, `FileVersion`, `Version`

2. **`version.json`** - File remoto per update checker
   - `Version`, `DownloadUrl`, `ReleaseDate`, `FileSize`, `Sha256Hash`, `ReleaseNotes`

3. **`installer/TaoGEST-Setup.iss`** - Script Inno Setup
   - `MyAppVersion` (riga 5)

4. **`docs/ReleaseNotes.txt`** - Note di rilascio mostrate durante installazione
   - Aggiunge le note della nuova versione in cima

5. **`CHANGELOG.md`** - Storico versioni
   - Aggiunge sezione per la nuova versione

**File da verificare manualmente (se contengono versioni hardcoded):**

6. **`src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml`** - Dialog "Info su TaoGEST"
   - La versione viene letta da `Assembly.GetExecutingAssembly()`, quindi si aggiorna automaticamente

7. **`src/WarfarinManager.UI/MainWindow.xaml`** - TextBlock versione in basso a sinistra
   - La versione viene bindata dal ViewModel, quindi si aggiorna automaticamente

</details>

## Testing

- xUnit for test framework
- FluentAssertions for assertions
- Moq for mocking
- Microsoft.EntityFrameworkCore.InMemory for database tests

Test files are in `tests/WarfarinManager.Tests/`:
- `Services/` - Unit tests for core services
- `Integration/` - Integration tests with in-memory database

## Key Technical Details

- Target framework: net8.0-windows (UI) / net8.0 (other projects)
- Language: C# 12
- Nullable reference types enabled
- Implicit usings enabled
- Logs stored at: `%LocalAppData%/WarfarinManager/Logs/`
- Clinical guidelines: FCSA-SIMG (Italy) 2024, ACCP/ACC (USA) 2012
