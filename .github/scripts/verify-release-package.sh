#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "${repository_root}"

artifact_dir="${1:-artifacts}"
mkdir -p "${artifact_dir}"
artifact_dir="$(cd "${artifact_dir}" && pwd)"

version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' Icod.TermInfo.csproj | head -n 1)"
package_version="$(sed -n 's:.*<PackageVersion>\(.*\)</PackageVersion>.*:\1:p' Icod.TermInfo.csproj | head -n 1)"

if [[ -z "${version}" || -z "${package_version}" || "${version}" != "${package_version}" ]]; then
  echo "Version and PackageVersion must both be present and identical." >&2
  exit 1
fi

nupkg="${artifact_dir}/Icod.TermInfo.${package_version}.nupkg"
snupkg="${artifact_dir}/Icod.TermInfo.${package_version}.snupkg"

test -f "${nupkg}"
test -f "${snupkg}"

if grep -R -n -E \
  'TerminalProfiles\.|TerminalProfile' \
  src/Parameterization; then
  echo "The generic parameterization layer contains a terminal-profile-specific reference." >&2
  exit 1
fi

python3 - "${nupkg}" "${snupkg}" "${package_version}" <<'PY'
from __future__ import annotations

import re
import sys
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

nupkg = Path(sys.argv[1])
snupkg = Path(sys.argv[2])
expected_version = sys.argv[3]


def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)


with zipfile.ZipFile(nupkg) as package:
    names = set(package.namelist())

    required = {
        "README.md",
        "lib/net10.0/Icod.TermInfo.dll",
        "lib/net10.0/Icod.TermInfo.xml",
    }
    missing = sorted(required - names)
    require(not missing, f"Primary package is missing required entries: {missing}")

    require(
        not any(name.startswith("runtimes/") for name in names),
        "Primary package unexpectedly contains a runtimes/ payload.",
    )
    require(
        not any("/native/" in f"/{name.lower()}/" for name in names),
        "Primary package unexpectedly contains a native payload directory.",
    )

    dlls = sorted(name for name in names if name.lower().endswith(".dll"))
    require(
        dlls == ["lib/net10.0/Icod.TermInfo.dll"],
        f"Primary package contains unexpected DLL payloads: {dlls}",
    )
    require(
        not any(name.lower().endswith((".so", ".dylib", ".a", ".lib")) for name in names),
        "Primary package unexpectedly contains a native library payload.",
    )
    require(
        not any(
            name.startswith(("tests/", "tools/", "fixtures/"))
            or "compiled-terminfo" in name.lower()
            or name.lower().endswith((".ti", ".bin"))
            for name in names
        ),
        "Primary package unexpectedly contains repository-only fixture/tooling data.",
    )

    nuspec_names = [name for name in names if name.lower().endswith(".nuspec")]
    require(len(nuspec_names) == 1, f"Expected one nuspec, found {nuspec_names}")
    root = ET.fromstring(package.read(nuspec_names[0]))
    metadata = next((item for item in root.iter() if local_name(item.tag) == "metadata"), None)
    require(metadata is not None, "Package nuspec has no metadata element.")

    def metadata_text(name: str) -> str | None:
        for item in metadata:
            if local_name(item.tag) == name:
                return item.text
        return None

    require(metadata_text("id") == "Icod.TermInfo", "Unexpected package id.")
    require(metadata_text("version") == expected_version, "Unexpected package version.")

    dependencies = [item for item in metadata.iter() if local_name(item.tag) == "dependency"]
    require(not dependencies, "Icod.TermInfo must not have runtime NuGet dependencies.")

    repository = next(
        (item for item in metadata.iter() if local_name(item.tag) == "repository"),
        None,
    )
    require(repository is not None, "Package metadata has no repository element.")
    require(repository.attrib.get("type") == "git", "Repository metadata is not git.")
    require(
        repository.attrib.get("url") == "https://github.com/uniblab/Icod.TermInfo",
        "Unexpected repository URL in package metadata.",
    )
    commit = repository.attrib.get("commit", "")
    require(
        re.fullmatch(r"[0-9a-fA-F]{40}", commit) is not None,
        f"Repository metadata has an invalid commit id: {commit!r}",
    )

with zipfile.ZipFile(snupkg) as symbols:
    names = set(symbols.namelist())
    pdb_name = "lib/net10.0/Icod.TermInfo.pdb"
    require(pdb_name in names, "Symbol package is missing the portable PDB.")
    pdb = symbols.read(pdb_name)
    require(pdb.startswith(b"BSJB"), "Icod.TermInfo.pdb is not a portable PDB.")
    require(
        b"raw.githubusercontent.com/uniblab/Icod.TermInfo/" in pdb,
        "Portable PDB does not contain the expected GitHub Source Link mapping.",
    )
    require(
        commit.encode("ascii") in pdb,
        "Portable PDB Source Link data does not contain the package repository commit.",
    )

print(f"Verified package structure, dependency closure, symbols, and Source Link for {expected_version}.")
PY

smoke_root="$(mktemp -d)"
trap 'rm -rf "${smoke_root}"' EXIT

cat > "${smoke_root}/NuGet.config" <<EOF2
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-artifacts" value="${artifact_dir}" />
  </packageSources>
</configuration>
EOF2

cat > "${smoke_root}/PackageSmoke.csproj" <<EOF2
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Icod.TermInfo" Version="${package_version}" />
  </ItemGroup>
</Project>
EOF2

cat > "${smoke_root}/Program.cs" <<'EOF2'
using System.Text;
using Icod.TermInfo;

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

TerminalDescription ansi = TerminalDatabase.BuiltIn.Load("ansi");
TerminalDescription vt100 = TerminalDatabase.BuiltIn.Load("vt100");
TerminalDescription vt100Alias = TerminalDatabase.BuiltIn.Load("vt100-am");
TerminalDescription xterm = TerminalDatabase.BuiltIn.Load("xterm");
TerminalDescription xterm16 = TerminalDatabase.BuiltIn.Load("xterm-16color");
TerminalDescription xterm88 = TerminalDatabase.BuiltIn.Load("xterm-88color");
TerminalDescription xterm256 = TerminalDatabase.BuiltIn.Load("xterm-256color");
TerminalDescription xtermDirect = TerminalDatabase.BuiltIn.Load("xterm-direct");
TerminalDescription xtermDirect16 = TerminalDatabase.BuiltIn.Load("xterm-direct16");
TerminalDescription xtermDirect256 = TerminalDatabase.BuiltIn.Load("xterm-direct256");
TerminalDescription winConsole = TerminalDatabase.BuiltIn.Load("winconsole");
TerminalDescription msTerminal = TerminalDatabase.BuiltIn.Load("ms-terminal");
TerminalDescription msTerminalDirect = TerminalDatabase.BuiltIn.Load("ms-terminal-direct");
TerminalDescription dumb = TerminalDatabase.BuiltIn.Load("dumb");

Require(ReferenceEquals(vt100, vt100Alias), "vt100-am must resolve to vt100.");
Require(
    StandardCapabilityCatalog.BooleanCapabilities.Count == 44
        && StandardCapabilityCatalog.NumericCapabilities.Count == 39
        && StandardCapabilityCatalog.StringCapabilities.Count == 414,
    "The complete standard capability catalog is not available from the package.");
StandardCapabilityMetadata<BooleanCapability> amMetadata =
    StandardCapabilityCatalog.GetMetadata(BooleanCapability.AutoRightMargin);
Require(
    amMetadata.ShortName == "am"
        && amMetadata.BinaryIndex == 1
        && (int)BooleanCapability.AutoRightMargin == 0,
    "Managed enum values must remain independent from compiled table indices.");
Require(
    !string.IsNullOrWhiteSpace(msTerminal.Description),
    "Verbose terminal descriptions must be available from the package.");
Require(
    msTerminal.NumericCapabilities.Any(
        pair =>
            pair.Key == NumericCapability.Colors
            && pair.Value == 256),
    "Per-description standard capability enumeration is not usable.");
Require(
    xterm.GetString(StringCapability.EnterCursorAddressingMode) is not null,
    "xterm must advertise cursor-addressing entry.");
Require(
    xterm.GetString(StringCapability.ExitCursorAddressingMode) is not null,
    "xterm must advertise cursor-addressing exit.");
Require(
    xterm.TryGetExtendedString("XM", out _),
    "xterm must carry XM mouse-mode metadata.");
Require(
    xterm.ExpandExtendedString("XM", 1) == "\x1b[?1006;1000h",
    "Extended string expansion changed.");
Require(
    xterm.TryGetExtendedString("BE", out string? pasteEnable)
        && pasteEnable == "\x1b[?2004h",
    "xterm bracketed-paste enable metadata changed.");
Require(
    xterm.TryGetExtendedString("fe", out string? focusEnable)
        && focusEnable == "\x1b[?1004h",
    "xterm focus-enable metadata changed.");
Require(
    xterm.TryGetExtendedString("Ms", out string? clipboard),
    "xterm must carry clipboard metadata.");
Require(
    TermInfoParameterExpander.Expand(clipboard!, "c", "YWJj")
        == "\x1b]52;c;YWJj\x1b\\",
    "xterm clipboard metadata expansion changed.");
Require(ansi.GetNumber(NumericCapability.Colors) == 8, "ANSI must advertise eight colors.");
Require(vt100.GetNumber(NumericCapability.Colors) is null, "VT100 must remain monochrome.");
Require(xterm16.GetNumber(NumericCapability.Colors) == 16, "xterm-16color must advertise 16 colors.");
Require(xterm88.GetNumber(NumericCapability.Colors) == 88, "xterm-88color must advertise 88 colors.");
Require(xterm88.GetNumber(NumericCapability.ColorPairs) == 7744, "xterm-88color must advertise 7744 pairs.");
Require(xterm256.GetNumber(NumericCapability.Colors) == 256, "xterm-256color must advertise 256 colors.");
Require(xterm256.GetNumber(NumericCapability.ColorPairs) == 65536, "xterm-256color must advertise 65536 pairs.");
Require(
    TerminalColors.ExpandForeground(xterm256, 255) == "\x1b[38;5;255m",
    "xterm-256color foreground expansion changed.");
Require(
    xtermDirect.GetNumber(NumericCapability.Colors) == (1 << 24),
    "xterm-direct must advertise the direct RGB color space.");
Require(
    TerminalColors.GetColorSupport(xtermDirect256).Model == TerminalColorModel.DirectRgb,
    "xterm-direct256 must classify as direct RGB.");
Require(
    TerminalColors.GetColorSupport(xtermDirect256).IndexedColorCount == 256,
    "xterm-direct256 must retain 256 indexed colors.");
Require(
    TerminalColors.ExpandForeground(xtermDirect16, 15) == "\x1b[97m",
    "xterm-direct16 indexed foreground expansion changed.");
Require(
    TerminalColors.ExpandForeground(
        xtermDirect256,
        new TerminalRgbColor(0x12, 0x34, 0x56))
        == "\x1b[38:2::18:52:86m",
    "xterm-direct256 RGB foreground expansion changed.");
Require(winConsole.Name == "winconsole", "winconsole must be available.");
Require(msTerminal.Name == "ms-terminal", "ms-terminal must be available.");
Require(
    TerminalColors.GetColorSupport(msTerminal).Model
        == TerminalColorModel.Indexed,
    "ms-terminal must retain indexed-color semantics.");
Require(
    TerminalColors.GetColorSupport(msTerminalDirect).Model
        == TerminalColorModel.DirectRgb,
    "ms-terminal-direct must retain direct-RGB semantics.");
Require(
    !ReferenceEquals(msTerminal, xterm256),
    "Windows Terminal must not be an xterm alias.");
Require(dumb.Name == "dumb", "The dumb fallback profile must be available.");
Require(
    !TerminalDatabase.BuiltIn.TryLoad("xterm-mono", out _),
    "Unselected terminal names must not silently resolve.");
Require(
    !TerminalDatabase.BuiltIn.TryLoad("linux", out _),
    "0.8 must not pretend to provide arbitrary system terminfo identities.");

TermInfoParameterProgram program =
    TermInfoParameterProgram.Parse("%p1%{1}%+%d");
Require(program.Source == "%p1%{1}%+%d", "Parsed program source changed.");
Require(program.Expand(41) == "42", "Reusable parsed-program expansion failed.");

string cup = ansi.Expand(StringCapability.CursorAddress, 0, 0);
Require(cup == "\x1b[1;1H", "ANSI cursor addressing expansion changed.");

using StringWriter writer = new();
TermInfoOutput.PutP(cup, writer);
Require(writer.ToString() == cup, "Padding-free output changed unexpectedly.");

writer.GetStringBuilder().Clear();
TermInfoOutput.PutP(
    vt100.GetRequiredString(StringCapability.ClearScreen),
    writer);
Require(
    !writer.ToString().Contains("$<", StringComparison.Ordinal),
    "Padding annotations must never be emitted literally in ignore mode.");

using MemoryStream capabilityBytes = new();
TermInfoOutput.TPuts(
    "\u0080",
    1,
    capabilityBytes,
    Encoding.Latin1);
byte[] rawCapabilityBytes = capabilityBytes.ToArray();
Require(
    rawCapabilityBytes.Length == 1
        && rawCapabilityBytes[0] == 0x80,
    "Latin-1 capability-byte round-trip changed.");

TerminalDescription xonTerminal =
    new TerminalDescriptionBuilder("package-smoke-xon")
        .SetBoolean(BooleanCapability.XonXoff)
        .Build();
writer.GetStringBuilder().Clear();
TermInfoOutput.TPuts(
    "before$<1>after",
    1,
    writer,
    new TermInfoOutputOptions(xonTerminal));
Require(
    writer.ToString() == "beforeafter",
    "Terminal-aware xon padding suppression changed.");

Require(
    TermInfoCompatibility.TiGetNum(ansi, "cols") == 80,
    "Compatibility capability lookup changed.");

Console.WriteLine("Icod.TermInfo package smoke test passed.");
EOF2

export NUGET_PACKAGES="${smoke_root}/packages"

dotnet restore "${smoke_root}/PackageSmoke.csproj" \
  --configfile "${smoke_root}/NuGet.config"
dotnet run --project "${smoke_root}/PackageSmoke.csproj" \
  -c Release \
  --no-restore

# The repository sample must have a non-interactive path suitable for CI.
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj \
  -c Release \
  -- --describe-only --profile ms-terminal-direct
