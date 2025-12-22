# 🔐 Guida Rapida: Calcolo Hash SHA256 per Installer

## Cos'è l'Hash SHA256?

L'hash SHA256 è un'impronta digitale unica del file installer che garantisce:

- ✅ **Integrità**: Il file non è stato corrotto durante il download
- ✅ **Sicurezza**: Il file non è stato modificato da terzi
- ✅ **Autenticità**: Il file proviene dalla fonte ufficiale

## Come Calcolare l'Hash

### Metodo 1: Script Automatico (Consigliato)

```powershell
# Calcola hash e aggiorna automaticamente version.json
.\scripts\Calculate-InstallerHash.ps1 -UpdateVersionJson
```

Lo script:
1. Trova automaticamente l'ultimo installer in `publish/`
2. Calcola l'hash SHA256
3. Aggiorna `version.json` con hash e dimensione corretti

**Output esempio:**
```
=== Calcolo Hash SHA256 Installer TaoGEST ===

📦 Installer trovato: TaoGEST-Setup-v1.2.0.exe

📄 File: TaoGEST-Setup-v1.2.0.exe
📏 Dimensione: 50.12 MB (52562944 bytes)

🔐 Calcolo hash SHA256...
✅ Hash calcolato in 0.85 secondi

Hash SHA256:
a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8c9d0e1f2

📝 Aggiornamento version.json...
✅ version.json aggiornato con successo!
```

### Metodo 2: Solo Visualizzare l'Hash

```powershell
# Calcola e mostra solo l'hash (senza modificare version.json)
.\scripts\Calculate-InstallerHash.ps1
```

### Metodo 3: File Specifico

```powershell
# Specifica un installer particolare
.\scripts\Calculate-InstallerHash.ps1 -InstallerPath "publish\TaoGEST-Setup-v1.2.0.exe" -UpdateVersionJson
```

### Metodo 4: PowerShell Manuale

```powershell
# Calcola hash manualmente
Get-FileHash "publish\TaoGEST-Setup-v1.2.0.exe" -Algorithm SHA256
```

Poi copia l'hash in `version.json`:
```json
{
  "Sha256Hash": "a1b2c3d4e5f6g7h8..."
}
```

## Workflow Completo Rilascio

### 1. Compila il Progetto

```bash
.\Update-Version.ps1 -NewVersion "1.2.0.0"
dotnet build -c Release
dotnet publish -c Release
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\TaoGEST-Setup.iss
```

### 2. Calcola Hash e Aggiorna version.json

```powershell
.\scripts\Calculate-InstallerHash.ps1 -UpdateVersionJson
```

### 3. Modifica version.json

Apri [version.json](version.json) e aggiorna:

```json
{
  "Version": "1.2.0",
  "DownloadUrl": "https://github.com/TUO-USERNAME/TaoGEST/releases/download/v1.2.0/TaoGEST-Setup-v1.2.0.exe",
  "ReleaseDate": "2025-12-22T00:00:00",
  "IsCritical": false,
  "FileSize": 52562944,  // ← Già aggiornato dallo script
  "Sha256Hash": "a1b2c3...",  // ← Già aggiornato dallo script
  "ReleaseNotes": "Novità:\n• Feature 1\n• Feature 2"
}
```

### 4. Carica su GitHub Releases

1. Vai su GitHub → Releases → Create new release
2. Tag: `v1.2.0`
3. Upload: `TaoGEST-Setup-v1.2.0.exe`
4. Pubblica la release

### 5. Carica version.json su FTP

```powershell
# Usa il tuo client FTP o PowerShell
$ftpServer = "ftp://tuo-server.com/updates/"
$username = "username"
$password = "password"

$ftpUri = $ftpServer + "version.json"
$ftpRequest = [System.Net.FtpWebRequest]::Create($ftpUri)
$ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
$ftpRequest.Credentials = New-Object System.Net.NetworkCredential($username, $password)

$fileContent = [System.IO.File]::ReadAllBytes("version.json")
$ftpRequest.ContentLength = $fileContent.Length

$requestStream = $ftpRequest.GetRequestStream()
$requestStream.Write($fileContent, 0, $fileContent.Length)
$requestStream.Close()
```

### 6. Testa!

Avvia TaoGEST → Menu **Aiuto** → **Verifica aggiornamenti**

## Cosa Vedono gli Utenti

Quando c'è un aggiornamento con hash, la finestra di notifica mostra:

```
┌──────────────────────────────────────────────┐
│ 🎯 Nuova Versione Disponibile!              │
│ Versione 1.2.0 disponibile (attuale: 1.1.2) │
├──────────────────────────────────────────────┤
│ Versione:      1.2.0                         │
│ Data rilascio: 22/12/2025                    │
│ Dimensione:    50.12 MB                      │
│ Hash SHA256:   a1b2c3d4e5f6g7h8...          │ ← Visualizzato
├──────────────────────────────────────────────┤
│ Novità in questa versione:                  │
│ • Sistema aggiornamenti automatici           │
│ • Verifica integrità download (SHA256)      │
│ • Miglioramenti interfaccia                  │
│                                              │
│ [Scarica Aggiornamento] [Ricordamelo Dopo] │
└──────────────────────────────────────────────┘
```

## Verifica Hash dopo Download (Utente)

Gli utenti possono verificare manualmente l'hash dopo aver scaricato:

```powershell
Get-FileHash "TaoGEST-Setup-v1.2.0.exe" -Algorithm SHA256
```

E confrontarlo con quello mostrato nella notifica.

## Domande Frequenti

### L'hash è obbligatorio?

No, è opzionale. Se ometti `Sha256Hash` dal file JSON:
- Il sistema funziona comunque
- L'hash non viene mostrato nella notifica
- Nessun controllo di integrità

**Raccomandazione**: Includi sempre l'hash per la sicurezza degli utenti.

### Cosa succede se l'hash non corrisponde?

Attualmente il sistema **mostra** l'hash ma non blocca il download. Gli utenti possono verificare manualmente se il file scaricato corrisponde.

In futuro potresti implementare:
- Download automatico con verifica hash integrata
- Blocco installazione se hash non corrispondente
- Avviso all'utente in caso di mismatch

### Quanto tempo impiega il calcolo?

Circa 0.5-2 secondi per un file da 50 MB, dipende dalla velocità del disco.

### L'hash cambia se ricompilo?

Sì! Ogni compilazione genera un binario leggermente diverso, quindi l'hash cambia. Per questo:
1. Compila l'installer finale
2. **POI** calcola l'hash
3. Non ricompilare dopo aver calcolato l'hash

## Risorse

- 📖 [AGGIORNAMENTI-FTP.md](AGGIORNAMENTI-FTP.md) - Guida completa sistema aggiornamenti
- 📝 [version.json](version.json) - File di configurazione versione
- 🔧 [Calculate-InstallerHash.ps1](scripts/Calculate-InstallerHash.ps1) - Script calcolo hash
