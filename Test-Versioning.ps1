# ============================================================
# TaoGEST - Test Sistema Versioning
# ============================================================

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     TaoGEST - Verifica Sistema Versioning        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$rootPath = $PSScriptRoot
$errors = @()
$warnings = @()
$success = @()

# ============================================================
# 1. Verifica Version.props
# ============================================================
Write-Host "[1/5] Verifica Version.props..." -ForegroundColor Yellow

$versionPropsPath = Join-Path $rootPath "Version.props"
if (Test-Path $versionPropsPath) {
    $content = Get-Content $versionPropsPath -Raw
    
    if ($content -match '<Product>TaoGEST</Product>') {
        $success += "   ✓ Product: TaoGEST"
    } else {
        $errors += "   ✗ Product name non impostato a TaoGEST"
    }
    
    if ($content -match '<AssemblyVersion>([\d\.]+)</AssemblyVersion>') {
        $version = $matches[1]
        $success += "   ✓ AssemblyVersion: $version"
    } else {
        $errors += "   ✗ AssemblyVersion non trovato"
    }
    
    if ($content -match '<FileVersion>([\d\.]+)</FileVersion>') {
        $fileVersion = $matches[1]
        $success += "   ✓ FileVersion: $fileVersion"
    } else {
        $errors += "   ✗ FileVersion non trovato"
    }
    
    if ($version -eq $fileVersion) {
        $success += "   ✓ Versioni coerenti"
    } else {
        $warnings += "   ⚠ AssemblyVersion ($version) ≠ FileVersion ($fileVersion)"
    }
} else {
    $errors += "   ✗ Version.props NON TROVATO!"
}

Write-Host ""

# ============================================================
# 2. Verifica Import nei .csproj
# ============================================================
Write-Host "[2/5] Verifica import Version.props..." -ForegroundColor Yellow

$csprojFiles = @(
    "src\WarfarinManager.UI\WarfarinManager.UI.csproj",
    "src\WarfarinManager.Core\WarfarinManager.Core.csproj",
    "src\WarfarinManager.Data\WarfarinManager.Data.csproj",
    "src\WarfarinManager.Shared\WarfarinManager.Shared.csproj"
)

foreach ($proj in $csprojFiles) {
    $fullPath = Join-Path $rootPath $proj
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        if ($content -match 'Import Project=".*Version\.props"') {
            $success += "   ✓ $(Split-Path $proj -Leaf)"
        } else {
            $errors += "   ✗ $(Split-Path $proj -Leaf) - Import mancante"
        }
    } else {
        $warnings += "   ⚠ $(Split-Path $proj -Leaf) - File non trovato"
    }
}

Write-Host ""

# ============================================================
# 3. Verifica MainWindow.xaml
# ============================================================
Write-Host "[3/5] Verifica MainWindow.xaml..." -ForegroundColor Yellow

$mainWindowPath = Join-Path $rootPath "src\WarfarinManager.UI\MainWindow.xaml"
if (Test-Path $mainWindowPath) {
    $content = Get-Content $mainWindowPath -Raw
    
    if ($content -match 'TaoGEST') {
        $success += "   ✓ Nome prodotto: TaoGEST"
    } else {
        $warnings += "   ⚠ Nome TaoGEST non trovato"
    }
    
    if ($content -match 'Versione ([\d\.]+)') {
        $uiVersion = $matches[1]
        $success += "   ✓ Versione UI: $uiVersion"
    } else {
        $warnings += "   ⚠ Versione non trovata"
    }
} else {
    $errors += "   ✗ MainWindow.xaml NON TROVATO!"
}

Write-Host ""

# ============================================================
# 4. Verifica README.md
# ============================================================
Write-Host "[4/5] Verifica README.md..." -ForegroundColor Yellow

$readmePath = Join-Path $rootPath "README.md"
if (Test-Path $readmePath) {
    $content = Get-Content $readmePath -Raw
    
    if ($content -match '# TaoGEST') {
        $success += "   ✓ Titolo: TaoGEST"
    } else {
        $warnings += "   ⚠ Titolo non aggiornato"
    }
} else {
    $errors += "   ✗ README.md NON TROVATO!"
}

Write-Host ""

# ============================================================
# 5. Verifica Scripts
# ============================================================
Write-Host "[5/5] Verifica script..." -ForegroundColor Yellow

$scripts = @("Update-Version.ps1", "Test-Versioning.ps1")
foreach ($script in $scripts) {
    if (Test-Path (Join-Path $rootPath $script)) {
        $success += "   ✓ $script"
    } else {
        $warnings += "   ⚠ $script non trovato"
    }
}

Write-Host ""

# ============================================================
# Riepilogo
# ============================================================
Write-Host "╔═══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║              RIEPILOGO VERIFICA                   ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if ($success.Count -gt 0) {
    Write-Host "✓ SUCCESSI ($($success.Count)):" -ForegroundColor Green
    foreach ($msg in $success) {
        Write-Host $msg -ForegroundColor Gray
    }
    Write-Host ""
}

if ($warnings.Count -gt 0) {
    Write-Host "⚠ ATTENZIONE ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($msg in $warnings) {
        Write-Host $msg -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($errors.Count -gt 0) {
    Write-Host "✗ ERRORI ($($errors.Count)):" -ForegroundColor Red
    foreach ($msg in $errors) {
        Write-Host $msg -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
if ($errors.Count -eq 0) {
    Write-Host "✓ SISTEMA PRONTO!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Per aggiornare versione:" -ForegroundColor White
    Write-Host "   .\Update-Version.ps1 -NewVersion `"1.1.0.0`"" -ForegroundColor Gray
} else {
    Write-Host "✗ CORREGGERE GLI ERRORI" -ForegroundColor Red
}
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
