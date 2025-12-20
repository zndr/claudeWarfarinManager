# 📚 TaoGEST - Documentazione Sistema Versioning e Distribuzione

## 🎯 Start Here

### Aggiornare la Versione
```powershell
.\Update-Version.ps1 -NewVersion "1.1.0.0"
```
📖 [Guida Rapida](QUICK_VERSION_GUIDE.md)

### Creare Build per Distribuzione
```powershell
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj `
    -c Release -r win-x64 --self-contained true `
    -o publish/TaoGEST-v1.0.0-beta
```
📖 [Istruzioni Build](BUILD_INSTRUCTIONS.md)

---

## 📋 Documenti Disponibili

### 🚀 Quick Start

| Documento | Descrizione | Quando Usarlo |
|-----------|-------------|---------------|
| [QUICK_VERSION_GUIDE.md](QUICK_VERSION_GUIDE.md) | Guida rapida versioning | Uso quotidiano |
| [SETUP_COMPLETE.md](SETUP_COMPLETE.md) | Riepilogo setup | Verifica configurazione |

### 📖 Documentazione Completa

| Documento | Contenuto |
|-----------|-----------|
| [docs/VERSIONING.md](docs/VERSIONING.md) | Sistema versioning dettagliato (12 sezioni) |
| [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) | Build, packaging, distribuzione |
| [docs/RELEASE_NOTES_v1.0.0.md](docs/RELEASE_NOTES_v1.0.0.md) | Note di rilascio Beta |

### 🛠️ Script Utility

| Script | Funzione | Uso |
|--------|----------|-----|
| `Update-Version.ps1` | Aggiorna versione automaticamente | `.\Update-Version.ps1 -NewVersion "1.1.0.0"` |
| `Test-Versioning.ps1` | Verifica configurazione | `.\Test-Versioning.ps1` |

---

## 🎓 Workflow Tipici

### Bug Fix (incrementa PATCH)

```powershell
# 1. Fix il bug
# 2. Aggiorna versione
.\Update-Version.ps1 -NewVersion "1.0.1.0"

# 3. Commit e tag
git add .
git commit -m "fix: corretto calcolo INR per edge case"
git tag -a v1.0.1 -m "Hotfix v1.0.1"
git push --all && git push --tags
```

### Nuova Feature (incrementa MINOR)

```powershell
# 1. Implementa feature
# 2. Aggiorna versione
.\Update-Version.ps1 -NewVersion "1.1.0.0"

# 3. Commit e tag
git add .
git commit -m "feat: aggiunto export Excel"
git tag -a v1.1.0 -m "Release v1.1.0"
git push --all && git push --tags
```

### Build per Distribuzione

```powershell
# 1. Build Release
dotnet clean
dotnet build -c Release

# 2. Publish self-contained
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -o publish/TaoGEST-v1.0.0-beta

# 3. Crea ZIP
Compress-Archive -Path "publish/TaoGEST-v1.0.0-beta/*" `
    -DestinationPath "TaoGEST-v1.0.0-beta-win-x64.zip"
```

---

## 📊 Struttura Sistema

```
TaoGEST/
├── Version.props                    ← Versione centralizzata
│
├── src/
│   ├── WarfarinManager.UI/
│   │   ├── *.csproj                 ← Importa Version.props
│   │   └── MainWindow.xaml          ← Mostra versione
│   ├── WarfarinManager.Core/
│   ├── WarfarinManager.Data/
│   └── WarfarinManager.Shared/
│
├── Update-Version.ps1               ← Script aggiornamento
├── Test-Versioning.ps1              ← Script verifica
│
├── docs/
│   ├── VERSIONING.md
│   └── RELEASE_NOTES_v1.0.0.md
│
├── QUICK_VERSION_GUIDE.md
├── BUILD_INSTRUCTIONS.md
└── README.md
```

---

## 🎯 Versione Corrente

```
Prodotto:  TaoGEST
Versione:  1.0.0.0
Status:    Beta
Target:    Windows 10/11 x64
Framework: .NET 8.0

✅ PRONTO PER DISTRIBUZIONE BETA
```

---

## 📞 Quick Reference

### Versioning

```powershell
# Patch (bug fix): 1.0.0.0 → 1.0.1.0
.\Update-Version.ps1 -NewVersion "1.0.1.0"

# Minor (feature): 1.0.1.0 → 1.1.0.0
.\Update-Version.ps1 -NewVersion "1.1.0.0"

# Major (breaking): 1.9.0.0 → 2.0.0.0
.\Update-Version.ps1 -NewVersion "2.0.0.0"
```

### Build

```powershell
# Build Release
dotnet build -c Release

# Test applicazione
.\src\WarfarinManager.UI\bin\Release\net8.0-windows\WarfarinManager.UI.exe
```

### Git

```powershell
# Verifica modifiche
git diff

# Commit versione
git commit -am "chore: bump version to X.Y.Z.W"

# Tag release
git tag -a vX.Y.Z -m "Release vX.Y.Z"

# Push
git push --all && git push --tags
```

---

## ✅ Checklist

### Prima di Aggiornare Versione

- [ ] Tutti i test passano
- [ ] Build senza errori
- [ ] Modifiche committate
- [ ] Changelog aggiornato

### Prima della Distribuzione

- [ ] Build Release OK
- [ ] Test su Windows pulito
- [ ] Pacchetto ZIP creato
- [ ] Documentazione aggiornata

---

**Ultima revisione**: 26 Novembre 2025  
**Status**: ✅ Sistema operativo
