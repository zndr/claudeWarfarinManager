#Requires -Version 5.1

<#
.SYNOPSIS
    Calcola l'hash SHA256 dell'installer TaoGEST

.DESCRIPTION
    Script per calcolare l'hash SHA256 dell'installer e aggiornare automaticamente
    il file version.json con l'hash e la dimensione corretti.

.PARAMETER InstallerPath
    Percorso del file installer. Default: publish\TaoGEST-Setup-v*.exe (cerca l'ultimo)

.PARAMETER UpdateVersionJson
    Se specificato, aggiorna automaticamente il file version.json con hash e dimensione

.EXAMPLE
    .\Calculate-InstallerHash.ps1
    Calcola l'hash dell'ultimo installer trovato

.EXAMPLE
    .\Calculate-InstallerHash.ps1 -UpdateVersionJson
    Calcola l'hash e aggiorna version.json automaticamente

.EXAMPLE
    .\Calculate-InstallerHash.ps1 -InstallerPath "publish\TaoGEST-Setup-v1.2.0.exe" -UpdateVersionJson
    Calcola l'hash di un installer specifico e aggiorna version.json
#>

param(
    [string]$InstallerPath = "",
    [switch]$UpdateVersionJson
)

# Colori per output
$colorSuccess = "Green"
$colorWarning = "Yellow"
$colorError = "Red"
$colorInfo = "Cyan"

Write-Host ""
Write-Host "=== Calcolo Hash SHA256 Installer TaoGEST ===" -ForegroundColor $colorInfo
Write-Host ""

# Se non specificato, cerca l'ultimo installer nella cartella publish
if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
    $publishFolder = Join-Path $PSScriptRoot "..\publish"

    if (Test-Path $publishFolder) {
        $installers = Get-ChildItem -Path $publishFolder -Filter "TaoGEST-Setup-v*.exe" | Sort-Object LastWriteTime -Descending

        if ($installers.Count -eq 0) {
            Write-Host "❌ Nessun installer trovato in $publishFolder" -ForegroundColor $colorError
            Write-Host "   Compila prima il progetto e crea l'installer!" -ForegroundColor $colorWarning
            exit 1
        }

        $InstallerPath = $installers[0].FullName
        Write-Host "📦 Installer trovato: $($installers[0].Name)" -ForegroundColor $colorSuccess
    } else {
        Write-Host "❌ Cartella publish non trovata!" -ForegroundColor $colorError
        exit 1
    }
} else {
    # Percorso relativo alla root del progetto
    if (-not [System.IO.Path]::IsPathRooted($InstallerPath)) {
        $InstallerPath = Join-Path $PSScriptRoot "..\$InstallerPath"
    }
}

# Verifica che il file esista
if (-not (Test-Path $InstallerPath)) {
    Write-Host "❌ File non trovato: $InstallerPath" -ForegroundColor $colorError
    exit 1
}

# Ottieni info sul file
$fileInfo = Get-Item $InstallerPath
$fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)

Write-Host ""
Write-Host "📄 File: $($fileInfo.Name)" -ForegroundColor $colorInfo
Write-Host "📏 Dimensione: $fileSizeMB MB ($($fileInfo.Length) bytes)" -ForegroundColor $colorInfo
Write-Host ""

# Calcola hash SHA256
Write-Host "🔐 Calcolo hash SHA256..." -ForegroundColor $colorInfo
$hashStart = Get-Date
$hash = Get-FileHash -Path $InstallerPath -Algorithm SHA256
$hashDuration = (Get-Date) - $hashStart

Write-Host "✅ Hash calcolato in $([math]::Round($hashDuration.TotalSeconds, 2)) secondi" -ForegroundColor $colorSuccess
Write-Host ""
Write-Host "Hash SHA256:" -ForegroundColor $colorInfo
Write-Host $hash.Hash.ToLower() -ForegroundColor $colorSuccess
Write-Host ""

# Aggiorna version.json se richiesto
if ($UpdateVersionJson) {
    $versionJsonPath = Join-Path $PSScriptRoot "..\version.json"

    if (-not (Test-Path $versionJsonPath)) {
        Write-Host "❌ File version.json non trovato in: $versionJsonPath" -ForegroundColor $colorError
        exit 1
    }

    Write-Host "📝 Aggiornamento version.json..." -ForegroundColor $colorInfo

    try {
        # Leggi il file JSON
        $versionJson = Get-Content $versionJsonPath -Raw | ConvertFrom-Json

        # Aggiorna hash e dimensione
        $versionJson.Sha256Hash = $hash.Hash.ToLower()
        $versionJson.FileSize = $fileInfo.Length

        # Salva il file aggiornato (con indentazione)
        $versionJson | ConvertTo-Json -Depth 10 | Set-Content $versionJsonPath -Encoding UTF8

        Write-Host "✅ version.json aggiornato con successo!" -ForegroundColor $colorSuccess
        Write-Host "   - Sha256Hash: $($hash.Hash.ToLower())" -ForegroundColor $colorInfo
        Write-Host "   - FileSize: $($fileInfo.Length) bytes" -ForegroundColor $colorInfo
    }
    catch {
        Write-Host "❌ Errore durante l'aggiornamento di version.json: $_" -ForegroundColor $colorError
        exit 1
    }
}

Write-Host ""
Write-Host "=== Riepilogo ===" -ForegroundColor $colorInfo
Write-Host "File:        $($fileInfo.Name)" -ForegroundColor White
Write-Host "Dimensione:  $fileSizeMB MB ($($fileInfo.Length) bytes)" -ForegroundColor White
Write-Host "Hash SHA256: $($hash.Hash.ToLower())" -ForegroundColor White

if (-not $UpdateVersionJson) {
    Write-Host ""
    Write-Host "💡 Tip: Usa -UpdateVersionJson per aggiornare automaticamente version.json" -ForegroundColor $colorWarning
}

Write-Host ""
