from pathlib import Path

path = Path("tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs")
text = path.read_text(encoding="utf-8")
old = '''\t\tAssert.Contains(
\t\t\t"PG01 - Architecture, vocabulary, and public API regret gate",
\t\t\tactiveRoadmap,
\t\t\tStringComparison.OrdinalIgnoreCase
\t\t);
\t\tAssert.Contains(
\t\t\t"1.12.0",
\t\t\tactiveRoadmap,
\t\t\tStringComparison.Ordinal
\t\t);
'''
new = '''\t\tAssert.Contains(
\t\t\t"**Current coordinated version:**",
\t\t\tactiveRoadmap,
\t\t\tStringComparison.Ordinal
\t\t);
\t\tAssert.Contains(
\t\t\t"**Status:**",
\t\t\tactiveRoadmap,
\t\t\tStringComparison.Ordinal
\t\t);
\t\tAssert.Contains(
\t\t\t"**Release audit:**",
\t\t\tactiveRoadmap,
\t\t\tStringComparison.Ordinal
\t\t);
'''
count = text.count(old)
if count != 1:
    raise RuntimeError(f"Expected exactly one stale MI07 current-development assertion block, found {count}")
path.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
