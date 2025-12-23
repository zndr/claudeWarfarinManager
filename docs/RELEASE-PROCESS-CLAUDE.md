# 🤖 GUIDA RELEASE PER CLAUDE CODE

Questo documento contiene le istruzioni DEFINITIVE per Claude Code quando deve assistere nella creazione di una nuova release di TaoGEST.

---

## ⚠️ REGOLA AUREA

**USARE SEMPRE E SOLO LO SCRIPT `.\scripts\Prepare-Release.ps1`**

**NON fare MAI release manuali.** Lo script automatico previene tutti gli errori critici.

---

## 🎯 PROCEDURA STANDARD (4 PASSI)

Quando l'utente chiede di creare una nuova release, seguire ESATTAMENTE questi passi:

### PASSO 1: Esegui lo script automatico

```powershell
.\scripts\Prepare-Release.ps1 -NewVersion "X.X.X.X"
```

Se l'utente fornisce le release notes, passale direttamente:

```powershell
.\scripts\Prepare-Release.ps1 -NewVersion "X.X.X.X" -ReleaseNotes "Descrizione delle modifiche..."
```

### PASSO 2: Verifica e Commit

```bash
# Verifica modifiche
git status

# Commit e push
git add -A
git commit -m "chore: Preparazione release vX.X.X.X"
git push
```

### PASSO 3: Crea Tag e Release GitHub

```bash
# Crea tag
git tag -a vX.X.X.X -m "Release vX.X.X.X"
git push origin vX.X.X.X

# Crea release GitHub (il comando esatto viene fornito dallo script)
gh release create vX.X.X.X publish/TaoGEST-Setup-vX.X.X.X.exe --title "TaoGEST vX.X.X.X" --notes "..."
```

### PASSO 4: Verifica finale

Chiedere all'utente di verificare:

- Download dell'installer funzionante
- Installazione corretta
- Versione visualizzata nell'app corrisponde
- Auto-updater (Strumenti > Verifica aggiornamenti) rileva la nuova versione

---

## 🔴 ERRORI CRITICI DA NON COMMETTERE MAI

### ❌ ERRORE CRITICO #1: Pubblicare in una cartella custom

**SBAGLIATO:**

```powershell
dotnet publish -o publish/custom/path  # ❌ MAI FARE QUESTO!
```

**PERCHÉ È SBAGLIATO:**

L'installer Inno Setup è configurato per leggere i binari da:

```
src/WarfarinManager.UI/bin/Release/net8.0-windows/win-x64/publish/
```

Se pubblichiamo in una cartella diversa con `-o`, l'installer prenderà binari vecchi o inesistenti!

**CORRETTO:**

```powershell
# NON specificare -o, lasciare che dotnet usi la cartella predefinita
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true
```

Lo script `Prepare-Release.ps1` è stato corretto per evitare questo errore.

### ❌ ERRORE CRITICO #2: Non verificare la versione dei binari

Dopo il publish, SEMPRE verificare che il binario abbia la versione corretta:

```powershell
$exeVersion = (Get-Item 'src\WarfarinManager.UI\bin\Release\net8.0-windows\win-x64\publish\WarfarinManager.UI.exe').VersionInfo.FileVersion

if ($exeVersion -ne "X.X.X.X") {
    Write-Error "ERRORE: Versione binario ($exeVersion) diversa da versione target!"
}
```

Lo script `Prepare-Release.ps1` fa questa verifica automaticamente.

### ❌ ERRORE CRITICO #3: Dimenticare dotnet clean

Se ci sono binari vecchi nella cartella di output, potrebbero finire nell'installer!

**SEMPRE fare:**

```powershell
dotnet clean -c Release
```

Prima di ogni `dotnet publish`.

### ❌ ERRORE CRITICO #4: Hash SHA256 non aggiornato

Se l'installer viene ricompilato, l'hash SHA256 cambia!

**SEMPRE:**

1. Ricompilare installer
2. Ricalcolare hash: `(Get-FileHash -Path "installer.exe" -Algorithm SHA256).Hash`
3. Aggiornare `version.json` con il nuovo hash

Lo script fa tutto automaticamente.

---

## 📋 CHECKLIST DI VERIFICA

Prima di confermare la release, verificare che lo script abbia fatto TUTTI questi passaggi:

- [ ] `Version.props` aggiornato (AssemblyVersion, FileVersion, VersionPrefix)
- [ ] `installer/TaoGEST-Setup.iss` aggiornato (MyAppVersion riga 5)
- [ ] `docs/ReleaseNotes.txt` aggiornato con nuova sezione
- [ ] `CHANGELOG.md` aggiornato con nuova sezione
- [ ] `dotnet clean -c Release` eseguito
- [ ] `dotnet restore` eseguito
- [ ] `dotnet build -c Release` eseguito con successo
- [ ] `dotnet publish` eseguito SENZA `-o` (usa cartella predefinita)
- [ ] Versione del binario pubblicato verificata e corretta
- [ ] Installer compilato con Inno Setup
- [ ] Hash SHA256 calcolato
- [ ] `version.json` aggiornato con: Version, DownloadUrl, ReleaseDate, FileSize, Sha256Hash, ReleaseNotes
- [ ] Installer trovato in `publish/TaoGEST-Setup-vX.X.X.X.exe`

---

## 📁 FILE COINVOLTI NEL PROCESSO

**File che DEVONO essere aggiornati ad ogni release:**

1. **`Version.props`** (root)
   - Campi: `<AssemblyVersion>`, `<FileVersion>`, `<VersionPrefix>`

2. **`installer/TaoGEST-Setup.iss`**
   - Riga 5: `#define MyAppVersion "X.X.X.X"`

3. **`version.json`** (root)
   - Tutti i campi: Version, DownloadUrl, ReleaseDate, FileSize, Sha256Hash, ReleaseNotes

4. **`docs/ReleaseNotes.txt`**
   - Aggiungere nuova sezione in cima

5. **`CHANGELOG.md`**
   - Aggiungere nuova sezione dopo il titolo

**File con versione automatica (NON modificare):**

- `src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml` (usa Assembly.GetExecutingAssembly())
- `src/WarfarinManager.UI/MainWindow.xaml` (binding dal ViewModel)

---

## 🔍 COSA FARE SE QUALCOSA VA STORTO

### Scenario: L'installer ha la versione sbagliata

**Causa:** Binari non ricompilati o pubblicati in cartella sbagliata.

**Soluzione:**

```powershell
# 1. Pulire tutto
dotnet clean -c Release

# 2. Ripubblicare (SENZA -o custom!)
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true

# 3. Verificare versione binario
(Get-Item 'src\WarfarinManager.UI\bin\Release\net8.0-windows\win-x64\publish\WarfarinManager.UI.exe').VersionInfo.FileVersion

# 4. Ricompilare installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\TaoGEST-Setup.iss"

# 5. Ricalcolare hash
$hash = (Get-FileHash -Path "publish\TaoGEST-Setup-vX.X.X.X.exe" -Algorithm SHA256).Hash

# 6. Aggiornare version.json con nuovo hash
```

### Scenario: Release GitHub già creata ma installer sbagliato

**Soluzione:**

```bash
# 1. Eliminare release e tag
gh release delete vX.X.X.X --yes
git tag -d vX.X.X.X
git push origin :refs/tags/vX.X.X.X

# 2. Correggere l'installer (vedi sopra)

# 3. Commit version.json aggiornato
git add version.json
git commit -m "fix: Aggiornato hash installer vX.X.X.X con binari corretti"
git push

# 4. Ricreare tag e release
git tag -a vX.X.X.X -m "Release vX.X.X.X"
git push origin vX.X.X.X
gh release create vX.X.X.X publish/TaoGEST-Setup-vX.X.X.X.exe --title "TaoGEST vX.X.X.X" --notes "..."
```

---

## 💡 BEST PRACTICES

1. **Testare sempre localmente prima di pubblicare**
   - Installare l'installer appena creato
   - Verificare che l'app mostri la versione corretta
   - Testare le funzionalità principali

2. **Tenere release notes chiare e concise**
   - Elencare cosa è stato risolto/aggiunto
   - Menzionare breaking changes se presenti
   - Includere dettagli tecnici rilevanti

3. **Versioning semantico**
   - Formato: `MAJOR.MINOR.PATCH.BUILD`
   - Esempio: `1.2.3.4`
   - MAJOR: Cambiamenti incompatibili
   - MINOR: Nuove funzionalità retrocompatibili
   - PATCH: Bug fix retrocompatibili
   - BUILD: Numero incrementale

4. **Verificare auto-updater dopo ogni release**
   - Aprire una versione precedente di TaoGEST
   - Andare in Strumenti > Verifica aggiornamenti
   - Controllare che rilevi la nuova versione
   - Verificare che il download funzioni

---

## 📞 RIFERIMENTI

- **Script principale**: `scripts/Prepare-Release.ps1`
- **Documentazione utente**: `CLAUDE.md` (sezione "PROCESSO DI RELEASE")
- **Configurazione installer**: `installer/TaoGEST-Setup.iss`
- **Repository GitHub**: `https://github.com/zndr/claudeWarfarinManager`

---

## 🎓 LEZIONI APPRESE

### Incidente v1.2.4.4 (2025-12-23)

**Problema**: Installer conteneva binari della versione 1.2.3.1 invece di 1.2.4.4.

**Causa root**: Lo script `Prepare-Release.ps1` usava `-o publish/` nel comando `dotnet publish`, ma l'installer Inno Setup leggeva da `bin/Release/.../publish/`.

**Fix applicato**:
- Rimosso parametro `-o` da `dotnet publish`
- Aggiunta verifica automatica della versione del binario
- Documentato errore in questa guida

**Prevenzione**: Lo script ora verifica SEMPRE che il binario pubblicato abbia la versione corretta prima di procedere con la compilazione dell'installer.

---

**ULTIMA REVISIONE**: 2025-12-23
**ULTIMA RELEASE TESTATA**: v1.2.4.4 (fix applicato e verificato)
