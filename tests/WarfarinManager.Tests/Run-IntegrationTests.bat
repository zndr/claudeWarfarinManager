@echo off
REM Simple batch script to run integration tests
REM Run: Run-IntegrationTests.bat

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║     WarfarinManager - Integration Tests                       ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

echo [*] Restoring packages...
dotnet restore WarfarinManager.Tests.csproj --verbosity quiet

if %ERRORLEVEL% NEQ 0 (
    echo [!] Package restore failed!
    exit /b 1
)

echo [✓] Packages restored
echo.

echo [*] Building test project...
dotnet build WarfarinManager.Tests.csproj --configuration Debug --no-restore --verbosity quiet

if %ERRORLEVEL% NEQ 0 (
    echo [!] Build failed!
    exit /b 1
)

echo [✓] Build successful
echo.

echo [*] Running integration tests...
echo.
dotnet test WarfarinManager.Tests.csproj --no-build --verbosity normal --filter "FullyQualifiedName~Integration"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ═══════════════════════════════════════════════════════════════
    echo [✓] All tests PASSED!
    echo ═══════════════════════════════════════════════════════════════
) else (
    echo.
    echo ═══════════════════════════════════════════════════════════════
    echo [!] Tests FAILED - Check output above
    echo ═══════════════════════════════════════════════════════════════
)

echo.
pause
