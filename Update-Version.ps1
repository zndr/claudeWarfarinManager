# TaoGEST - Script Aggiornamento Versione
param(
    [Parameter(Mandatory=$true)][string]$NewVersion,
    [Parameter(Mandatory=$false)][string]$VersionSuffix = ""
)

if ($NewVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    Write-Error "Formato non valido. Usare X.Y.Z.W"
    exit 1
}

$versionParts = $NewVersion.Split('.')
$versionPrefix = "$($versionParts[0]).$($versionParts[1]).$($versionParts[2])"

Write-Host "Aggiornamento a versione: $NewVersion" -ForegroundColor Green

# Aggiorna Version.props
$versionPropsPath = Join-Path $PSScriptRoot "Version.props"
if (Test-Path $versionPropsPath) {
    $content = Get-Content $versionPropsPath -Raw
    $content = $content -replace '<VersionPrefix>[\d\.]+</VersionPrefix>', "<VersionPrefix>$versionPrefix</VersionPrefix>"
    $content = $content -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$NewVersion</AssemblyVersion>"
    $content = $content -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$NewVersion</FileVersion>"
    $content | Set-Content $versionPropsPath -NoNewline
    Write-Host "✓ Version.props aggiornato"
}

# Aggiorna MainWindow.xaml
$mainWindowPath = Join-Path $PSScriptRoot "src\WarfarinManager.UI\MainWindow.xaml"
if (Test-Path $mainWindowPath) {
    $content = Get-Content $mainWindowPath -Raw
    $content = $content -replace 'Versione [\d\.]+ - Beta', "Versione $versionPrefix - Beta"
    $content | Set-Content $mainWindowPath -NoNewline
    Write-Host "✓ MainWindow.xaml aggiornato"
}

Write-Host "`nVersione aggiornata a $NewVersion!" -ForegroundColor Cyan
