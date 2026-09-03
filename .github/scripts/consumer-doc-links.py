from pathlib import Path


def replace_once(path, old, new):
    file = Path(path)
    text = file.read_text(encoding='utf-8')
    if old not in text:
        raise SystemExit(f'Required marker not found in {path}.')
    file.write_text(text.replace(old, new, 1), encoding='utf-8', newline='\n')


replace_once(
    'README.md',
    '''Stable `1.10.0` is promotion-only after the exact Alpha-8 release gate is green.\nSee `docs/1.10.0-RELEASE-AUDIT.md`.\n''',
    '''The exact Alpha-8 product gate is green. Before the stable version bump, the\nconsumer-facing closure adds executable documentation only: the consolidated\n`docs/1.10.0-MULTI-DATABASE-GUIDE.md`, the reusable-API\n`samples/Icod.TermInfo.DatabaseSet.Sample`, and the 1.10 extensions in\n`samples/ToolSuite`. The new sample's normalized v2 fixtures are now part of the\npermanent release verifier on `net8.0`, `net9.0`, and `net10.0`.\n\nStable `1.10.0` remains a promotion-only version/documentation transition after\nthe final post-documentation Staging gate is green. See\n`docs/1.10.0-RELEASE-AUDIT.md`.\n''',
)

replace_once(
    'README.md',
    '''The repository contains three executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.\n''',
    '''The repository contains four executable API samples plus one command-suite\nwalkthrough with deliberately different purposes.\n''',
)

marker = '### Managed tool-suite walkthrough\n'
database_sample = '''### Multi-database Inspection sample\n\n`samples/Icod.TermInfo.DatabaseSet.Sample` is the focused 1.10 reusable-API\nexample. It creates controlled conventional databases through the public\nCompiler API and exercises ordered `InspectSet(...)` construction, conclusive\nlookup precedence, semantic shadow and alias-collision analysis, set comparison,\nconflict-free multi-database parent planning, and all three version-2 JSON\ndocument kinds.\n\nRun it with:\n\n```text\ndotnet run --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj -f net10.0\n```\n\nThe permanent release verifier checks the sample's normalized JSON fixtures on\n`net8.0`, `net9.0`, and `net10.0`. See\n`samples/Icod.TermInfo.DatabaseSet.Sample/README.md` and\n`docs/1.10.0-MULTI-DATABASE-GUIDE.md`.\n\n'''
root = Path('README.md')
text = root.read_text(encoding='utf-8')
if database_sample not in text:
    if marker not in text:
        raise SystemExit('Root sample insertion marker not found.')
    root.write_text(text.replace(marker, database_sample + marker, 1), encoding='utf-8', newline='\n')

replace_once(
    'Icod.TermInfo.Inspection/README.md',
    '''See `docs/1.10.0-DA08-API-SCHEMA-COMMAND-PACKAGE-AND-DOCUMENTATION-FREEZE.md`\nand `docs/1.10.0-RELEASE-AUDIT.md`.\n''',
    '''For consumer-facing use of the frozen 1.10 surface, see\n`docs/1.10.0-MULTI-DATABASE-GUIDE.md` and\n`samples/Icod.TermInfo.DatabaseSet.Sample/README.md`. Release-contract evidence\nremains in\n`docs/1.10.0-DA08-API-SCHEMA-COMMAND-PACKAGE-AND-DOCUMENTATION-FREEZE.md` and\n`docs/1.10.0-RELEASE-AUDIT.md`.\n''',
)
