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
