# TaoGEST - Guida Rapida Versioning

## 🎯 Aggiornare la Versione

```powershell
cd D:\Claude\winTaoGest
.\Update-Version.ps1 -NewVersion "1.1.0.0"
```

## Schema Versioning

```
MAJOR.MINOR.PATCH.BUILD
  1  .  0  .  0  .  0

MAJOR  → Breaking changes, riscritture importanti  
MINOR  → Nuove funzionalità backward-compatible
PATCH  → Bug fix, correzioni minori
BUILD  → Auto-increment (CI/CD)
```

## ⚡ Esempi Rapidi

### Bug Fix (PATCH)
```powershell
.\Update-Version.ps1 -NewVersion "1.0.1.0"
git commit -am "fix: corretto calcolo INR"
git tag -a v1.0.1 -m "Bug fix"
git push --all && git push --tags
```

### Nuova Feature (MINOR)
```powershell
.\Update-Version.ps1 -NewVersion "1.1.0.0"
git commit -am "feat: export Excel"
git tag -a v1.1.0 -m "Nuova feature"
git push --all && git push --tags
```

### Breaking Change (MAJOR)
```powershell
.\Update-Version.ps1 -NewVersion "2.0.0.0"
git commit -am "feat!: nuovo database schema"
git tag -a v2.0.0 -m "Major release"
git push --all && git push --tags
```

## 🔍 Verifica Versione

```powershell
# Verifica Version.props
Get-Content Version.props | Select-String "Version"

# Output atteso:
# <VersionPrefix>1.0.0</VersionPrefix>
# <AssemblyVersion>1.0.0.0</AssemblyVersion>
# <FileVersion>1.0.0.0</FileVersion>
```

## ⚠️ Troubleshooting

### Script non si avvia
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\Update-Version.ps1 -NewVersion "1.0.0.0"
```

### Versione non aggiornata dopo build
```powershell
dotnet clean
Remove-Item -Recurse -Force bin, obj
dotnet build -c Release
```

## 📚 Documentazione Completa

Vedi `docs/VERSIONING.md` per informazioni dettagliate.

---

**Versione documento**: 1.0  
**Ultimo aggiornamento**: 26 Novembre 2025
