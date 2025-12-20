# Sistema di Versioning - TaoGEST

## Panoramica

TaoGEST utilizza **Semantic Versioning 2.0.0** (MAJOR.MINOR.PATCH.BUILD).

### Schema

```
MAJOR.MINOR.PATCH.BUILD
  │      │      │     │
  │      │      │     └─── Build (CI/CD)
  │      │      └───────── Patch: bug fix
  │      └──────────────── Minor: nuove funzionalità
  └─────────────────────── Major: breaking changes
```

**Esempio**: `1.2.3.0`
- **1** = Major (prima versione stabile)
- **2** = Minor (seconda release feature)
- **3** = Patch (terza correzione bug)
- **0** = Build (gestito da CI/CD)

---

## File di Configurazione

### Version.props

File XML centralizzato:

```xml
<Project>
  <PropertyGroup>
    <VersionPrefix>1.0.0</VersionPrefix>
    <VersionSuffix></VersionSuffix>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    
    <Product>TaoGEST</Product>
    <Company>Studio Medico</Company>
    <Copyright>Copyright © 2025</Copyright>
  </PropertyGroup>
</Project>
```

### Import nei progetti

Ogni `.csproj` importa:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="..\..\Version.props" />
  <PropertyGroup>
    <!-- configurazione -->
  </PropertyGroup>
</Project>
```

---

## Script Update-Version.ps1

### Uso Base

```powershell
# Aggiorna versione
.\Update-Version.ps1 -NewVersion "1.1.0.0"

# Con suffisso (beta, rc1)
.\Update-Version.ps1 -NewVersion "2.0.0.0" -VersionSuffix "beta"
```

### Cosa Fa

1. Aggiorna `Version.props`
2. Aggiorna `MainWindow.xaml` (status bar)
3. Aggiorna `README.md`
4. Cerca `AssemblyInfo.cs` (se presente)

### Output

```
=======================================
  Aggiornamento Versione TaoGEST
=======================================

Nuova versione: 1.1.0.0

[1/4] Aggiornamento Version.props...
   ✓ Version.props aggiornato
[2/4] Aggiornamento MainWindow.xaml...
   ✓ MainWindow.xaml aggiornato
[3/4] Aggiornamento README.md...
   ✓ README.md aggiornato

=======================================
  Aggiornamento Completato!
=======================================

Prossimi passi:
   1. git diff
   2. git commit -am 'chore: bump version to 1.1.0.0'
   3. git tag -a v1.1.0 -m 'Release v1.1.0'
   4. git push && git push --tags
```

---

## Workflow di Release

### 1. Bug Fix (Patch)

```powershell
# Fix bug nel codice
# ...

# Aggiorna PATCH
.\Update-Version.ps1 -NewVersion "1.0.1.0"

# Commit e tag
git add .
git commit -m "fix: corretto calcolo INR"
git tag -a v1.0.1 -m "Hotfix v1.0.1"
git push --all && git push --tags
```

### 2. Nuova Feature (Minor)

```powershell
# Implementa feature
# ...

# Aggiorna MINOR
.\Update-Version.ps1 -NewVersion "1.1.0.0"

# Commit e tag
git add .
git commit -m "feat: export Excel"
git tag -a v1.1.0 -m "Release v1.1.0"
git push --all && git push --tags
```

### 3. Breaking Change (Major)

```powershell
# Riscrittura importante
# ...

# Aggiorna MAJOR
.\Update-Version.ps1 -NewVersion "2.0.0.0"

# Commit e tag
git add .
git commit -m "feat!: nuovo database schema

BREAKING CHANGE: richiede migrazione"
git tag -a v2.0.0 -m "Release v2.0.0"
git push --all && git push --tags
```

---

## Convenzioni Git

### Commit Messages (Conventional Commits)

```
<type>: <description>

[optional body]
[optional footer]
```

**Types**:
- `feat`: Nuova feature (minor bump)
- `fix`: Bug fix (patch bump)
- `chore`: Manutenzione
- `docs`: Documentazione
- `refactor`: Refactoring
- `test`: Test

**Esempi**:
```bash
git commit -m "feat: aggiunto grafico TTR"
git commit -m "fix: corretto calcolo dosaggio"
git commit -m "chore: bump version to 1.2.0.0"
```

### Tag Format

```
v<MAJOR>.<MINOR>.<PATCH>[-<SUFFIX>]
```

**Esempi**:
- `v1.0.0` - Release stabile
- `v1.1.0` - Feature release
- `v2.0.0-beta` - Beta release
- `v2.0.0-rc1` - Release candidate

---

## Esempi Pratici

### Scenario 1: Bug Urgente

```powershell
# Versione corrente: 1.0.0.0

# 1. Fix bug
# 2. Aggiorna patch
.\Update-Version.ps1 -NewVersion "1.0.1.0"

# 3. Commit
git commit -am "fix: corretto INR per target 2.5-3.5"

# 4. Tag
git tag -a v1.0.1 -m "Hotfix v1.0.1"

# 5. Deploy
git push --all && git push --tags
```

### Scenario 2: Nuova Feature

```powershell
# Versione corrente: 1.0.1.0

# 1. Sviluppa feature
# 2. Aggiorna minor
.\Update-Version.ps1 -NewVersion "1.1.0.0"

# 3. Commit
git commit -am "feat: export PDF"

# 4. Tag
git tag -a v1.1.0 -m "Release v1.1.0"

# 5. Push
git push --all && git push --tags
```

### Scenario 3: Beta Release

```powershell
# Versione corrente: 1.1.0.0

# 1. Beta
.\Update-Version.ps1 -NewVersion "2.0.0.0" -VersionSuffix "beta"

# 2. Tag
git commit -am "chore: release 2.0.0-beta"
git tag -a v2.0.0-beta -m "Beta v2.0.0"

# 3. Dopo testing → stable
.\Update-Version.ps1 -NewVersion "2.0.0.0"
git commit -am "chore: release 2.0.0 stable"
git tag -a v2.0.0 -m "Release v2.0.0"
```

---

## Checklist Pre-Release

- [ ] Test passano (`dotnet test`)
- [ ] Build senza warning
- [ ] README aggiornato
- [ ] Versione aggiornata
- [ ] Build Release testato
- [ ] Tag Git creato

---

## Troubleshooting

### Script non si avvia

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\Update-Version.ps1 -NewVersion "1.0.0.0"
```

### Versione non aggiorna nell'UI

```powershell
dotnet clean
dotnet build -c Release
```

### Versione diversa tra file

```powershell
# Ri-esegui script
.\Update-Version.ps1 -NewVersion "1.0.0.0"
```

---

## Riferimenti

- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Tagging](https://git-scm.com/book/en/v2/Git-Basics-Tagging)

---

**Versione documento**: 1.0  
**Ultimo aggiornamento**: 26 Novembre 2025
