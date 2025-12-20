# Script per eseguire tutti i test di integrazione con reporting completo
# Run: .\Run-IntegrationTests.ps1

param(
    [switch]$Coverage,
    [switch]$Verbose,
    [string]$Filter = "Integration"
)

$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     WarfarinManager - Integration Tests Execution             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Paths
$SolutionRoot = Split-Path -Parent $PSScriptRoot
$SolutionRoot = Split-Path -Parent $SolutionRoot
$TestProject = Join-Path $PSScriptRoot "WarfarinManager.Tests.csproj"

Write-Host "📁 Solution Root: $SolutionRoot" -ForegroundColor Gray
Write-Host "🧪 Test Project: $TestProject" -ForegroundColor Gray
Write-Host ""

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $TestProject --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Packages restored successfully" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "🔨 Building test project..." -ForegroundColor Yellow
dotnet build $TestProject --configuration Debug --no-restore --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "🧪 Running integration tests..." -ForegroundColor Yellow
Write-Host "   Filter: *$Filter*" -ForegroundColor Gray
Write-Host ""

$TestCommand = "dotnet test `"$TestProject`" --no-build --verbosity normal --filter `"FullyQualifiedName~$Filter`""

if ($Coverage) {
    Write-Host "📊 Code coverage enabled" -ForegroundColor Cyan
    $TestCommand += " /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./coverage/"
}

if ($Verbose) {
    $TestCommand = $TestCommand -replace "--verbosity normal", "--verbosity detailed"
}

# Execute
$StartTime = Get-Date
Invoke-Expression $TestCommand
$TestExitCode = $LASTEXITCODE
$EndTime = Get-Date
$Duration = ($EndTime - $StartTime).TotalSeconds

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($TestExitCode -eq 0) {
    Write-Host "✅ All tests PASSED!" -ForegroundColor Green
    Write-Host "⏱️  Duration: $([math]::Round($Duration, 2))s" -ForegroundColor Gray
    
    # Performance check
    if ($Duration -lt 30) {
        Write-Host "⚡ Performance: EXCELLENT (< 30s)" -ForegroundColor Green
    } elseif ($Duration -lt 60) {
        Write-Host "⚠️  Performance: ACCEPTABLE (30-60s)" -ForegroundColor Yellow
    } else {
        Write-Host "🐌 Performance: SLOW (> 60s)" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Tests FAILED!" -ForegroundColor Red
    Write-Host "   Check output above for details" -ForegroundColor Yellow
}

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Coverage report
if ($Coverage) {
    $CoverageFile = Join-Path $PSScriptRoot "coverage\coverage.cobertura.xml"
    
    if (Test-Path $CoverageFile) {
        Write-Host "📊 Coverage report generated:" -ForegroundColor Cyan
        Write-Host "   $CoverageFile" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   To view: Install 'Coverage Gutters' VS Code extension" -ForegroundColor Gray
        Write-Host "   Or use: reportgenerator -reports:$CoverageFile -targetdir:coverage/html" -ForegroundColor Gray
    }
}

# Summary recommendations
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan

if ($TestExitCode -eq 0) {
    Write-Host "   ✅ All tests passing - Ready to proceed to UI layer!" -ForegroundColor Green
    Write-Host "   💡 Consider running: .\Run-IntegrationTests.ps1 -Coverage" -ForegroundColor Gray
} else {
    Write-Host "   🔧 Fix failing tests before proceeding" -ForegroundColor Yellow
    Write-Host "   📝 Check test output for error details" -ForegroundColor Gray
    Write-Host "   🐛 Use 'dotnet test --logger:console;verbosity=detailed' for more info" -ForegroundColor Gray
}

Write-Host ""

exit $TestExitCode
