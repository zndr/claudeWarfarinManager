# WarfarinManager Pro

## Gestione Terapia Anticoagulante Orale (TAO) con Warfarin

Applicazione desktop Windows per medici di medicina generale italiani.

### Struttura Progetto

```
WarfarinManager/
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

- .NET 9.0 SDK
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

**Fase 2: Core Business Logic** 🚧 (Prossima)
- [ ] DosageCalculatorService
- [ ] TTRCalculatorService
- [ ] InteractionCheckerService
- [ ] Seeding dati lookup

### Tecnologie

- **.NET 9.0** con C# 13
- **WPF** per UI
- **Entity Framework Core 9** + SQLite
- **CommunityToolkit.Mvvm**
- **xUnit** per testing
- **FluentAssertions**

### Linee Guida Implementate

- FCSA-SIMG (Italia) 2024
- ACCP/ACC (USA) 2012

---

**Versione**: 1.0.0-dev  
**Ultimo aggiornamento**: 22 Novembre 2025
