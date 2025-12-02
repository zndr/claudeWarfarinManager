# Build-Installer.ps1
# Script automatico per compilazione installer con Inno Setup

param(
    [string]$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    [switch]$SkipBuild = $false,
    [switch]$OpenFolder = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TaoGest - Build Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Percorsi
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptRoot
$InstallerScript = Join-Path $SolutionRoot "installer\TaoGest-Setup.iss"
$VersionPropsPath = Join-Path $SolutionRoot "Version.props"

Write-Host "→ Verifica Inno Setup..." -ForegroundColor Yellow

# Verifica Inno Setup installato
if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "✗ ERRORE: Inno Setup non trovato!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Percorso cercato: $InnoSetupPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Scarica e installa Inno Setup da:" -ForegroundColor Yellow
    Write-Host "  https://jrsoftware.org/isdl.php" -ForegroundColor White
    Write-Host ""
    Write-Host "Oppure specifica il percorso corretto:" -ForegroundColor Yellow
    Write-Host "  .\Build-Installer.ps1 -InnoSetupPath 'C:\path\to\ISCC.exe'" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "  ✓ Inno Setup trovato: $InnoSetupPath" -ForegroundColor Green
Write-Host ""

# Leggi versione
Write-Host "→ Lettura versione..." -ForegroundColor Yellow
[xml]$versionProps = Get-Content $VersionPropsPath
$version = $versionProps.Project.PropertyGroup.VersionPrefix
Write-Host "  Versione applicazione: $version" -ForegroundColor Green
Write-Host ""

# Step 1: Build Release (se non skippato)
if (-not $SkipBuild) {
    Write-Host "→ [1/3] Esecuzione build release..." -ForegroundColor Cyan
    
    $BuildScript = Join-Path $ScriptRoot "Build-Release.ps1"
    
    if (-not (Test-Path $BuildScript)) {
        Write-Host "✗ Errore: Build-Release.ps1 non trovato!" -ForegroundColor Red
        exit 1
    }
    
    & $BuildScript -SkipTests
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Errore durante build!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ Build completata" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "→ [1/3] Build skippato (-SkipBuild)" -ForegroundColor Yellow
    Write-Host ""
}

# Step 2: Verifica file pubblicati
Write-Host "→ [2/3] Verifica file pubblicati..." -ForegroundColor Cyan

$PublishDir = Join-Path $SolutionRoot "publish\TaoGest-v$version"
Write-Host "  Controllo directory: $PublishDir" -ForegroundColor Gray

if (-not (Test-Path $PublishDir)) {
    Write-Host "✗ Errore: Directory pubblicata non trovata!" -ForegroundColor Red
    Write-Host "  Atteso: $PublishDir" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Eseguire prima: .\scripts\Build-Release.ps1" -ForegroundColor Yellow
    exit 1
}

$ExePath = Join-Path $PublishDir "WarfarinManager.UI.exe"
Write-Host "  Controllo eseguibile: $ExePath" -ForegroundColor Gray

if (-not (Test-Path $ExePath)) {
    Write-Host "✗ Errore: Eseguibile non trovato!" -ForegroundColor Red
    Write-Host "  Atteso: $ExePath" -ForegroundColor Gray
    exit 1
}

Write-Host "  ✓ File pubblicati trovati" -ForegroundColor Green
Write-Host ""

# Step 2.5: Aggiorna versione in script Inno Setup
Write-Host "→ [2.5/3] Aggiornamento versione in script Inno Setup..." -ForegroundColor Cyan
Write-Host "  File script: $InstallerScript" -ForegroundColor Gray

if (-not (Test-Path $InstallerScript)) {
    Write-Host "✗ Errore: Script Inno Setup non trovato!" -ForegroundColor Red
    Write-Host "  Atteso: $InstallerScript" -ForegroundColor Gray
    exit 1
}

try {
    $IssContent = Get-Content $InstallerScript -Raw
    $IssContent = $IssContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion ""$version"""
    $IssContent | Set-Content $InstallerScript -Encoding UTF8
    Write-Host "  ✓ Versione aggiornata a $version" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Errore durante aggiornamento versione: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Crea directory output installer se non esiste
$InstallerOutputDir = Join-Path $SolutionRoot "publish\installer"
if (-not (Test-Path $InstallerOutputDir)) {
    Write-Host "→ Creazione directory output installer..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $InstallerOutputDir -Force | Out-Null
    Write-Host "  ✓ Directory creata: $InstallerOutputDir" -ForegroundColor Green
    Write-Host ""
}

# Step 4: Compila installer
Write-Host "→ [3/3] Compilazione installer con Inno Setup..." -ForegroundColor Cyan
Write-Host "  Script: $InstallerScript" -ForegroundColor Gray
Write-Host "  Compiler: $InnoSetupPath" -ForegroundColor Gray
Write-Host ""
Write-Host "  Esecuzione ISCC.exe (attendere)..." -ForegroundColor Yellow
Write-Host ""

try {
    # Esegui Inno Setup Compiler con output
    $process = Start-Process -FilePath $InnoSetupPath `
                             -ArgumentList "`"$InstallerScript`"" `
                             -NoNewWindow `
                             -Wait `
                             -PassThru
    
    $exitCode = $process.ExitCode
    
    if ($exitCode -ne 0) {
        Write-Host ""
        Write-Host "✗ Errore durante compilazione installer!" -ForegroundColor Red
        Write-Host "  Exit code: $exitCode" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Controlla:" -ForegroundColor Yellow
        Write-Host "  1. Log Inno Setup per dettagli errore" -ForegroundColor Gray
        Write-Host "  2. Path file in TaoGest-Setup.iss corretto" -ForegroundColor Gray
        Write-Host "  3. Tutti i file esistono in publish\TaoGest-v$version\" -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "✗ Errore durante esecuzione Inno Setup: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "  ✓ Installer compilato con successo" -ForegroundColor Green
Write-Host ""

# Step 5: Verifica file installer generato
$InstallerFile = Join-Path $InstallerOutputDir "TaoGest-Setup-v$version.exe"
Write-Host "→ Verifica file installer..." -ForegroundColor Yellow
Write-Host "  Percorso: $InstallerFile" -ForegroundColor Gray

if (Test-Path $InstallerFile) {
    $InstallerSize = (Get-Item $InstallerFile).Length / 1MB
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  INSTALLER CREATO CON SUCCESSO!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "File installer:" -ForegroundColor Cyan
    Write-Host "  $InstallerFile" -ForegroundColor White
    Write-Host "  Dimensione: $([math]::Round($InstallerSize, 2)) MB" -ForegroundColor Gray
    Write-Host ""
    
    # Calcola hash per verificare integrità
    Write-Host "Calcolo hash SHA256..." -ForegroundColor Yellow
    $Hash = (Get-FileHash $InstallerFile -Algorithm SHA256).Hash
    Write-Host "  SHA256: $Hash" -ForegroundColor Gray
    
    # Salva hash in file
    $HashFile = "$InstallerFile.sha256"
    "$Hash  TaoGest-Setup-v$version.exe" | Out-File -FilePath $HashFile -Encoding ASCII
    Write-Host "  Hash salvato in: $HashFile" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Prossimi passi:" -ForegroundColor Yellow
    Write-Host "  1. Testare installer su macchina pulita (VM consigliata)" -ForegroundColor Gray
    Write-Host "  2. Verificare installazione, avvio applicazione, database" -ForegroundColor Gray
    Write-Host "  3. Testare disinstallazione" -ForegroundColor Gray
    Write-Host "  4. Distribuire installer agli utenti finali" -ForegroundColor Gray
    Write-Host ""
    
    # Apri cartella se richiesto
    if ($OpenFolder) {
        Write-Host "Apertura cartella installer..." -ForegroundColor Cyan
        Start-Process explorer.exe -ArgumentList $InstallerOutputDir
    } else {
        Write-Host "Per aprire la cartella installer esegui:" -ForegroundColor Cyan
        Write-Host "  explorer ""$InstallerOutputDir""" -ForegroundColor Gray
    }
    
} else {
    Write-Host ""
    Write-Host "⚠ Warning: File installer non trovato nel percorso atteso" -ForegroundColor Yellow
    Write-Host "  Cercato: $InstallerFile" -ForegroundColor Gray
    Write-Host "  Controlla directory: $InstallerOutputDir" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Build installer completato!" -ForegroundColor Green
Write-Host ""
