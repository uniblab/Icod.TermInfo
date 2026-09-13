from pathlib import Path


def replace_exact(path: str, old: str, new: str, count: int = 1) -> None:
    target = Path(path)
    text = target.read_text(encoding="utf-8")
    old_count = text.count(old)
    new_count = text.count(new)
    if old_count == 0 and new_count == count:
        return
    if old_count != count:
        raise SystemExit(f"{path}: expected {count} old occurrence(s), found {old_count}")
    target.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


replace_exact(
    "tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.11.0", ReadText( stdout ) );',
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
)
replace_exact(
    "tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.11.0", ReadText( stdout ) );',
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
)
replace_exact(
    "tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.11.0", ReadText( stdout ) );',
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
)
replace_exact(
    "tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs",
    '"1.11.0",\n\t\t\tReadRequiredProperty(',
    '"1.12.0",\n\t\t\tReadRequiredProperty(',
)
replace_exact(
    "tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs",
    'Assert.Contains( "1.11.0", ReadText( stdout ) );',
    'Assert.Contains( "1.12.0", ReadText( stdout ) );',
    count=2,
)
replace_exact(
    "tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs",
    'private const string DevelopmentVersion = "1.11.0";',
    'private const string DevelopmentVersion = "1.12.0";',
)

roadmap = "Icod.TermInfo-Post-1.0-Development-Roadmap.md"
replace_exact(
    roadmap,
    "**Current coordinated version:** `1.10.0-Alpha-6`",
    "**Current coordinated version:** `1.12.0-Alpha-1`",
)
replace_exact(
    roadmap,
    "**Next development line:** `1.10.0` - Deterministic Multi-Database Inspection, Comparison, and Planning Automation\n**Status:** 1.10.0 implementation in progress\n**Current tranche:** DA06 - Command and machine-readable automation composition\n**Primary objective:** Render effective descriptions, comparisons, plans, and explicit catalogs as deterministic bounded versioned JSON, then compose that reusable representation through `infocmp` and `toe` without changing frozen lower-layer semantics.",
    "**Final 1.10 prerelease:** `1.10.0-Alpha-8`\n**Final 1.11 prerelease:** `1.11.0-Alpha-8`\n**Next development line:** `1.12.0` - Advanced Persistent-Raster Placement Semantics and Planning\n**Status:** 1.12.0 implementation in progress\n**Current tranche:** PG01 - Architecture, vocabulary, and public API regret gate\n**Primary objective:** Add protocol-neutral source-rectangle and signed-z-order placement semantics beside the frozen 1.11 persistent-raster lifecycle model, preserving JSON v1-v3 and downstream execution ownership.",
)
replace_exact(
    roadmap,
    "| **1.10.0** | Deterministic multi-database inspection, comparison, and planning automation | Aggregate ordered explicit catalogs with stable evidence, then add precedence, conflict analysis, set comparison, multi-catalog planning, and versioned automation |\n| **later** | Exotic storage/formats | Berkeley DB provider and historical Unix dialects as justified |",
    "| **1.10.0** | Deterministic multi-database inspection, comparison, and planning automation | Aggregate ordered explicit catalogs with stable evidence, then add precedence, conflict analysis, set comparison, multi-catalog planning, and versioned automation |\n| **1.11.0** | Persistent-raster lifecycle semantics and planning | Classify protocol-neutral persistent-raster lifecycle evidence and produce deterministic advisory plans without owning terminal execution |\n| **1.12.0** | Advanced persistent-raster placement semantics and planning | Classify source-rectangle and signed-z-order placement support and compose those requirements with the frozen 1.11 lifecycle model |\n| **later** | Exotic storage/formats and broader graphics policy | Berkeley DB, historical Unix dialects, multi-protocol preference/negotiation, and other deferred work as justified |",
)
