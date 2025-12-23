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

## PREPARAZIONE RELEASE AUTOMATIZZATA ⚡

**USA LO SCRIPT AUTOMATICO** per evitare errori e risparmiare tempo:

```powershell
# Script completo di preparazione release
.\scripts\Prepare-Release.ps1 -NewVersion "1.3.0.0"

# Lo script chiederà interattivamente le release notes
# Oppure passale direttamente:
.\scripts\Prepare-Release.ps1 -NewVersion "1.3.0.0" -ReleaseNotes "Nuove funzionalità..."
```

### Cosa fa automaticamente lo script:

✅ Aggiorna tutti i file di versione (`Version.props`, `TaoGEST-Setup.iss`)
✅ Aggiorna `ReleaseNotes.txt` e `CHANGELOG.md`
✅ Esegue build e publish
✅ Compila l'installer con Inno Setup
✅ Calcola SHA256 hash
✅ Aggiorna `version.json` con tutte le info
✅ Fornisce i comandi pronti per commit, tag e GitHub release

### Dopo lo script, esegui manualmente:

```bash
# 1. Verifica modifiche
git status

# 2. Commit e push
git add -A
git commit -m "chore: Preparazione release vX.X.X.X"
git push

# 3. Crea tag
git tag -a vX.X.X.X -m "Release vX.X.X.X"
git push origin vX.X.X.X

# 4. Crea release GitHub (il comando esatto viene fornito dallo script)
gh release create vX.X.X.X publish/TaoGEST-Setup-vX.X.X.X.exe --title "TaoGEST vX.X.X.X" --notes "..."

# 5. Testa auto-updater
# - Apri TaoGEST
# - Vai in Strumenti > Verifica aggiornamenti
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
