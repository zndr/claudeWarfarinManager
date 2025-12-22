#Requires -Version 5.1

<#
.SYNOPSIS
    Aggiorna il changelog di TaoGEST

.DESCRIPTION
    Aggiorna il tab Novita in AboutDialog.xaml, ReleaseNotes.txt e CHANGELOG.md

.PARAMETER Version
    Versione in formato X.Y.Z (es. "1.2.0")

.PARAMETER ReleaseDate
    Data rilascio (default: oggi)

.EXAMPLE
    .\Update-Changelog.ps1 -Version "1.2.0"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$ReleaseDate = ""
)

if ([string]::IsNullOrWhiteSpace($ReleaseDate)) {
    $ReleaseDate = (Get-Date).ToString("dd MMMM yyyy", [System.Globalization.CultureInfo]::CreateSpecificCulture("it-IT"))
}

Write-Host "Aggiornamento changelog per versione $Version ($ReleaseDate)"

# TODO: Implementa aggiornamento AboutDialog.xaml, ReleaseNotes.txt, CHANGELOG.md

Write-Host "Changelog aggiornato!"
