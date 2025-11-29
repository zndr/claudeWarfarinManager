# Script per sincronizzare le guide dalla cartella docs a Resources/Guides
# Utilizzo: powershell -ExecutionPolicy Bypass -File sync-guides.ps1

param(
    [string]$SourceDir = "D:\Claude\winTaoGest\docs",
    [string]$TargetDir = "D:\Claude\winTaoGest\src\WarfarinManager.UI\Resources\Guides"
)

Write-Host "=== Sincronizzazione Guide TaoGEST ===" -ForegroundColor Cyan
Write-Host "Sorgente: $SourceDir" -ForegroundColor Gray
Write-Host "Destinazione: $TargetDir" -ForegroundColor Gray
Write-Host ""

# Verifica che le directory esistano
if (-not (Test-Path $SourceDir)) {
    Write-Host "ERRORE: Directory sorgente non trovata: $SourceDir" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $TargetDir)) {
    Write-Host "Creazione directory destinazione: $TargetDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

# Definisci i file da sincronizzare
$filesToSync = @(
    "interactions.html",
    "LineeGuida.html",
    "LineeGuida.pdf",
    "Guida Warfarin per pazienti.pdf",
    "Algoritmo Gestione INR.html",
    "infografica-tao.html"
)

$syncedCount = 0
$skippedCount = 0
$errorCount = 0

foreach ($file in $filesToSync) {
    $sourcePath = Join-Path $SourceDir $file

    # Determina il nome del file di destinazione (alcuni file hanno nomi diversi)
    $targetFileName = switch ($file) {
        "LineeGuida.html" { "linee-guida-tao.html" }
        "Algoritmo Gestione INR.html" { "algoritmo-gestione-inr.html" }
        default { $file }
    }

    $targetPath = Join-Path $TargetDir $targetFileName

    if (-not (Test-Path $sourcePath)) {
        Write-Host "⚠️  File non trovato: $file" -ForegroundColor Yellow
        $skippedCount++
        continue
    }

    try {
        # Controlla se il file di destinazione esiste e confronta le date
        $needsCopy = $true
        if (Test-Path $targetPath) {
            $sourceTime = (Get-Item $sourcePath).LastWriteTime
            $targetTime = (Get-Item $targetPath).LastWriteTime

            if ($sourceTime -le $targetTime) {
                Write-Host "⏭️  Già aggiornato: $file" -ForegroundColor Gray
                $skippedCount++
                $needsCopy = $false
            }
        }

        if ($needsCopy) {
            Copy-Item -Path $sourcePath -Destination $targetPath -Force
            Write-Host "✅ Copiato: $file → $targetFileName" -ForegroundColor Green
            $syncedCount++
        }
    }
    catch {
        Write-Host "❌ Errore copiando $file : $_" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "=== Riepilogo ===" -ForegroundColor Cyan
Write-Host "File copiati: $syncedCount" -ForegroundColor Green
Write-Host "File già aggiornati: $skippedCount" -ForegroundColor Gray
if ($errorCount -gt 0) {
    Write-Host "Errori: $errorCount" -ForegroundColor Red
}
Write-Host ""

if ($syncedCount -gt 0) {
    Write-Host "Sincronizzazione completata! Esegui il rebuild dell'applicazione." -ForegroundColor Green
} else {
    Write-Host "Nessuna modifica rilevata." -ForegroundColor Gray
}

exit 0
