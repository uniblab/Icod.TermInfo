#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: verify-release-package.sh <artifact-directory> <Debug|Staging|Release>" >&2
}

if (( $# != 2 )); then
  usage
  exit 2
fi

artifact_dir="$1"
configuration="$2"

case "${configuration}" in
  Debug|Staging|Release)
    ;;
  *)
    usage
    exit 2
    ;;
esac

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "${repository_root}"

mkdir -p "${artifact_dir}"
artifact_dir="$(cd "${artifact_dir}" && pwd)"
export ICOD_TERMINFO_ARTIFACT_DIR="${artifact_dir}"
smoke_nuget_config="${repository_root}/.github/scripts/package-smoke.NuGet.Config"

# Repository-only maintenance tools are executed on one explicit framework.
dotnet run \
  --project tools/terminfo-metadata/Icod.TermInfo.MetadataGenerator.csproj \
  -c "${configuration}" \
  -f net10.0 \
  -- --check

# The reviewed 1.0 API baseline must remain exact.
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --check

# The three shipped target frameworks must expose the exact same API.
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   bin/${configuration}/net8.0/Icod.TermInfo.dll   bin/${configuration}/net9.0/Icod.TermInfo.dll
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   bin/${configuration}/net8.0/Icod.TermInfo.dll   bin/${configuration}/net10.0/Icod.TermInfo.dll

# The optional Source package must expose the same API on all shipped frameworks.
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   Icod.TermInfo.Source/bin/${configuration}/net8.0/Icod.TermInfo.Source.dll   Icod.TermInfo.Source/bin/${configuration}/net9.0/Icod.TermInfo.Source.dll
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   Icod.TermInfo.Source/bin/${configuration}/net8.0/Icod.TermInfo.Source.dll   Icod.TermInfo.Source/bin/${configuration}/net10.0/Icod.TermInfo.Source.dll

# The first Source public surface is reviewed and frozen independently.
dotnet run \
  --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \
  -c "${configuration}" \
  --no-build \
  -- --check \
  docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt \
  Icod.TermInfo.Source/bin/${configuration}/net10.0/Icod.TermInfo.Source.dll

# The Compiler package must expose the same API on all shipped frameworks.
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   Icod.TermInfo.Compiler/bin/${configuration}/net8.0/Icod.TermInfo.Compiler.dll   Icod.TermInfo.Compiler/bin/${configuration}/net9.0/Icod.TermInfo.Compiler.dll
dotnet run   --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj   -c "${configuration}"   --no-build   -- --compare   Icod.TermInfo.Compiler/bin/${configuration}/net8.0/Icod.TermInfo.Compiler.dll   Icod.TermInfo.Compiler/bin/${configuration}/net10.0/Icod.TermInfo.Compiler.dll

# C01 freezes the initial Compiler public surface independently.
dotnet run \
  --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \
  -c "${configuration}" \
  --no-build \
  -- --check \
  docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt \
  Icod.TermInfo.Compiler/bin/${configuration}/net10.0/Icod.TermInfo.Compiler.dll

# The Inspection package must expose the same API on all shipped frameworks.
dotnet run \
  --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \
  -c "${configuration}" \
  --no-build \
  -- --compare \
  Icod.TermInfo.Inspection/bin/${configuration}/net8.0/Icod.TermInfo.Inspection.dll \
  Icod.TermInfo.Inspection/bin/${configuration}/net9.0/Icod.TermInfo.Inspection.dll
dotnet run \
  --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \
  -c "${configuration}" \
  --no-build \
  -- --compare \
  Icod.TermInfo.Inspection/bin/${configuration}/net8.0/Icod.TermInfo.Inspection.dll \
  Icod.TermInfo.Inspection/bin/${configuration}/net10.0/Icod.TermInfo.Inspection.dll

# I01 establishes the independent Inspection baseline before public behavior.
dotnet run \
  --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \
  -c "${configuration}" \
  --no-build \
  -- --check \
  docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt \
  Icod.TermInfo.Inspection/bin/${configuration}/net10.0/Icod.TermInfo.Inspection.dll

# Structural package, Source Link, dependency, and architecture verification.
dotnet run \
  --project tools/package-verifier/Icod.TermInfo.PackageVerifier.csproj \
  -c "${configuration}" \
  -f net10.0 \
  -- "${artifact_dir}"
dotnet run \
  --project tools/compiler-package-verifier/Icod.TermInfo.Compiler.PackageVerifier.csproj \
  -c "${configuration}" \
  -f net10.0 \
  -- "${artifact_dir}"

dotnet run \
  --project tools/inspection-package-verifier/Icod.TermInfo.Inspection.PackageVerifier.csproj \
  -c "${configuration}" \
  -f net10.0 \
  -- "${artifact_dir}"

package_version="$(
  dotnet msbuild Icod.TermInfo.csproj \
    -nologo \
    -getProperty:PackageVersion
)"

if [[ -z "${package_version}" ]]; then
  echo "Unable to determine PackageVersion." >&2
  exit 1
fi

source_package_version="$(
  dotnet msbuild Icod.TermInfo.Source/Icod.TermInfo.Source.csproj \
    -nologo \
    -getProperty:PackageVersion
)"

if [[ -z "${source_package_version}" ]]; then
  echo "Unable to determine Icod.TermInfo.Source PackageVersion." >&2
  exit 1
fi

if [[ "${source_package_version}" != "${package_version}" ]]; then
  echo "Icod.TermInfo and Icod.TermInfo.Source PackageVersion values must match." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Source.${source_package_version}.nupkg" ]]; then
  echo "Icod.TermInfo.Source package not found." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Source.${source_package_version}.snupkg" ]]; then
  echo "Icod.TermInfo.Source symbol package not found." >&2
  exit 1
fi

compiler_package_version="$(
  dotnet msbuild Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj \
    -nologo \
    -getProperty:PackageVersion
)"

if [[ -z "${compiler_package_version}" ]]; then
  echo "Unable to determine Icod.TermInfo.Compiler PackageVersion." >&2
  exit 1
fi

if [[ "${compiler_package_version}" != "${package_version}" ]]; then
  echo "Icod.TermInfo and Icod.TermInfo.Compiler PackageVersion values must match." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Compiler.${compiler_package_version}.nupkg" ]]; then
  echo "Icod.TermInfo.Compiler package not found." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Compiler.${compiler_package_version}.snupkg" ]]; then
  echo "Icod.TermInfo.Compiler symbol package not found." >&2
  exit 1
fi

inspection_package_version="$(
  dotnet msbuild Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj \
    -nologo \
    -getProperty:PackageVersion
)"

if [[ -z "${inspection_package_version}" ]]; then
  echo "Unable to determine Icod.TermInfo.Inspection PackageVersion." >&2
  exit 1
fi

if [[ "${inspection_package_version}" != "${package_version}" ]]; then
  echo "Icod.TermInfo and Icod.TermInfo.Inspection PackageVersion values must match." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Inspection.${inspection_package_version}.nupkg" ]]; then
  echo "Icod.TermInfo.Inspection package not found." >&2
  exit 1
fi

if [[ ! -f "${artifact_dir}/Icod.TermInfo.Inspection.${inspection_package_version}.snupkg" ]]; then
  echo "Icod.TermInfo.Inspection symbol package not found." >&2
  exit 1
fi

# Copy the package-reference-only consumer to a temporary directory so the smoke
# test cannot accidentally use a project reference or stale repository outputs.
smoke_root="$(mktemp -d)"
source_smoke_root=""
compiler_smoke_root=""
inspection_smoke_root=""
trap 'rm -rf "${smoke_root}" "${source_smoke_root}" "${compiler_smoke_root}" "${inspection_smoke_root}"' EXIT

cp \
  tools/package-smoke/Icod.TermInfo.PackageSmoke.csproj \
  "${smoke_root}/Icod.TermInfo.PackageSmoke.csproj"
cp \
  tools/package-smoke/Program.cs \
  "${smoke_root}/Program.cs"

export NUGET_PACKAGES="${smoke_root}/packages"

dotnet restore \
  "${smoke_root}/Icod.TermInfo.PackageSmoke.csproj" \
  --configfile "${smoke_nuget_config}" \
  -p:IcodTermInfoPackageVersion="${package_version}"

# The isolated consumer must execute against every supported target framework.
dotnet run \
  --project "${smoke_root}/Icod.TermInfo.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net8.0 \
  --no-restore \
  -p:IcodTermInfoPackageVersion="${package_version}"

dotnet run \
  --project "${smoke_root}/Icod.TermInfo.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net9.0 \
  --no-restore \
  -p:IcodTermInfoPackageVersion="${package_version}"
dotnet run \
  --project "${smoke_root}/Icod.TermInfo.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net10.0 \
  --no-restore \
  -p:IcodTermInfoPackageVersion="${package_version}"

source_smoke_root="$(mktemp -d)"

cp \
  tools/source-package-smoke/Icod.TermInfo.Source.PackageSmoke.csproj \
  "${source_smoke_root}/Icod.TermInfo.Source.PackageSmoke.csproj"
cp \
  tools/source-package-smoke/Program.cs \
  "${source_smoke_root}/Program.cs"

export NUGET_PACKAGES="${source_smoke_root}/packages"

dotnet restore \
  "${source_smoke_root}/Icod.TermInfo.Source.PackageSmoke.csproj" \
  --configfile "${smoke_nuget_config}" \
  -p:IcodTermInfoSourcePackageVersion="${source_package_version}"

dotnet run \
  --project "${source_smoke_root}/Icod.TermInfo.Source.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net8.0 \
  --no-restore \
  -p:IcodTermInfoSourcePackageVersion="${source_package_version}"

dotnet run \
  --project "${source_smoke_root}/Icod.TermInfo.Source.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net9.0 \
  --no-restore \
  -p:IcodTermInfoSourcePackageVersion="${source_package_version}"
dotnet run \
  --project "${source_smoke_root}/Icod.TermInfo.Source.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net10.0 \
  --no-restore \
  -p:IcodTermInfoSourcePackageVersion="${source_package_version}"

compiler_smoke_root="$(mktemp -d)"

cp \
  tools/compiler-package-smoke/Icod.TermInfo.Compiler.PackageSmoke.csproj \
  "${compiler_smoke_root}/Icod.TermInfo.Compiler.PackageSmoke.csproj"
cp \
  tools/compiler-package-smoke/Program.cs \
  "${compiler_smoke_root}/Program.cs"

export NUGET_PACKAGES="${compiler_smoke_root}/packages"

dotnet restore \
  "${compiler_smoke_root}/Icod.TermInfo.Compiler.PackageSmoke.csproj" \
  --configfile "${smoke_nuget_config}" \
  -p:IcodTermInfoCompilerPackageVersion="${compiler_package_version}"

dotnet run \
  --project "${compiler_smoke_root}/Icod.TermInfo.Compiler.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net8.0 \
  --no-restore \
  -p:IcodTermInfoCompilerPackageVersion="${compiler_package_version}"
dotnet run \
  --project "${compiler_smoke_root}/Icod.TermInfo.Compiler.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net9.0 \
  --no-restore \
  -p:IcodTermInfoCompilerPackageVersion="${compiler_package_version}"
dotnet run \
  --project "${compiler_smoke_root}/Icod.TermInfo.Compiler.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net10.0 \
  --no-restore \
  -p:IcodTermInfoCompilerPackageVersion="${compiler_package_version}"

inspection_smoke_root="$(mktemp -d)"

cp \
  tools/inspection-package-smoke/Icod.TermInfo.Inspection.PackageSmoke.csproj \
  "${inspection_smoke_root}/Icod.TermInfo.Inspection.PackageSmoke.csproj"
cp \
  tools/inspection-package-smoke/Program.cs \
  "${inspection_smoke_root}/Program.cs"

export NUGET_PACKAGES="${inspection_smoke_root}/packages"

dotnet restore \
  "${inspection_smoke_root}/Icod.TermInfo.Inspection.PackageSmoke.csproj" \
  --configfile "${smoke_nuget_config}" \
  -p:IcodTermInfoInspectionPackageVersion="${inspection_package_version}"

dotnet run \
  --project "${inspection_smoke_root}/Icod.TermInfo.Inspection.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net8.0 \
  --no-restore \
  -p:IcodTermInfoInspectionPackageVersion="${inspection_package_version}"
dotnet run \
  --project "${inspection_smoke_root}/Icod.TermInfo.Inspection.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net9.0 \
  --no-restore \
  -p:IcodTermInfoInspectionPackageVersion="${inspection_package_version}"
dotnet run \
  --project "${inspection_smoke_root}/Icod.TermInfo.Inspection.PackageSmoke.csproj" \
  -c "${configuration}" \
  -f net10.0 \
  --no-restore \
  -p:IcodTermInfoInspectionPackageVersion="${inspection_package_version}"

# The repository sample must retain a non-interactive path suitable for CI.
dotnet run \
  --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj \
  -c "${configuration}" \
  -f net10.0 \
  -- --describe-only --profile ms-terminal-direct
