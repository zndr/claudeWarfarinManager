@echo off
REM Script per sincronizzare le guide dalla cartella docs a Resources/Guides

echo === Sincronizzazione Guide TaoGEST ===
echo.

set SOURCE_DIR=D:\Claude\winTaoGest\docs
set TARGET_DIR=D:\Claude\winTaoGest\src\WarfarinManager.UI\Resources\Guides

echo Sorgente: %SOURCE_DIR%
echo Destinazione: %TARGET_DIR%
echo.

REM Verifica esistenza directory
if not exist "%SOURCE_DIR%" (
    echo ERRORE: Directory sorgente non trovata!
    pause
    exit /b 1
)

if not exist "%TARGET_DIR%" (
    echo Creazione directory destinazione...
    mkdir "%TARGET_DIR%"
)

REM Copia i file HTML
echo Copiando file HTML...
xcopy /Y /D "%SOURCE_DIR%\interactions.html" "%TARGET_DIR%\" 2>nul && echo   - interactions.html
xcopy /Y /D "%SOURCE_DIR%\LineeGuida.html" "%TARGET_DIR%\linee-guida-tao.html*" 2>nul && echo   - LineeGuida.html -^> linee-guida-tao.html
xcopy /Y /D "%SOURCE_DIR%\Algoritmo Gestione INR.html" "%TARGET_DIR%\algoritmo-gestione-inr.html*" 2>nul && echo   - Algoritmo Gestione INR.html -^> algoritmo-gestione-inr.html
xcopy /Y /D "%SOURCE_DIR%\infografica-tao.html" "%TARGET_DIR%\" 2>nul && echo   - infografica-tao.html

REM Copia i file PDF
echo.
echo Copiando file PDF...
xcopy /Y /D "%SOURCE_DIR%\LineeGuida.pdf" "%TARGET_DIR%\" 2>nul && echo   - LineeGuida.pdf
xcopy /Y /D "%SOURCE_DIR%\Guida Warfarin per pazienti.pdf" "%TARGET_DIR%\" 2>nul && echo   - Guida Warfarin per pazienti.pdf

echo.
echo === Sincronizzazione completata! ===
echo.
echo Ora esegui il rebuild dell'applicazione in Visual Studio.
echo.
pause
