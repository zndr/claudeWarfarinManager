# TaoGEST

## Gestione Terapia Anticoagulante Orale (TAO) con Warfarin

Applicazione desktop Windows per medici di medicina generale italiani.

### Struttura Progetto

```
TaoGEST/
├── src/
│   ├── WarfarinManager.UI/          # WPF Application
│   ├── WarfarinManager.Core/        # Business Logic
│   ├── WarfarinManager.Data/        # Data Access Layer
│   └── WarfarinManager.Shared/      # Shared Models & Enums
├── tests/
│   └── WarfarinManager.Tests/       # Unit Tests
└── docs/                             # Documentation
```

### Requisiti

- .NET 8.0 SDK
- Visual Studio 2022 (17.8+)
- Windows 10/11

### Build

```bash
dotnet restore
dotnet build
```

### Run

```bash
dotnet run --project src/WarfarinManager.UI/WarfarinManager.UI.csproj
```

### Stato Sviluppo

**Fase 1: Foundation** ✅ (Completata)
- [x] Solution structure
- [x] Database entities
- [x] DbContext configuration
- [x] Enums & Constants
- [x] Base WPF app skeleton

**Fase 2: Core Business Logic** ✅ (Completata)
- [x] DosageCalculatorService
- [x] TTRCalculatorService
- [x] InteractionCheckerService
- [x] Seeding dati lookup

**Fase 3: User Interface WPF** ✅ (Completata)
- [x] Dashboard pazienti
- [x] Gestione controlli INR
- [x] Grafici trend INR
- [x] Gestione farmaci e interazioni
- [x] Export e reporting

### Tecnologie

- **.NET 8.0** con C# 12
- **WPF** per UI
- **Entity Framework Core 8** + SQLite
- **CommunityToolkit.Mvvm**
- **LiveCharts2** per grafici
- **QuestPDF** per export PDF
- **xUnit** per testing

### Linee Guida Implementate

- FCSA-SIMG (Italia) 2024
- ACCP/ACC (USA) 2012

---

**Versione**: 1.0.0.0 - Beta  
**Ultimo aggiornamento**: 26 Novembre 2025
