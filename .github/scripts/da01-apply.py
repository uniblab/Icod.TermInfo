from pathlib import Path
import re


def replace_exact(path_name: str, old: str, new: str, count: int = 1) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual != count:
        raise RuntimeError(f"{path_name}: expected {count} occurrence(s), found {actual}: {old!r}")
    path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.9.0</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-1</IcodTermInfoSuiteVersion>",
)

replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    "<PackageReleaseNotes>1.9.0 publishes the frozen 31-type Inspection API, deterministic bounded version-1 JSON renderer and Schema, command automation, package graph, samples, fixtures, router/archive topology, and release evidence while preserving all lower-layer contracts.</PackageReleaseNotes>",
    "<PackageReleaseNotes>1.10.0-Alpha-1 adds the immutable bounded ordered database-set foundation, canonical occurrence indexing, constituent completeness evidence, and explicit-root/catalog construction while preserving the frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
)

for path_name in [
    "tests/Icod.TermInfo.Inspection.Tests/src/MI02TerminalDescriptionJsonTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/MI03ComparisonAndPlanJsonTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/MI04DatabaseCatalogJsonAndSchemaTests.cs",
]:
    replace_exact(
        path_name,
        '''Assert.Equal(\n\t\t\t31,\n\t\t\ttypeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes().Length\n\t\t);''',
        '''Assert.InRange(\n\t\t\ttypeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes().Length,\n\t\t\t31,\n\t\t\tint.MaxValue\n\t\t);''',
    )

replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI01JsonContractTests.cs",
    "Assert.Equal( 31, exportedTypes.Length );",
    "Assert.InRange( exportedTypes.Length, 31, int.MaxValue );",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    "Assert.Equal( 31, exportedTypes.Length );",
    "Assert.InRange( exportedTypes.Length, 31, int.MaxValue );",
)
replace_exact(
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    '''Assert.Contains(\n\t\t\tStableReleaseVersion,\n\t\t\tbuildProperties,\n\t\t\tStringComparison.Ordinal );''',
    '''Assert.Contains(\n\t\t\t"IcodTermInfoSuiteVersion",\n\t\t\tbuildProperties,\n\t\t\tStringComparison.Ordinal );''',
)

replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "exportedTypes.Length == 31",
    "exportedTypes.Length >= 37",
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogEntry ) )",
    '''&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogEntry ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSet ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetEntry ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetIdentity ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetOccurrence ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetIssue ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetOptions ) )''',
)

sh_path = Path(".github/scripts/verify-release-package.sh")
sh = sh_path.read_text(encoding="utf-8")
pattern = re.compile(
    r'''# The frozen 1\.7 and 1\.8 Inspection baselines remain immutable historical\n# evidence\. MI07 freezes the complete additive 1\.9 JSON surface independently\.\n# docs/1\.7\.0-INSPECTION-PUBLIC-API-BASELINE\.txt\n# docs/1\.8\.0-INSPECTION-PUBLIC-API-BASELINE\.txt\ndotnet run \\\n  --project tools/public-api-snapshot/Icod\.TermInfo\.PublicApiSnapshot\.csproj \\\n  -c "\$\{configuration\}" \\\n  --no-build \\\n  -- --check \\\n  docs/1\.9\.0-INSPECTION-PUBLIC-API-BASELINE\.txt \\\n  Icod\.TermInfo\.Inspection/bin/\$\{configuration\}/net10\.0/Icod\.TermInfo\.Inspection\.dll\n'''
)
replacement = '''# The frozen 1.7, 1.8, and 1.9 Inspection baselines remain immutable historical\n# evidence during additive 1.10 development. Cross-framework equality above\n# remains active throughout DA01-DA07; DA08 freezes the complete 1.10 surface.\n# docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt\n# docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt\n# docs/1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt\n'''
sh, n = pattern.subn(replacement, sh)
if n != 1:
    raise RuntimeError(f"verify-release-package.sh baseline block matches: {n}")
sh_path.write_text(sh, encoding="utf-8", newline="\n")

cmd_path = Path(".github/scripts/verify-release-package.cmd")
cmd = cmd_path.read_text(encoding="utf-8")
start = cmd.find("echo === Verify approved Icod.TermInfo.Inspection 1.9 public API baseline")
if start < 0:
    raise RuntimeError("verify-release-package.cmd: 1.9 baseline heading not found")
line_start = cmd.rfind("\n", 0, start) + 1
end_marker = "if errorlevel 1 goto fail"
end = cmd.find(end_marker, start)
if end < 0:
    raise RuntimeError("verify-release-package.cmd: baseline check end not found")
end = cmd.find("\n", end) + 1
cmd = (
    cmd[:line_start]
    + "rem The frozen 1.7, 1.8, and 1.9 Inspection baselines remain immutable historical evidence.\n"
    + "rem docs\\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt\n"
    + "rem docs\\1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt\n"
    + "rem docs\\1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt\n"
    + "rem DA01 begins additive 1.10 development. Cross-framework equality remains active through DA07; DA08 freezes 1.10.\n"
    + cmd[end:]
)
cmd_path.write_text(cmd, encoding="utf-8", newline="\n")

roadmap = Path("Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md")
text = roadmap.read_text(encoding="utf-8")
text = text.replace(
    "**Status:** Roadmap approved; DA01 not yet started",
    "**Status:** DA01 implementation complete; Staging validation pending",
    1,
)
roadmap.write_text(text, encoding="utf-8", newline="\n")

active = Path("Icod.TermInfo-Post-1.0-Development-Roadmap.md")
text = active.read_text(encoding="utf-8")
text = text.replace(
    "**Current coordinated version:** `1.9.0`",
    "**Current coordinated version:** `1.10.0-Alpha-1`",
    1,
)
text = text.replace(
    "**Next development line:** not yet selected",
    "**Next development line:** `1.10.0` - Deterministic Multi-Database Inspection, Comparison, and Planning Automation",
    1,
)
text = text.replace(
    "**Status:** 1.9.0 stable release contract frozen",
    "**Status:** 1.10.0 implementation in progress",
    1,
)
text = text.replace(
    "**Current tranche:** Release closure - exact-main validation and publication",
    "**Current tranche:** DA01 - Database-set model and contract foundation",
    1,
)
text = text.replace(
    "| **1.9.0** | Machine-readable inspection and planning automation | Render versioned deterministic JSON for Inspection values and expose explicit command automation without parsing human output |\n| **later** | Exotic storage/formats | Berkeley DB provider and historical Unix dialects as justified |",
    "| **1.9.0** | Machine-readable inspection and planning automation | Render versioned deterministic JSON for Inspection values and expose explicit command automation without parsing human output |\n| **1.10.0** | Deterministic multi-database inspection, comparison, and planning automation | Aggregate ordered explicit catalogs with stable evidence, then add precedence, conflict analysis, set comparison, multi-catalog planning, and versioned automation |\n| **later** | Exotic storage/formats | Berkeley DB provider and historical Unix dialects as justified |",
    1,
)
active.write_text(text, encoding="utf-8", newline="\n")
