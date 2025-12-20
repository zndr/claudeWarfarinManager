@echo off
cd /d D:\Claude\winTaoGest
dotnet build WarfarinManager.sln 2>&1
echo.
echo Return code: %ERRORLEVEL%
pause
