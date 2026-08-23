@echo off
setlocal EnableExtensions

pushd "%~dp0\..\.." >nul || exit /b 1

set "RESULT=0"
set "ARTIFACT_DIR=%~1"
if not defined ARTIFACT_DIR set "ARTIFACT_DIR=artifacts"
if not exist "%ARTIFACT_DIR%" mkdir "%ARTIFACT_DIR%" || goto fail
for %%I in ("%ARTIFACT_DIR%") do set "ARTIFACT_DIR=%%~fI"

echo.
echo === Verify generated capability metadata ===
dotnet run --project tools\terminfo-metadata\Icod.TermInfo.MetadataGenerator.csproj -c Release -- --check
if errorlevel 1 goto fail

echo.
echo === Verify package structure and symbols ===
dotnet run --project tools\package-verifier\Icod.TermInfo.PackageVerifier.csproj -c Release -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail

set "PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.csproj -nologo -getProperty:PackageVersion') do set "PACKAGE_VERSION=%%V"
if not defined PACKAGE_VERSION (
    echo Unable to determine PackageVersion. 1>&2
    goto fail
)

set "SMOKE_ROOT=%TEMP%\Icod.TermInfo-package-smoke-%RANDOM%-%RANDOM%"
if exist "%SMOKE_ROOT%" rmdir /s /q "%SMOKE_ROOT%"
mkdir "%SMOKE_ROOT%" || goto fail

copy /y tools\package-smoke\Icod.TermInfo.PackageSmoke.csproj "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" >nul || goto fail
copy /y tools\package-smoke\Program.cs "%SMOKE_ROOT%\Program.cs" >nul || goto fail

set "OLD_NUGET_PACKAGES=%NUGET_PACKAGES%"
set "NUGET_PACKAGES=%SMOKE_ROOT%\packages"

echo.
echo === Fresh package consumer ===
dotnet restore "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" --source "%ARTIFACT_DIR%" -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

dotnet run --project "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" -c Release --no-restore -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Non-interactive repository sample ===
dotnet run --project samples\Icod.TermInfo.Sample\Icod.TermInfo.Sample.csproj -c Release -- --describe-only --profile ms-terminal-direct
if errorlevel 1 goto fail

goto cleanup

:fail
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" set "RESULT=1"

:cleanup
if defined SMOKE_ROOT if exist "%SMOKE_ROOT%" rmdir /s /q "%SMOKE_ROOT%"
if defined OLD_NUGET_PACKAGES (
    set "NUGET_PACKAGES=%OLD_NUGET_PACKAGES%"
) else (
    set "NUGET_PACKAGES="
)
popd >nul
exit /b %RESULT%
