@echo off
REM Copyright 2026 Julien Bombled
REM Licensed under the Apache License, Version 2.0
REM
REM Local convenience launcher: builds ThemeForge in Release and runs
REM the Studio. Double-click friendly; pauses on failure so the error
REM stays visible.
setlocal

pushd "%~dp0"

echo [ThemeForge] Building Release...
dotnet build -c Release ThemeForge.slnx
if errorlevel 1 (
    echo.
    echo [ThemeForge] Build failed.
    pause
    popd
    exit /b 1
)

echo.
echo [ThemeForge] Launching Studio...
dotnet run --project src\ThemeForge.Studio\ThemeForge.Studio.csproj -c Release --no-build
set EXITCODE=%ERRORLEVEL%

if not "%EXITCODE%"=="0" (
    echo.
    echo [ThemeForge] Studio exited with code %EXITCODE%.
    pause
)

popd
exit /b %EXITCODE%
