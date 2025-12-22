# 📦 Guida Completa: Creazione Release su GitHub

Guida passo-passo per creare una nuova release di TaoGEST e caricare l'installer su GitHub.

## 📋 Prerequisiti

- [GitHub CLI (gh)](https://cli.github.com/) installato e autenticato
- [Inno Setup 6](https://jrsoftware.org/isdl.php) installato
- .NET 8 SDK installato
- Repository clonato localmente

## 🚀 Procedura Completa

### Step 1: Aggiorna la Versione del Progetto

```powershell
# Dalla root del progetto
.\Update-Version.ps1 -NewVersion "1.3.0.0"
```

Questo script aggiorna automaticamente:
- `Version.props`
- `installer/TaoGEST-Setup.iss`
- `src/WarfarinManager.UI/MainWindow.xaml`

### Step 2: Compila il Progetto

```bash
# Clean della soluzione
dotnet clean

# Build in modalità Release
dotnet build -c Release

# Publish dell'applicazione
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

**Nota**: Il comando publish deve essere eseguito **prima** di creare l'installer.

### Step 3: Crea l'Installer con Inno Setup

```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\TaoGEST-Setup.iss
```

L'installer verrà creato in: `publish/TaoGEST-Setup-v1.3.0.exe`

### Step 4: Calcola Hash SHA256 e Dimensione File

#### Opzione A: Script Automatico (Raccomandato)

```powershell
# Calcola hash e aggiorna automaticamente version.json
.\scripts\Calculate-InstallerHash.ps1 -UpdateVersionJson
```

#### Opzione B: Manualmente

```powershell
# Calcola hash SHA256
(Get-FileHash 'publish\TaoGEST-Setup-v1.3.0.exe' -Algorithm SHA256).Hash.ToLower()

# Ottieni dimensione file in bytes
(Get-Item 'publish\TaoGEST-Setup-v1.3.0.exe').Length

# Converti in MB per le note di rilascio
[math]::Round((Get-Item 'publish\TaoGEST-Setup-v1.3.0.exe').Length / 1MB, 2)
```

Annota i valori ottenuti per il prossimo step.

### Step 5: Aggiorna version.json

Modifica il file `version.json` nella root del progetto:

```json
{
  "Version": "1.3.0",
  "DownloadUrl": "https://github.com/zndr/claudeWarfarinManager/releases/download/v1.3.0/TaoGEST-Setup-v1.3.0.exe",
  "ReleaseDate": "2025-12-22T00:00:00",
  "IsCritical": false,
  "FileSize": 66484934,
  "Sha256Hash": "abc123...",
  "ReleaseNotes": "🎯 TaoGEST v1.3.0 - [Titolo Release]\n\n🆕 NUOVE FUNZIONALITÀ:\n• Feature 1\n• Feature 2\n\n🔧 MIGLIORAMENTI:\n• Miglioramento 1\n• Miglioramento 2\n\n📥 INSTALLAZIONE:\n1. Chiudi TaoGEST se aperto\n2. Scarica e installa l'aggiornamento\n3. Riavvia l'applicazione"
}
```

**Campi da aggiornare**:
- `Version`: Nuova versione (es. "1.3.0")
- `DownloadUrl`: URL del file su GitHub Release
- `ReleaseDate`: Data di rilascio in formato ISO 8601
- `IsCritical`: `true` se è un aggiornamento critico
- `FileSize`: Dimensione in bytes (dal comando precedente)
- `Sha256Hash`: Hash SHA256 (dal comando precedente)
- `ReleaseNotes`: Note di rilascio in formato testo con `\n` per a capo

### Step 6: Commit delle Modifiche di Versione

```bash
# Aggiungi i file modificati
git add Version.props installer/TaoGEST-Setup.iss version.json

# Commit con messaggio descrittivo
git commit -m "Release: Versione 1.3.0 - [Descrizione breve]

Aggiornata versione a 1.3.0 con le seguenti modifiche:

Versioning:
• Version.props: 1.2.0 → 1.3.0
• TaoGEST-Setup.iss: 1.2.0 → 1.3.0
• version.json aggiornato con hash e dimensioni corrette

Installer:
• File: TaoGEST-Setup-v1.3.0.exe
• Dimensione: XX.XX MB (XX,XXX,XXX bytes)
• Hash SHA256: abc123...

Nuove funzionalità principali:
• Feature 1
• Feature 2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

# Push su GitHub
git push origin master
```

### Step 7: Crea Release su GitHub

#### Metodo 1: Comando Unico (Raccomandato)

```bash
gh release create v1.3.0 \
  --title "TaoGEST v1.3.0 - [Titolo Descrittivo]" \
  --notes "$(cat <<'EOF'
## 🎯 TaoGEST v1.3.0 - [Titolo Descrittivo]

### 🆕 Nuove Funzionalità
- **Feature 1**: Descrizione dettagliata
- **Feature 2**: Descrizione dettagliata
- **Feature 3**: Descrizione dettagliata

### 🔧 Miglioramenti
- Miglioramento prestazioni database
- Miglioramenti all'interfaccia utente
- Ottimizzazioni varie

### 🐛 Correzioni Bug
- Fix bug #123: Descrizione
- Fix crash su Windows 10
- Correzione calcolo dosaggio

### 📥 Installazione
1. Chiudi TaoGEST se aperto
2. Scarica e installa l'aggiornamento
3. Riavvia l'applicazione

### 📄 Dettagli Installer
- **File**: TaoGEST-Setup-v1.3.0.exe
- **Dimensione**: XX.XX MB (XX,XXX,XXX bytes)
- **SHA256**: `abc123def456...`

### 🔗 Documentazione
- [Guida Sistema Aggiornamenti](https://github.com/zndr/claudeWarfarinManager/blob/master/AGGIORNAMENTI-HTTPS.md)
- [Guida Rapida Hash SHA256](https://github.com/zndr/claudeWarfarinManager/blob/master/GUIDA-RAPIDA-HASH.md)

**Full Changelog**: https://github.com/zndr/claudeWarfarinManager/compare/v1.2.0...v1.3.0
EOF
)" \
  publish/TaoGEST-Setup-v1.3.0.exe
```

**Sostituisci**:
- `v1.3.0` con la nuova versione
- `[Titolo Descrittivo]` con un titolo significativo
- Le note di rilascio con il contenuto appropriato
- `v1.2.0` nel changelog con la versione precedente

#### Metodo 2: Step-by-Step

Se preferisci creare la release in più passaggi:

```bash
# 1. Crea la release senza file
gh release create v1.3.0 --title "TaoGEST v1.3.0 - [Titolo]"

# 2. Carica l'installer
gh release upload v1.3.0 publish/TaoGEST-Setup-v1.3.0.exe

# 3. Modifica le note di rilascio via web
gh release view v1.3.0 --web
```

### Step 8: Verifica Release Creata

```bash
# Visualizza dettagli della release
gh release view v1.3.0

# Verifica URL download dell'installer
gh release view v1.3.0 --json assets --jq '.assets[].url'

# Apri la release nel browser
gh release view v1.3.0 --web
```

**L'URL di download sarà**:
```
https://github.com/zndr/claudeWarfarinManager/releases/download/v1.3.0/TaoGEST-Setup-v1.3.0.exe
```

### Step 9: Verifica version.json (Opzionale)

Se l'URL in `version.json` non corrisponde esattamente all'URL della release:

```bash
# Aggiorna version.json con l'URL corretto
# Modifica manualmente il file

# Commit e push
git add version.json
git commit -m "Docs: Aggiornato URL download in version.json per release v1.3.0"
git push origin master
```

## 🧪 Test del Sistema Aggiornamenti

Dopo aver creato la release, testa il sistema:

### Metodo 1: Con Versione Precedente Installata

1. Installa o avvia TaoGEST versione precedente (es. v1.2.0)
2. Menu **Aiuto** → **Verifica aggiornamenti**
3. Dovresti vedere la finestra di notifica con la nuova versione!

### Metodo 2: Con appsettings.json Locale

1. Crea o modifica `src/WarfarinManager.UI/appsettings.json`:
   ```json
   {
     "UpdateChecker": {
       "Enabled": true,
       "VersionFileUrl": "https://raw.githubusercontent.com/zndr/claudeWarfarinManager/master/version.json",
       "TimeoutSeconds": 30,
       "CheckIntervalHours": 24,
       "CheckOnStartup": true
     }
   }
   ```

2. Compila e avvia l'applicazione
3. Menu **Aiuto** → **Verifica aggiornamenti**

### Metodo 3: Test con File Locale

1. Modifica temporaneamente `version.json` con una versione superiore
2. Avvia un server HTTP locale:
   ```bash
   python -m http.server 8000
   # o
   npx http-server -p 8000
   ```

3. In `appsettings.json`:
   ```json
   "VersionFileUrl": "http://localhost:8000/version.json"
   ```

4. Compila e testa

## 📋 Checklist Completa Release

Usa questa checklist per assicurarti di non dimenticare nulla:

- [ ] **Step 1**: Aggiornata versione con `Update-Version.ps1`
- [ ] **Step 2**: Compilato progetto (`dotnet build -c Release`)
- [ ] **Step 2**: Pubblicata applicazione (`dotnet publish`)
- [ ] **Step 3**: Creato installer con Inno Setup
- [ ] **Step 4**: Calcolato hash SHA256 dell'installer
- [ ] **Step 5**: Aggiornato `version.json` con nuova versione, hash e dimensione
- [ ] **Step 6**: Committato e pushato modifiche versione
- [ ] **Step 7**: Creata release su GitHub con `gh release create`
- [ ] **Step 8**: Verificata release su GitHub (URL, dimensione, hash)
- [ ] **Step 9**: Verificato che l'URL in `version.json` sia corretto
- [ ] **Test**: Testato sistema aggiornamenti
- [ ] **Comunicazione**: Annunciata release (opzionale)

## ❓ Troubleshooting

### Errore: "gh: command not found"

**Problema**: GitHub CLI non installato.

**Soluzione**:
1. Scarica da https://cli.github.com/
2. Installa e riavvia il terminale
3. Autentica con `gh auth login`

### Errore: "File not found: publish/TaoGEST-Setup-v1.3.0.exe"

**Problema**: L'installer non è stato creato o il nome è diverso.

**Soluzione**:
1. Verifica che Inno Setup abbia completato correttamente
2. Controlla la cartella `publish/` per il nome esatto del file
3. Verifica che la versione in `TaoGEST-Setup.iss` corrisponda

### L'hash SHA256 non corrisponde dopo il download

**Problema**: Il file è stato ricompilato dopo aver calcolato l'hash.

**Soluzione**:
1. **NON ricompilare** dopo aver calcolato l'hash
2. Se necessario, ricalcola l'hash e aggiorna `version.json`
3. Elimina la release vecchia e ricreala:
   ```bash
   gh release delete v1.3.0 --yes
   gh release create v1.3.0 ... # ricrea
   ```

### La notifica di aggiornamento non appare

**Possibili cause**:
1. `version.json` non è accessibile (verifica l'URL nel browser)
2. La versione in `version.json` non è superiore alla versione installata
3. `appsettings.json` ha `Enabled: false`
4. Errore di formato in `version.json` (verifica con JSONLint)

**Debug**:
1. Controlla i log in `%LocalAppData%\WarfarinManager\Logs\`
2. Verifica che l'URL funzioni: apri l'URL di `VersionFileUrl` nel browser
3. Verifica formato JSON: https://jsonlint.com/

## 🔗 Riferimenti Utili

- [Documentazione GitHub CLI](https://cli.github.com/manual/)
- [Documentazione Inno Setup](https://jrsoftware.org/ishelp/)
- [Semantic Versioning](https://semver.org/)
- [AGGIORNAMENTI-HTTPS.md](AGGIORNAMENTI-HTTPS.md) - Guida sistema aggiornamenti
- [GUIDA-RAPIDA-HASH.md](GUIDA-RAPIDA-HASH.md) - Guida hash SHA256

## 📝 Note Importanti

1. **Semantic Versioning**: Usa il formato `MAJOR.MINOR.PATCH`
   - `MAJOR`: Breaking changes (es. 1.x.x → 2.0.0)
   - `MINOR`: Nuove funzionalità (es. 1.2.x → 1.3.0)
   - `PATCH`: Bug fixes (es. 1.2.0 → 1.2.1)

2. **Hash SHA256**: Calcola **sempre** l'hash dopo aver creato l'installer finale

3. **Tag Git**: I tag seguono il formato `vMAJOR.MINOR.PATCH` (es. `v1.3.0`)

4. **URL Download**: Verifica sempre che l'URL in `version.json` corrisponda al repository GitHub

5. **Pre-release**: Per versioni beta/alpha, usa:
   ```bash
   gh release create v1.3.0-beta.1 --prerelease ...
   ```

6. **Draft Release**: Per preparare una release senza pubblicarla:
   ```bash
   gh release create v1.3.0 --draft ...
   ```

## 🎯 Best Practices

- ✅ Testa sempre l'applicazione prima di creare la release
- ✅ Scrivi note di rilascio chiare e dettagliate
- ✅ Includi sempre l'hash SHA256 per sicurezza
- ✅ Mantieni un changelog aggiornato
- ✅ Tagga le release con semantic versioning
- ✅ Testa il sistema di aggiornamenti prima dell'annuncio pubblico
- ✅ Mantieni backup degli installer precedenti

---

**Ultima modifica**: 2025-12-22
**Versione guida**: 1.0
