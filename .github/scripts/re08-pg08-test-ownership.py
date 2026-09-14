from pathlib import Path

path = Path("tests/Icod.TermInfo.Inspection.Tests/src/PG08ReleaseClosureTests.cs")
text = path.read_text(encoding="utf-8")
old_variable = '''\t\tstring inspectionProject = ReadRepositoryFile(\n\t\t\t"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"\n\t\t);\n'''
old_assertion = '''\t\tAssert.Contains(\n\t\t\t"<PackageReleaseNotes>1.12.0",\n\t\t\tinspectionProject,\n\t\t\tStringComparison.Ordinal\n\t\t);\n'''
new_assertion = '''\t\tAssert.Contains(\n\t\t\t"The coordinated version is `1.12.0`",\n\t\t\taudit,\n\t\t\tStringComparison.Ordinal\n\t\t);\n'''
if old_variable not in text:
    raise SystemExit("PG08 current-package metadata variable anchor not found")
if old_assertion not in text:
    raise SystemExit("PG08 stale current-package release-notes assertion not found")
text = text.replace(old_variable, "", 1)
text = text.replace(old_assertion, new_assertion, 1)
path.write_text(text, encoding="utf-8", newline="\n")
