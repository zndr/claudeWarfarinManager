# Sistema di Controllo Aggiornamenti via FTP

## Panoramica

TaoGEST include un sistema automatico di controllo aggiornamenti che:
- Controlla la disponibilità di nuove versioni all'avvio dell'applicazione
- Esegue controlli periodici in background (ogni 24 ore di default)
- Permette controlli manuali tramite menu `Aiuto > Verifica aggiornamenti`
- Mostra notifiche con le novità e il link per scaricare l'installer

## Configurazione Server FTP

### 1. Modifica appsettings.json

Apri il file `src/WarfarinManager.UI/appsettings.json` e configura i parametri FTP:

```json
{
  "UpdateChecker": {
    "Enabled": true,
    "FtpHost": "ftp://tuo-server-ftp.com/path/to/updates",
    "FtpUsername": "tuo-username",
    "FtpPassword": "tua-password",
    "VersionFileName": "version.json",
    "TimeoutSeconds": 30,
    "CheckIntervalHours": 24,
    "CheckOnStartup": true
  }
}
```

### 2. Parametri di Configurazione

- **Enabled**: Abilita/disabilita il controllo aggiornamenti
- **FtpHost**: URL completo del server FTP (incluso il percorso alla cartella)
- **FtpUsername**: Nome utente FTP
- **FtpPassword**: Password FTP
- **VersionFileName**: Nome del file JSON con le info versione (default: `version.json`)
- **TimeoutSeconds**: Timeout connessione FTP in secondi
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

## Caricamento su FTP

### Manualmente

Puoi usare qualsiasi client FTP (FileZilla, WinSCP, ecc.) per caricare `version.json` sul server.

### Script PowerShell (esempio)

```powershell
# Upload-Version.ps1
$ftpServer = "ftp://tuo-server.com/updates/"
$username = "tuo-username"
$password = "tua-password"
$localFile = "version.json"

$ftpUri = $ftpServer + "version.json"
$ftpRequest = [System.Net.FtpWebRequest]::Create($ftpUri)
$ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
$ftpRequest.Credentials = New-Object System.Net.NetworkCredential($username, $password)

$fileContent = [System.IO.File]::ReadAllBytes($localFile)
$ftpRequest.ContentLength = $fileContent.Length

$requestStream = $ftpRequest.GetRequestStream()
$requestStream.Write($fileContent, 0, $fileContent.Length)
$requestStream.Close()

Write-Host "File caricato con successo!"
```

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

4. **Carica l'installer** su un server web pubblico (es. GitHub Releases, tuo sito)

5. **Aggiorna version.json** con:
   - Nuova versione
   - URL download dell'installer
   - Note di rilascio
   - Data di rilascio
   - Hash SHA256 e dimensione (già aggiornati dallo script)

6. **Carica version.json sul server FTP**

7. Gli utenti riceveranno automaticamente la notifica al prossimo controllo!

## Test del Sistema

### Test Locale (senza FTP)

Per testare senza configurare FTP, puoi:

1. Modificare temporaneamente `UpdateCheckerService.cs` per leggere da file locale
2. Creare un file `version.json` locale con versione superiore
3. Avviare l'applicazione e verificare che compaia la notifica

### Test con FTP

1. Configura `appsettings.json` con credenziali FTP di test
2. Carica `version.json` con versione > versione corrente
3. Usa menu `Aiuto > Verifica aggiornamenti`
4. Dovresti vedere la finestra di notifica

## Sicurezza

⚠️ **IMPORTANTE**:

- Il file `appsettings.json` contiene credenziali FTP in chiaro
- **NON committare** `appsettings.json` con credenziali reali su repository pubblici
- Considera l'uso di variabili d'ambiente per le credenziali in produzione
- Il file è già aggiunto a `.gitignore` per evitare commit accidentali

### Alternativa Sicura: Variabili d'Ambiente

Modifica `App.xaml.cs` per leggere credenziali da variabili d'ambiente:

```csharp
var ftpHost = Environment.GetEnvironmentVariable("TAOGEST_FTP_HOST")
    ?? configuration.GetValue<string>("UpdateChecker:FtpHost") ?? "";
var ftpUsername = Environment.GetEnvironmentVariable("TAOGEST_FTP_USER")
    ?? configuration.GetValue<string>("UpdateChecker:FtpUsername") ?? "";
var ftpPassword = Environment.GetEnvironmentVariable("TAOGEST_FTP_PASS")
    ?? configuration.GetValue<string>("UpdateChecker:FtpPassword") ?? "";
```

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

1. Verifica connessione FTP (credenziali corrette)
2. Controlla che `version.json` sia nel percorso corretto sul server
3. Verifica formato JSON valido (usa JSONLint.com)
4. Controlla i log in `%LocalAppData%\WarfarinManager\Logs\`

### Timeout FTP

Aumenta `TimeoutSeconds` in `appsettings.json`:

```json
"TimeoutSeconds": 60
```

### La versione non viene riconosciuta come più recente

Il confronto versioni usa `System.Version`:
- Formato valido: `1.2.0` o `1.2.0.0`
- Versione errata: `v1.2.0` o `1.2.0-beta`
