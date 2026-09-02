@echo off
setlocal EnableExtensions

if "%~1"=="" goto usage
if "%~2"=="" goto usage
if not "%~3"=="" goto usage

set "ARTIFACT_DIR=%~1"
set "CONFIGURATION=%~2"

if /I "%CONFIGURATION%"=="Debug" (
    set "CONFIGURATION=Debug"
) else if /I "%CONFIGURATION%"=="Staging" (
    set "CONFIGURATION=Staging"
) else if /I "%CONFIGURATION%"=="Release" (
    set "CONFIGURATION=Release"
) else (
    goto usage
)

pushd "%~dp0\..\.." >nul || exit /b 1

set "RESULT=0"
if not exist "%ARTIFACT_DIR%" mkdir "%ARTIFACT_DIR%" || goto fail
for %%I in ("%ARTIFACT_DIR%") do set "ARTIFACT_DIR=%%~fI"
set "ICOD_TERMINFO_ARTIFACT_DIR=%ARTIFACT_DIR%"
set "SMOKE_NUGET_CONFIG=%~dp0package-smoke.NuGet.Config"

echo.
echo === Verify generated capability metadata (%CONFIGURATION%) ===
dotnet run --project tools\terminfo-metadata\Icod.TermInfo.MetadataGenerator.csproj -c %CONFIGURATION% -f net10.0 -- --check
if errorlevel 1 goto fail

echo.
echo === Verify approved public API baseline (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --check
if errorlevel 1 goto fail

echo.
echo === Verify net8.0/net9.0/net10.0 API equivalence (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare bin\%CONFIGURATION%\net8.0\Icod.TermInfo.dll bin\%CONFIGURATION%\net9.0\Icod.TermInfo.dll
if errorlevel 1 goto fail
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare bin\%CONFIGURATION%\net8.0\Icod.TermInfo.dll bin\%CONFIGURATION%\net10.0\Icod.TermInfo.dll
if errorlevel 1 goto fail

echo.
echo === Verify Icod.TermInfo.Source net8.0/net9.0/net10.0 API equivalence (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Source\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Source.dll Icod.TermInfo.Source\bin\%CONFIGURATION%\net9.0\Icod.TermInfo.Source.dll
if errorlevel 1 goto fail
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Source\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Source.dll Icod.TermInfo.Source\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Source.dll
if errorlevel 1 goto fail

echo.
echo === Verify approved Icod.TermInfo.Source public API baseline (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --check docs\1.1.0-SOURCE-PUBLIC-API-BASELINE.txt Icod.TermInfo.Source\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Source.dll
if errorlevel 1 goto fail

echo.
echo === Verify Icod.TermInfo.Termcap frozen TC08 API (%CONFIGURATION%) ===
set "TERMCAP_API_BASELINE_SHA256=1e24b8a555b506594c58cf58d03bf87b2b60192f6316537cb4200498c6a92ab0"
set "TERMCAP_API_COMPARE=%TEMP%\Icod.TermInfo.Termcap-api-%RANDOM%-%RANDOM%.txt"
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Termcap\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Termcap.dll Icod.TermInfo.Termcap\bin\%CONFIGURATION%\net9.0\Icod.TermInfo.Termcap.dll > "%TERMCAP_API_COMPARE%"
if errorlevel 1 (
    type "%TERMCAP_API_COMPARE%"
    del /q "%TERMCAP_API_COMPARE%" >nul 2>&1
    goto fail
)
type "%TERMCAP_API_COMPARE%"
findstr /x /c:"SHA-256 %TERMCAP_API_BASELINE_SHA256%" "%TERMCAP_API_COMPARE%" >nul
if errorlevel 1 (
    echo Icod.TermInfo.Termcap public API differs from the frozen TC08 reflection-manifest fingerprint. 1>&2
    del /q "%TERMCAP_API_COMPARE%" >nul 2>&1
    goto fail
)
del /q "%TERMCAP_API_COMPARE%" >nul 2>&1
set "TERMCAP_API_COMPARE="
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Termcap\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Termcap.dll Icod.TermInfo.Termcap\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Termcap.dll
if errorlevel 1 goto fail

echo.
echo === Verify Icod.TermInfo.Compiler net8.0/net9.0/net10.0 API equivalence (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Compiler\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Compiler.dll Icod.TermInfo.Compiler\bin\%CONFIGURATION%\net9.0\Icod.TermInfo.Compiler.dll
if errorlevel 1 goto fail
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Compiler\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Compiler.dll Icod.TermInfo.Compiler\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Compiler.dll
if errorlevel 1 goto fail

echo.
echo === Verify approved Icod.TermInfo.Compiler public API baseline (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --check docs\1.2.0-COMPILER-PUBLIC-API-BASELINE.txt Icod.TermInfo.Compiler\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Compiler.dll
if errorlevel 1 goto fail

echo.
echo === Verify Icod.TermInfo.Inspection net8.0/net9.0/net10.0 API equivalence (%CONFIGURATION%) ===
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Inspection\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Inspection.dll Icod.TermInfo.Inspection\bin\%CONFIGURATION%\net9.0\Icod.TermInfo.Inspection.dll
if errorlevel 1 goto fail
dotnet run --project tools\public-api-snapshot\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --compare Icod.TermInfo.Inspection\bin\%CONFIGURATION%\net8.0\Icod.TermInfo.Inspection.dll Icod.TermInfo.Inspection\bin\%CONFIGURATION%\net10.0\Icod.TermInfo.Inspection.dll
if errorlevel 1 goto fail

rem The frozen 1.7 Inspection baseline remains immutable historical evidence:
rem docs\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem RP01 begins additive 1.8 Inspection API development. Cross-framework equality
rem remains enforced above; RP08 freezes the exact stable 1.8 Inspection baseline.

echo.
echo === Verify package structure and symbols (%CONFIGURATION%) ===
dotnet run --project tools\package-verifier\Icod.TermInfo.PackageVerifier.csproj -c %CONFIGURATION% -f net10.0 -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail
dotnet run --project tools\termcap-package-verifier\Icod.TermInfo.Termcap.PackageVerifier.csproj -c %CONFIGURATION% -f net10.0 -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail
dotnet run --project tools\compiler-package-verifier\Icod.TermInfo.Compiler.PackageVerifier.csproj -c %CONFIGURATION% -f net10.0 -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail

dotnet run --project tools\inspection-package-verifier\Icod.TermInfo.Inspection.PackageVerifier.csproj -c %CONFIGURATION% -f net10.0 -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail

dotnet run --project tools\tool-package-verifier\Icod.TermInfo.Tools.PackageVerifier.csproj -c %CONFIGURATION% -f net10.0 -- "%ARTIFACT_DIR%"
if errorlevel 1 goto fail

set "PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.csproj -nologo -getProperty:PackageVersion') do set "PACKAGE_VERSION=%%V"
if not defined PACKAGE_VERSION (
    echo Unable to determine PackageVersion. 1>&2
    goto fail
)

set "SOURCE_PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.Source\Icod.TermInfo.Source.csproj -nologo -getProperty:PackageVersion') do set "SOURCE_PACKAGE_VERSION=%%V"
if not defined SOURCE_PACKAGE_VERSION (
    echo Unable to determine Icod.TermInfo.Source PackageVersion. 1>&2
    goto fail
)
if not "%SOURCE_PACKAGE_VERSION%"=="%PACKAGE_VERSION%" (
    echo Icod.TermInfo and Icod.TermInfo.Source PackageVersion values must match. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Source.%SOURCE_PACKAGE_VERSION%.nupkg" (
    echo Icod.TermInfo.Source package not found. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Source.%SOURCE_PACKAGE_VERSION%.snupkg" (
    echo Icod.TermInfo.Source symbol package not found. 1>&2
    goto fail
)

set "COMPILER_PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.Compiler\Icod.TermInfo.Compiler.csproj -nologo -getProperty:PackageVersion') do set "COMPILER_PACKAGE_VERSION=%%V"
if not defined COMPILER_PACKAGE_VERSION (
    echo Unable to determine Icod.TermInfo.Compiler PackageVersion. 1>&2
    goto fail
)
if not "%COMPILER_PACKAGE_VERSION%"=="%PACKAGE_VERSION%" (
    echo Icod.TermInfo and Icod.TermInfo.Compiler PackageVersion values must match. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Compiler.%COMPILER_PACKAGE_VERSION%.nupkg" (
    echo Icod.TermInfo.Compiler package not found. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Compiler.%COMPILER_PACKAGE_VERSION%.snupkg" (
    echo Icod.TermInfo.Compiler symbol package not found. 1>&2
    goto fail
)

set "TERMCAP_PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.Termcap\Icod.TermInfo.Termcap.csproj -nologo -getProperty:PackageVersion') do set "TERMCAP_PACKAGE_VERSION=%%V"
if not defined TERMCAP_PACKAGE_VERSION (
    echo Unable to determine Icod.TermInfo.Termcap PackageVersion. 1>&2
    goto fail
)
if not "%TERMCAP_PACKAGE_VERSION%"=="%PACKAGE_VERSION%" (
    echo Icod.TermInfo and Icod.TermInfo.Termcap PackageVersion values must match. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Termcap.%TERMCAP_PACKAGE_VERSION%.nupkg" (
    echo Icod.TermInfo.Termcap package not found. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Termcap.%TERMCAP_PACKAGE_VERSION%.snupkg" (
    echo Icod.TermInfo.Termcap symbol package not found. 1>&2
    goto fail
)

set "INSPECTION_PACKAGE_VERSION="
for /f "delims=" %%V in ('dotnet msbuild Icod.TermInfo.Inspection\Icod.TermInfo.Inspection.csproj -nologo -getProperty:PackageVersion') do set "INSPECTION_PACKAGE_VERSION=%%V"
if not defined INSPECTION_PACKAGE_VERSION (
    echo Unable to determine Icod.TermInfo.Inspection PackageVersion. 1>&2
    goto fail
)
if not "%INSPECTION_PACKAGE_VERSION%"=="%PACKAGE_VERSION%" (
    echo Icod.TermInfo and Icod.TermInfo.Inspection PackageVersion values must match. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Inspection.%INSPECTION_PACKAGE_VERSION%.nupkg" (
    echo Icod.TermInfo.Inspection package not found. 1>&2
    goto fail
)
if not exist "%ARTIFACT_DIR%\Icod.TermInfo.Inspection.%INSPECTION_PACKAGE_VERSION%.snupkg" (
    echo Icod.TermInfo.Inspection symbol package not found. 1>&2
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
echo === Fresh package consumer: net8.0 ===
dotnet restore "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" --configfile "%SMOKE_NUGET_CONFIG%" -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

dotnet run --project "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" -c %CONFIGURATION% -f net8.0 --no-restore -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh package consumer: net9.0 ===
dotnet run --project "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" -c %CONFIGURATION% -f net9.0 --no-restore -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh package consumer: net10.0 ===
dotnet run --project "%SMOKE_ROOT%\Icod.TermInfo.PackageSmoke.csproj" -c %CONFIGURATION% -f net10.0 --no-restore -p:IcodTermInfoPackageVersion=%PACKAGE_VERSION%
if errorlevel 1 goto fail

set "SOURCE_SMOKE_ROOT=%TEMP%\Icod.TermInfo.Source-package-smoke-%RANDOM%-%RANDOM%"
if exist "%SOURCE_SMOKE_ROOT%" rmdir /s /q "%SOURCE_SMOKE_ROOT%"
mkdir "%SOURCE_SMOKE_ROOT%" || goto fail

copy /y tools\source-package-smoke\Icod.TermInfo.Source.PackageSmoke.csproj "%SOURCE_SMOKE_ROOT%\Icod.TermInfo.Source.PackageSmoke.csproj" >nul || goto fail
copy /y tools\source-package-smoke\Program.cs "%SOURCE_SMOKE_ROOT%\Program.cs" >nul || goto fail

set "NUGET_PACKAGES=%SOURCE_SMOKE_ROOT%\packages"

echo.
echo === Fresh Icod.TermInfo.Source package consumer: net8.0 ===
dotnet restore "%SOURCE_SMOKE_ROOT%\Icod.TermInfo.Source.PackageSmoke.csproj" --configfile "%SMOKE_NUGET_CONFIG%" -p:IcodTermInfoSourcePackageVersion=%SOURCE_PACKAGE_VERSION%
if errorlevel 1 goto fail

dotnet run --project "%SOURCE_SMOKE_ROOT%\Icod.TermInfo.Source.PackageSmoke.csproj" -c %CONFIGURATION% -f net8.0 --no-restore -p:IcodTermInfoSourcePackageVersion=%SOURCE_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Source package consumer: net9.0 ===
dotnet run --project "%SOURCE_SMOKE_ROOT%\Icod.TermInfo.Source.PackageSmoke.csproj" -c %CONFIGURATION% -f net9.0 --no-restore -p:IcodTermInfoSourcePackageVersion=%SOURCE_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Source package consumer: net10.0 ===
dotnet run --project "%SOURCE_SMOKE_ROOT%\Icod.TermInfo.Source.PackageSmoke.csproj" -c %CONFIGURATION% -f net10.0 --no-restore -p:IcodTermInfoSourcePackageVersion=%SOURCE_PACKAGE_VERSION%
if errorlevel 1 goto fail

set "TERMCAP_SMOKE_ROOT=%TEMP%\Icod.TermInfo.Termcap-package-smoke-%RANDOM%-%RANDOM%"
if exist "%TERMCAP_SMOKE_ROOT%" rmdir /s /q "%TERMCAP_SMOKE_ROOT%"
mkdir "%TERMCAP_SMOKE_ROOT%" || goto fail

copy /y tools\termcap-package-smoke\Icod.TermInfo.Termcap.PackageSmoke.csproj "%TERMCAP_SMOKE_ROOT%\Icod.TermInfo.Termcap.PackageSmoke.csproj" >nul || goto fail
copy /y tools\termcap-package-smoke\Program.cs "%TERMCAP_SMOKE_ROOT%\Program.cs" >nul || goto fail

set "NUGET_PACKAGES=%TERMCAP_SMOKE_ROOT%\packages"

echo.
echo === Fresh Icod.TermInfo.Termcap package consumer: net8.0 ===
dotnet restore "%TERMCAP_SMOKE_ROOT%\Icod.TermInfo.Termcap.PackageSmoke.csproj" --configfile "%SMOKE_NUGET_CONFIG%" -p:IcodTermInfoTermcapPackageVersion=%TERMCAP_PACKAGE_VERSION%
if errorlevel 1 goto fail
dotnet run --project "%TERMCAP_SMOKE_ROOT%\Icod.TermInfo.Termcap.PackageSmoke.csproj" -c %CONFIGURATION% -f net8.0 --no-restore -p:IcodTermInfoTermcapPackageVersion=%TERMCAP_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Termcap package consumer: net9.0 ===
dotnet run --project "%TERMCAP_SMOKE_ROOT%\Icod.TermInfo.Termcap.PackageSmoke.csproj" -c %CONFIGURATION% -f net9.0 --no-restore -p:IcodTermInfoTermcapPackageVersion=%TERMCAP_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Termcap package consumer: net10.0 ===
dotnet run --project "%TERMCAP_SMOKE_ROOT%\Icod.TermInfo.Termcap.PackageSmoke.csproj" -c %CONFIGURATION% -f net10.0 --no-restore -p:IcodTermInfoTermcapPackageVersion=%TERMCAP_PACKAGE_VERSION%
if errorlevel 1 goto fail

set "COMPILER_SMOKE_ROOT=%TEMP%\Icod.TermInfo.Compiler-package-smoke-%RANDOM%-%RANDOM%"
if exist "%COMPILER_SMOKE_ROOT%" rmdir /s /q "%COMPILER_SMOKE_ROOT%"
mkdir "%COMPILER_SMOKE_ROOT%" || goto fail

copy /y tools\compiler-package-smoke\Icod.TermInfo.Compiler.PackageSmoke.csproj "%COMPILER_SMOKE_ROOT%\Icod.TermInfo.Compiler.PackageSmoke.csproj" >nul || goto fail
copy /y tools\compiler-package-smoke\Program.cs "%COMPILER_SMOKE_ROOT%\Program.cs" >nul || goto fail

set "NUGET_PACKAGES=%COMPILER_SMOKE_ROOT%\packages"

echo.
echo === Fresh Icod.TermInfo.Compiler package consumer: net8.0 ===
dotnet restore "%COMPILER_SMOKE_ROOT%\Icod.TermInfo.Compiler.PackageSmoke.csproj" --configfile "%SMOKE_NUGET_CONFIG%" -p:IcodTermInfoCompilerPackageVersion=%COMPILER_PACKAGE_VERSION%
if errorlevel 1 goto fail

dotnet run --project "%COMPILER_SMOKE_ROOT%\Icod.TermInfo.Compiler.PackageSmoke.csproj" -c %CONFIGURATION% -f net8.0 --no-restore -p:IcodTermInfoCompilerPackageVersion=%COMPILER_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Compiler package consumer: net9.0 ===
dotnet run --project "%COMPILER_SMOKE_ROOT%\Icod.TermInfo.Compiler.PackageSmoke.csproj" -c %CONFIGURATION% -f net9.0 --no-restore -p:IcodTermInfoCompilerPackageVersion=%COMPILER_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Compiler package consumer: net10.0 ===
dotnet run --project "%COMPILER_SMOKE_ROOT%\Icod.TermInfo.Compiler.PackageSmoke.csproj" -c %CONFIGURATION% -f net10.0 --no-restore -p:IcodTermInfoCompilerPackageVersion=%COMPILER_PACKAGE_VERSION%
if errorlevel 1 goto fail

set "INSPECTION_SMOKE_ROOT=%TEMP%\Icod.TermInfo.Inspection-package-smoke-%RANDOM%-%RANDOM%"
if exist "%INSPECTION_SMOKE_ROOT%" rmdir /s /q "%INSPECTION_SMOKE_ROOT%"
mkdir "%INSPECTION_SMOKE_ROOT%" || goto fail

copy /y tools\inspection-package-smoke\Icod.TermInfo.Inspection.PackageSmoke.csproj "%INSPECTION_SMOKE_ROOT%\Icod.TermInfo.Inspection.PackageSmoke.csproj" >nul || goto fail
copy /y tools\inspection-package-smoke\Program.cs "%INSPECTION_SMOKE_ROOT%\Program.cs" >nul || goto fail

set "NUGET_PACKAGES=%INSPECTION_SMOKE_ROOT%\packages"

echo.
echo === Fresh Icod.TermInfo.Inspection package consumer: net8.0 ===
dotnet restore "%INSPECTION_SMOKE_ROOT%\Icod.TermInfo.Inspection.PackageSmoke.csproj" --configfile "%SMOKE_NUGET_CONFIG%" -p:IcodTermInfoInspectionPackageVersion=%INSPECTION_PACKAGE_VERSION%
if errorlevel 1 goto fail

dotnet run --project "%INSPECTION_SMOKE_ROOT%\Icod.TermInfo.Inspection.PackageSmoke.csproj" -c %CONFIGURATION% -f net8.0 --no-restore -p:IcodTermInfoInspectionPackageVersion=%INSPECTION_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Inspection package consumer: net9.0 ===
dotnet run --project "%INSPECTION_SMOKE_ROOT%\Icod.TermInfo.Inspection.PackageSmoke.csproj" -c %CONFIGURATION% -f net9.0 --no-restore -p:IcodTermInfoInspectionPackageVersion=%INSPECTION_PACKAGE_VERSION%
if errorlevel 1 goto fail

echo.
echo === Fresh Icod.TermInfo.Inspection package consumer: net10.0 ===
dotnet run --project "%INSPECTION_SMOKE_ROOT%\Icod.TermInfo.Inspection.PackageSmoke.csproj" -c %CONFIGURATION% -f net10.0 --no-restore -p:IcodTermInfoInspectionPackageVersion=%INSPECTION_PACKAGE_VERSION%
if errorlevel 1 goto fail

rem Package-smoke consumers intentionally use isolated NuGet caches. Restore the
rem caller's cache before executing repository projects so that isolation does
rem not leak into repository sample/toolchain validation.
if defined OLD_NUGET_PACKAGES (
    set "NUGET_PACKAGES=%OLD_NUGET_PACKAGES%"
) else (
    set "NUGET_PACKAGES="
)

echo.
echo === Non-interactive repository sample ===
dotnet run --project samples\Icod.TermInfo.Sample\Icod.TermInfo.Sample.csproj -c %CONFIGURATION% -f net10.0 --no-build -- --describe-only --profile ms-terminal-direct
if errorlevel 1 goto fail

echo.
echo === Deterministic planning/Source/Compiler/Inspection toolchain sample ===
set "TOOLCHAIN_FIRST=%INSPECTION_SMOKE_ROOT%\rp07-toolchain-first.ti"
set "TOOLCHAIN_SECOND=%INSPECTION_SMOKE_ROOT%\rp07-toolchain-second.ti"
dotnet run --project samples\Icod.TermInfo.Toolchain.Sample\Icod.TermInfo.Toolchain.Sample.csproj -c %CONFIGURATION% -f net10.0 --no-build > "%TOOLCHAIN_FIRST%"
if errorlevel 1 goto fail
dotnet run --project samples\Icod.TermInfo.Toolchain.Sample\Icod.TermInfo.Toolchain.Sample.csproj -c %CONFIGURATION% -f net10.0 --no-build > "%TOOLCHAIN_SECOND%"
if errorlevel 1 goto fail
fc /b "%TOOLCHAIN_FIRST%" "%TOOLCHAIN_SECOND%" >nul
if errorlevel 1 (
    type "%TOOLCHAIN_FIRST%"
    type "%TOOLCHAIN_SECOND%"
    echo Toolchain planning output changed across separate process executions. 1>&2
    goto fail
)
type "%TOOLCHAIN_FIRST%"

goto cleanup

:usage
echo Usage: verify-release-package.cmd ^<artifact-directory^> ^<DEBUG^|Staging^|Release^> 1>&2
exit /b 2

:fail
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" set "RESULT=1"

:cleanup
if defined SMOKE_ROOT if exist "%SMOKE_ROOT%" rmdir /s /q "%SMOKE_ROOT%"
if defined SOURCE_SMOKE_ROOT if exist "%SOURCE_SMOKE_ROOT%" rmdir /s /q "%SOURCE_SMOKE_ROOT%"
if defined TERMCAP_SMOKE_ROOT if exist "%TERMCAP_SMOKE_ROOT%" rmdir /s /q "%TERMCAP_SMOKE_ROOT%"
if defined COMPILER_SMOKE_ROOT if exist "%COMPILER_SMOKE_ROOT%" rmdir /s /q "%COMPILER_SMOKE_ROOT%"
if defined INSPECTION_SMOKE_ROOT if exist "%INSPECTION_SMOKE_ROOT%" rmdir /s /q "%INSPECTION_SMOKE_ROOT%"
if defined OLD_NUGET_PACKAGES (
    set "NUGET_PACKAGES=%OLD_NUGET_PACKAGES%"
) else (
    set "NUGET_PACKAGES="
)
popd >nul
exit /b %RESULT%
