# Sistema di Controllo Aggiornamenti via HTTPS

## Panoramica

TaoGEST include un sistema automatico di controllo aggiornamenti che:
- Controlla la disponibilità di nuove versioni all'avvio dell'applicazione
- Esegue controlli periodici in background (ogni 24 ore di default)
- Permette controlli manuali tramite menu `Aiuto > Verifica aggiornamenti`
- Mostra notifiche con le novità e il link per scaricare l'installer
- **Nessuna credenziale richiesta** - usa HTTPS pubblico

## Configurazione

### 1. Modifica appsettings.json

Apri il file `src/WarfarinManager.UI/appsettings.json` e configura l'URL:

```json
{
  "UpdateChecker": {
    "Enabled": true,
    "VersionFileUrl": "https://raw.githubusercontent.com/TUO-USERNAME/TaoGEST/master/version.json",
    "TimeoutSeconds": 30,
    "CheckIntervalHours": 24,
    "CheckOnStartup": true
  }
}
```

### 2. Parametri di Configurazione

- **Enabled**: Abilita/disabilita il controllo aggiornamenti
- **VersionFileUrl**: URL pubblico del file version.json (HTTPS)
- **TimeoutSeconds**: Timeout connessione HTTP in secondi
- **CheckIntervalHours**: Intervallo tra i controlli automatici in ore
- **CheckOnStartup**: Se `true`, controlla aggiornamenti all'avvio

## Preparazione File version.json

### 1. Copia il file di esempio

```bash
copy version.json.example version.json
```

### 2. Modifica version.json con i dati della nuova release

```json
{
  "Version": "1.2.0",
  "DownloadUrl": "https://www.tuo-sito.com/downloads/TaoGEST-Setup-v1.2.0.exe",
  "ReleaseDate": "2025-01-15T00:00:00",
  "IsCritical": false,
  "FileSize": 52428800,
  "Sha256Hash": "abc123def456...",
  "ReleaseNotes": "Novità in TaoGEST v1.2.0:\n\n• Feature 1\n• Feature 2\n• Bug fix"
}
```

### 3. Campi del file version.json

- **Version**: Versione dell'aggiornamento (formato: `major.minor.patch`)
- **DownloadUrl**: URL pubblico per scaricare l'installer
- **ReleaseDate**: Data di rilascio in formato ISO 8601
- **IsCritical**: `true` per aggiornamenti critici (mostra badge rosso)
- **FileSize**: Dimensione file in byte (per mostrare MB nel dialog)
- **Sha256Hash**: Hash SHA256 per verifica integrità del download (raccomandato)
- **ReleaseNotes**: Note di rilascio in formato testo (supporta `\n` per a capo)

### 🔐 Hash SHA256 - Perché è importante?

L'hash SHA256 permette di verificare che il file scaricato dall'utente sia identico a quello originale:

- ✅ **Integrità**: Verifica che il file non sia stato corrotto durante il download
- ✅ **Sicurezza**: Protegge da file modificati da terzi (man-in-the-middle)
- ✅ **Autenticità**: Conferma che il file proviene dalla fonte ufficiale

**Consiglio**: Includi sempre l'hash SHA256 nel file `version.json` per garantire la sicurezza degli utenti.

## Caricamento version.json

### Opzione 1: GitHub Repository (Consigliato)

Il modo più semplice è commitare `version.json` direttamente nel repository GitHub:

```bash
# Dopo aver aggiornato version.json
git add version.json
git commit -m "Update: Versione 1.2.0"
git push origin master
```

**URL pubblico**: `https://raw.githubusercontent.com/TUO-USERNAME/TaoGEST/master/version.json`

✅ **Vantaggi**:
- Nessun server FTP necessario
- Nessuna credenziale da gestire
- Gratuito e sempre disponibile
- Versionato con Git

### Opzione 2: GitHub Releases

Puoi anche caricare `version.json` come asset di una release:

1. Vai su GitHub → Releases → Create new release
2. Upload `version.json` come asset
3. Usa l'URL dell'asset

### Opzione 3: Server Web Pubblico

Se hai un sito web, carica semplicemente il file:

```bash
# Via FTP o SFTP al tuo hosting
scp version.json user@yourserver.com:/var/www/html/updates/
```

**URL**: `https://tuosito.com/updates/version.json`

## Workflow Rilascio Nuova Versione

1. **Aggiorna Version.props** con la nuova versione:
   ```bash
   .\Update-Version.ps1 -NewVersion "1.2.0.0"
   ```

2. **Compila e crea l'installer**:
   ```bash
   dotnet build -c Release
   dotnet publish -c Release
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\TaoGEST-Setup.iss
   ```

3. **Calcola hash SHA256 e aggiorna version.json**:
   ```powershell
   # Calcola hash e aggiorna automaticamente version.json
   .\scripts\Calculate-InstallerHash.ps1 -UpdateVersionJson

   # Oppure solo per vedere l'hash senza aggiornare
   .\scripts\Calculate-InstallerHash.ps1
   ```

   Lo script:
   - Trova automaticamente l'ultimo installer creato
   - Calcola l'hash SHA256
   - Aggiorna automaticamente `version.json` con hash e dimensione corretti

4. **Carica l'installer** su GitHub Releases

5. **Aggiorna version.json** con:
   - Nuova versione
   - URL download dell'installer
   - Note di rilascio
   - Data di rilascio
   - Hash SHA256 e dimensione (già aggiornati dallo script)

6. **Committa e pusha version.json**:
   ```bash
   git add version.json
   git commit -m "Update: Versione 1.2.0"
   git push origin master
   ```

7. Gli utenti riceveranno automaticamente la notifica al prossimo controllo!

## Test del Sistema

### Test Locale

Per testare il sistema, puoi usare un file version.json locale o un server HTTP locale:

#### Opzione 1: File Locale con Server HTTP Semplice

```powershell
# Avvia un server HTTP nella cartella root del progetto
python -m http.server 8000

# O con Node.js
npx http-server -p 8000
```

Poi in `appsettings.json`:
```json
"VersionFileUrl": "http://localhost:8000/version.json"
```

### Test Remoto

1. Committa `version.json` con una versione superiore alla corrente
2. Pusha su GitHub
3. Avvia TaoGEST → Menu "Aiuto > Verifica aggiornamenti"
4. Dovresti vedere la finestra di notifica!

## Sicurezza

✅ **Nessuna credenziale necessaria**:

- Il sistema usa HTTPS pubblico, nessuna autenticazione richiesta
- Il file `appsettings.json` contiene solo un URL pubblico
- Nessun rischio di esposizione credenziali
- Il file è comunque in `.gitignore` per sicurezza

## Disabilitare gli Aggiornamenti

Per disabilitare completamente il sistema:

```json
{
  "UpdateChecker": {
    "Enabled": false
  }
}
```

## Troubleshooting

### L'applicazione non rileva aggiornamenti

1. Verifica connessione internet
2. Controlla che l'URL in `appsettings.json` sia corretto
3. Verifica che `version.json` sia accessibile pubblicamente (apri l'URL nel browser)
4. Verifica formato JSON valido (usa JSONLint.com)
5. Controlla i log in `%LocalAppData%\WarfarinManager\Logs\`

### Timeout HTTP

Aumenta `TimeoutSeconds` in `appsettings.json`:

```json
"TimeoutSeconds": 60
```

### Errore 404 Not Found

L'URL non è corretto o il file non esiste:
- Verifica l'URL aprendo nel browser
- Se usi GitHub, assicurati che il file sia nel branch master
- L'URL deve essere `https://raw.githubusercontent.com/...` (non `https://github.com/...`)

### La versione non viene riconosciuta come più recente

Il confronto versioni usa `System.Version`:
- Formato valido: `1.2.0` o `1.2.0.0`
- Versione errata: `v1.2.0` o `1.2.0-beta`
