@echo off
echo Building RevitAIAssistant for Revit 2025...

:: Set variables
set CONFIG=Debug
set PLATFORM=x64
set PROJECT_PATH=src\RevitAIAssistant\RevitAIAssistant.csproj
set OUTPUT_PATH=src\RevitAIAssistant\bin\Debug
set ADDINS_PATH=%APPDATA%\Autodesk\Revit\Addins\2025

:: Create addins directory if it doesn't exist
if not exist "%ADDINS_PATH%" (
    mkdir "%ADDINS_PATH%"
    echo Created Revit addins directory: %ADDINS_PATH%
)

:: Build the project
echo Building project...
dotnet build "%PROJECT_PATH%" -c %CONFIG% /p:Platform=%PLATFORM%

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build succeeded!
echo.
echo Installing add-in to Revit...

:: Copy files to Revit addins folder
copy /Y "RevitAIAssistant.addin" "%ADDINS_PATH%\" >nul
echo   Copied RevitAIAssistant.addin

copy /Y "%OUTPUT_PATH%\RevitAIAssistant.dll" "%ADDINS_PATH%\" >nul
echo   Copied RevitAIAssistant.dll

:: Copy all other DLLs except Revit API DLLs
for %%f in ("%OUTPUT_PATH%\*.dll") do (
    if not "%%~nf"=="RevitAPI" if not "%%~nf"=="RevitAPIUI" if not "%%~nf"=="RevitAIAssistant" (
        copy /Y "%%f" "%ADDINS_PATH%\" >nul
        echo   Copied %%~nf.dll
    )
)

:: Copy config files if they exist
if exist "%OUTPUT_PATH%\appsettings.json" (
    copy /Y "%OUTPUT_PATH%\appsettings.json" "%ADDINS_PATH%\" >nul
    echo   Copied appsettings.json
)

echo.
echo Installation complete!
echo The AI Engineering Assistant add-in has been installed to:
echo   %ADDINS_PATH%
echo.
echo Restart Revit 2025 to load the add-in.
echo.
pause