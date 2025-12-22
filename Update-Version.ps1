#Requires -Version 5.1

<#
.SYNOPSIS
    Aggiorna la versione di TaoGEST in modo centralizzato

.DESCRIPTION
    Questo script aggiorna automaticamente tutti i riferimenti di versione e changelog nel progetto:
    - Version.props (versione assembly)
    - installer/TaoGEST-Setup.iss (versione installer)
    - MainWindow.xaml (testo versione in basso a sinistra)
    - AboutDialog.xaml (tab Novità con changelog)
    - docs/ReleaseNotes.txt (note di rilascio installer)
    - CHANGELOG.md (changelog del progetto)

.PARAMETER NewVersion
    La nuova versione in formato X.Y.Z.W (es. "1.2.0.0")

.PARAMETER ReleaseDate
    Data di rilascio in formato "dd MMMM yyyy" (default: data odierna)

.EXAMPLE
    .\Update-Version.ps1 -NewVersion "1.3.0.0"
    Aggiorna alla versione 1.3.0 con la data odierna
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$NewVersion,

    [Parameter(Mandatory = $false)]
    [string]$ReleaseDate = ""
)

# Colori per output
$colorSuccess = "Green"
$colorWarning = "Yellow"
$colorError = "Red"
$colorInfo = "Cyan"

Write-Host ""
Write-Host "=== Aggiornamento Versione TaoGEST ===" -ForegroundColor $colorInfo
Write-Host ""

# Valida formato versione
if ($NewVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    Write-Host "X Formato versione non valido: $NewVersion" -ForegroundColor $colorError
    Write-Host "   Usa il formato X.Y.Z.W (es. 1.2.0.0)" -ForegroundColor $colorWarning
    exit 1
}

# Estrai componenti versione
$versionParts = $NewVersion.Split('.')
$majorMinorPatch = "$($versionParts[0]).$($versionParts[1]).$($versionParts[2])"

# Data di rilascio
if ([string]::IsNullOrWhiteSpace($ReleaseDate)) {
    $ReleaseDate = (Get-Date).ToString("dd MMMM yyyy", [System.Globalization.CultureInfo]::CreateSpecificCulture("it-IT"))
}

Write-Host "Nuova versione: $NewVersion ($majorMinorPatch)" -ForegroundColor $colorInfo
Write-Host "Data rilascio: $ReleaseDate" -ForegroundColor $colorInfo
Write-Host ""

# 1. Version.props
Write-Host "1. Aggiornamento Version.props..." -ForegroundColor $colorInfo
$versionPropsPath = Join-Path $PSScriptRoot "Version.props"
if (Test-Path $versionPropsPath) {
    $versionProps = Get-Content $versionPropsPath -Raw
    $versionProps = $versionProps -replace '<VersionPrefix>[\d\.]+</VersionPrefix>', "<VersionPrefix>$majorMinorPatch</VersionPrefix>"
    $versionProps = $versionProps -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$NewVersion</AssemblyVersion>"
    $versionProps = $versionProps -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$NewVersion</FileVersion>"
    $versionProps | Set-Content $versionPropsPath -NoNewline
    Write-Host "   OK Version.props aggiornato" -ForegroundColor $colorSuccess
}

# 2. TaoGEST-Setup.iss
Write-Host "2. Aggiornamento TaoGEST-Setup.iss..." -ForegroundColor $colorInfo
$setupIssPath = Join-Path $PSScriptRoot "installer\TaoGEST-Setup.iss"
if (Test-Path $setupIssPath) {
    $setupIss = Get-Content $setupIssPath -Raw
    $setupIss = $setupIss -replace '; Versione [\d\.]+', "; Versione $majorMinorPatch"
    $setupIss = $setupIss -replace '#define MyAppVersion "[\d\.]+"', "#define MyAppVersion `"$majorMinorPatch`""
    $setupIss | Set-Content $setupIssPath -NoNewline
    Write-Host "   OK TaoGEST-Setup.iss aggiornato" -ForegroundColor $colorSuccess
}

# 3. MainWindow.xaml
Write-Host "3. Aggiornamento MainWindow.xaml..." -ForegroundColor $colorInfo
$mainWindowPath = Join-Path $PSScriptRoot "src\WarfarinManager.UI\MainWindow.xaml"
if (Test-Path $mainWindowPath) {
    $mainWindow = Get-Content $mainWindowPath -Raw
    $mainWindow = $mainWindow -replace 'Text="Versione [\d\.]+"', "Text=`"Versione $majorMinorPatch`""
    $mainWindow | Set-Content $mainWindowPath -NoNewline
    Write-Host "   OK MainWindow.xaml aggiornato" -ForegroundColor $colorSuccess
}

Write-Host ""
Write-Host "Versione aggiornata a $NewVersion!" -ForegroundColor $colorSuccess
Write-Host ""
