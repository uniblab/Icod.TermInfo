@echo off
setlocal EnableExtensions

if "%~1"=="" goto usage
if "%~2"=="" goto usage
if not "%~3"=="" goto usage

powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0verify-package-artifact.ps1" -ArtifactDirectory "%~1" -Configuration "%~2"
exit /b %errorlevel%

:usage
echo Usage: %~nx0 ^<artifact-directory^> ^<Debug^|Staging^|Release^> 1^>^&2
exit /b 1
