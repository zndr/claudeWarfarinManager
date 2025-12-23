<#
.SYNOPSIS
    Script di preparazione release automatizzata per TaoGEST

.DESCRIPTION
    Questo script automatizza tutti i passaggi necessari per creare una nuova release:
    - Aggiorna tutti i file di versione
    - Verifica la compilazione
    - Crea il publish
    - Compila l'installer
    - Calcola l'hash SHA256
    - Aggiorna version.json
    - Guida attraverso commit, tag e release GitHub

.PARAMETER NewVersion
    La nuova versione nel formato X.X.X.X (es: 1.3.0.0)

.PARAMETER ReleaseNotes
    Note di rilascio (opzionale, può essere inserito interattivamente)

.EXAMPLE
    .\scripts\Prepare-Release.ps1 -NewVersion "1.3.0.0"
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$NewVersion,

    [Parameter(Mandatory=$false)]
    [string]$ReleaseNotes
)

$ErrorActionPreference = "Stop"
$RootPath = Split-Path -Parent $PSScriptRoot

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       TaoGEST - Script Preparazione Release v$NewVersion       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# STEP 1: Richiedi Release Notes se non fornite
# ============================================================================
if ([string]::IsNullOrWhiteSpace($ReleaseNotes)) {
    Write-Host "📝 STEP 1: Inserisci le release notes" -ForegroundColor Yellow
    Write-Host "Scrivi le note di rilascio (premi CTRL+Z poi INVIO per terminare):" -ForegroundColor Gray
    $ReleaseNotes = [System.Console]::In.ReadToEnd()
}

Write-Host "✓ Release notes acquisite" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 2: Aggiorna Version.props
# ============================================================================
Write-Host "📦 STEP 2: Aggiornamento Version.props..." -ForegroundColor Yellow

$versionPropsPath = Join-Path $RootPath "Version.props"
[xml]$versionProps = Get-Content $versionPropsPath

$versionProps.Project.PropertyGroup.AssemblyVersion = $NewVersion
$versionProps.Project.PropertyGroup.FileVersion = $NewVersion
$versionProps.Project.PropertyGroup.Version = $NewVersion

$versionProps.Save($versionPropsPath)
Write-Host "✓ Version.props aggiornato" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 3: Aggiorna TaoGEST-Setup.iss
# ============================================================================
Write-Host "📦 STEP 3: Aggiornamento installer script..." -ForegroundColor Yellow

$issPath = Join-Path $RootPath "installer\TaoGEST-Setup.iss"
$issContent = Get-Content $issPath -Raw
$issContent = $issContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$NewVersion`""

Set-Content -Path $issPath -Value $issContent -NoNewline
Write-Host "✓ TaoGEST-Setup.iss aggiornato" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 4: Aggiorna ReleaseNotes.txt
# ============================================================================
Write-Host "📦 STEP 4: Aggiornamento ReleaseNotes.txt..." -ForegroundColor Yellow

$releaseNotesPath = Join-Path $RootPath "docs\ReleaseNotes.txt"
$currentDate = Get-Date -Format "yyyy-MM-dd"

$newSection = @"
═══════════════════════════════════════════════════════════════
VERSIONE $NewVersion - $currentDate
═══════════════════════════════════════════════════════════════

$ReleaseNotes

"@

$existingContent = Get-Content $releaseNotesPath -Raw
$updatedContent = $newSection + "`r`n" + $existingContent

Set-Content -Path $releaseNotesPath -Value $updatedContent -NoNewline
Write-Host "✓ ReleaseNotes.txt aggiornato" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 5: Aggiorna CHANGELOG.md
# ============================================================================
Write-Host "📦 STEP 5: Aggiornamento CHANGELOG.md..." -ForegroundColor Yellow

$changelogPath = Join-Path $RootPath "CHANGELOG.md"
$changelogSection = @"
## [$NewVersion] - $currentDate

$ReleaseNotes

"@

if (Test-Path $changelogPath) {
    $changelogContent = Get-Content $changelogPath -Raw
    # Inserisci dopo il titolo principale
    $changelogContent = $changelogContent -replace '(# Changelog\s+)', "`$1`r`n$changelogSection"
    Set-Content -Path $changelogPath -Value $changelogContent -NoNewline
} else {
    $newChangelog = @"
# Changelog

Tutte le modifiche importanti di questo progetto verranno documentate in questo file.

$changelogSection
"@
    Set-Content -Path $changelogPath -Value $newChangelog -NoNewline
}

Write-Host "✓ CHANGELOG.md aggiornato" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 6: Build Release
# ============================================================================
Write-Host "🔨 STEP 6: Compilazione Release..." -ForegroundColor Yellow

Push-Location $RootPath
try {
    & dotnet clean -c Release | Out-Null
    & dotnet restore | Out-Null
    & dotnet build -c Release

    if ($LASTEXITCODE -ne 0) {
        throw "Errore durante la compilazione"
    }

    Write-Host "✓ Build completato con successo" -ForegroundColor Green
} finally {
    Pop-Location
}
Write-Host ""

# ============================================================================
# STEP 7: Publish
# ============================================================================
Write-Host "📤 STEP 7: Creazione publish self-contained..." -ForegroundColor Yellow

$publishPath = Join-Path $RootPath "publish"
if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

Push-Location $RootPath
try {
    & dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=false `
        -o $publishPath

    if ($LASTEXITCODE -ne 0) {
        throw "Errore durante il publish"
    }

    Write-Host "✓ Publish completato" -ForegroundColor Green
} finally {
    Pop-Location
}
Write-Host ""

# ============================================================================
# STEP 8: Compila Installer
# ============================================================================
Write-Host "📦 STEP 8: Compilazione installer con Inno Setup..." -ForegroundColor Yellow

$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $innoSetupPath)) {
    Write-Host "⚠ Inno Setup non trovato in: $innoSetupPath" -ForegroundColor Red
    Write-Host "Scaricalo da: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

& $innoSetupPath $issPath

$installerName = "TaoGEST-Setup-v$NewVersion.exe"
$installerPath = Join-Path $publishPath $installerName

if (-not (Test-Path $installerPath)) {
    throw "Installer non trovato: $installerPath"
}

Write-Host "✓ Installer creato: $installerName" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 9: Calcola SHA256
# ============================================================================
Write-Host "🔐 STEP 9: Calcolo hash SHA256..." -ForegroundColor Yellow

$hash = (Get-FileHash -Path $installerPath -Algorithm SHA256).Hash
$fileSize = (Get-Item $installerPath).Length

Write-Host "✓ SHA256: $hash" -ForegroundColor Green
Write-Host "✓ Dimensione: $([Math]::Round($fileSize / 1MB, 2)) MB" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 10: Aggiorna version.json
# ============================================================================
Write-Host "📦 STEP 10: Aggiornamento version.json..." -ForegroundColor Yellow

$versionJsonPath = Join-Path $RootPath "version.json"
$downloadUrl = "https://github.com/zndr/claudeWarfarinManager/releases/download/v$NewVersion/$installerName"

$versionJson = @{
    Version = $NewVersion
    DownloadUrl = $downloadUrl
    ReleaseDate = $currentDate
    FileSize = $fileSize
    Sha256Hash = $hash
    ReleaseNotes = $ReleaseNotes
} | ConvertTo-Json -Depth 10

Set-Content -Path $versionJsonPath -Value $versionJson -NoNewline
Write-Host "✓ version.json aggiornato" -ForegroundColor Green
Write-Host ""

# ============================================================================
# STEP 11: Riepilogo e istruzioni finali
# ============================================================================
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ✓ PREPARAZIONE COMPLETATA!                   ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "📋 RIEPILOGO:" -ForegroundColor Cyan
Write-Host "   Versione:        $NewVersion" -ForegroundColor White
Write-Host "   Installer:       $installerPath" -ForegroundColor White
Write-Host "   SHA256:          $hash" -ForegroundColor White
Write-Host "   Dimensione:      $([Math]::Round($fileSize / 1MB, 2)) MB" -ForegroundColor White
Write-Host ""

Write-Host "📝 PROSSIMI PASSI MANUALI:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   1. Verifica che tutto sia corretto" -ForegroundColor White
Write-Host "      - Controlla i file modificati con: git status" -ForegroundColor Gray
Write-Host "      - Testa l'installer manualmente" -ForegroundColor Gray
Write-Host ""
Write-Host "   2. Commit e Push" -ForegroundColor White
Write-Host "      git add -A" -ForegroundColor Gray
Write-Host "      git commit -m 'chore: Preparazione release v$NewVersion'" -ForegroundColor Gray
Write-Host "      git push" -ForegroundColor Gray
Write-Host ""
Write-Host "   3. Crea Tag" -ForegroundColor White
Write-Host "      git tag -a v$NewVersion -m 'Release v$NewVersion'" -ForegroundColor Gray
Write-Host "      git push origin v$NewVersion" -ForegroundColor Gray
Write-Host ""
Write-Host "   4. Crea Release GitHub" -ForegroundColor White
Write-Host "      gh release create v$NewVersion $installerPath --title 'TaoGEST v$NewVersion' --notes '$($ReleaseNotes.Replace("`n", " "))'" -ForegroundColor Gray
Write-Host ""
Write-Host "   5. Verifica auto-updater" -ForegroundColor White
Write-Host "      - Apri TaoGEST" -ForegroundColor Gray
Write-Host "      - Vai in Strumenti > Verifica aggiornamenti" -ForegroundColor Gray
Write-Host "      - Controlla che rilevi la nuova versione" -ForegroundColor Gray
Write-Host ""

Write-Host "✅ Tutto pronto per la release!" -ForegroundColor Green
