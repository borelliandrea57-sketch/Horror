@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Installa .NET SDK 8 prima di continuare: https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)
set "GODOT=%~1"
if not defined GODOT set /p "GODOT=Percorso completo di Godot 4.7.2 .NET exe console: "
set "GODOT=%GODOT:"=%"
if not exist "%GODOT%" (
  echo Eseguibile Godot non trovato. Usa la versione .NET con template di esportazione installati.
  pause
  exit /b 1
)
dotnet run --project CoreTests\LaTerzaCampana.CoreTests.csproj
if errorlevel 1 goto fail
"%GODOT%" --headless --path "%CD%\GodotProject" --editor --import
if errorlevel 1 goto fail
dotnet build GodotProject\LaTerzaCampana.csproj -c Debug
if errorlevel 1 goto fail
if not exist ReleaseWindows mkdir ReleaseWindows
"%GODOT%" --headless --path "%CD%\GodotProject" --export-release "Windows Desktop" "%CD%\ReleaseWindows\LaTerzaCampana.exe"
if errorlevel 1 goto fail
if not exist ReleaseWindows\LaTerzaCampana.exe goto fail
echo Esportazione completata. Mantieni tutti i file di ReleaseWindows insieme.
pause
exit /b 0
:fail
echo Compilazione non riuscita. Controlla il log e la versione di Godot .NET/template.
pause
exit /b 1
