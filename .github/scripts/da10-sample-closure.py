from pathlib import Path

sh = Path('.github/scripts/verify-release-package.sh')
text = sh.read_text(encoding='utf-8')
marker = '# The repository sample must retain a non-interactive path suitable for CI.\n'
section = '''# The 1.10 database-set sample is executable documentation for the frozen public\n# API and all three version-2 database automation document kinds.\nfor database_set_sample_framework in net8.0 net9.0 net10.0; do\n  dotnet run \\\n    --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj \\\n    -c "${configuration}" \\\n    -f "${database_set_sample_framework}" \\\n    --no-build \\\n    -- --verify-fixtures samples/Icod.TermInfo.DatabaseSet.Sample/expected \\\n    > /dev/null\ndone\n\n'''
if marker not in text:
    raise SystemExit('POSIX sample verification marker was not found.')
if 'Icod.TermInfo.DatabaseSet.Sample' not in text:
    text = text.replace(marker, section + marker, 1)
sh.write_text(text, encoding='utf-8', newline='\n')

cmd = Path('.github/scripts/verify-release-package.cmd')
text = cmd.read_text(encoding='utf-8')
marker = 'echo.\necho === Non-interactive repository sample ===\n'
section = '''echo.\necho === Icod.TermInfo 1.10 database-set sample fixtures ===\nfor %%F in (net8.0 net9.0 net10.0) do (\n    dotnet run --project samples\\Icod.TermInfo.DatabaseSet.Sample\\Icod.TermInfo.DatabaseSet.Sample.csproj -c %CONFIGURATION% -f %%F --no-build -- --verify-fixtures samples\\Icod.TermInfo.DatabaseSet.Sample\\expected >nul\n    if errorlevel 1 goto fail\n)\n\n'''
if marker not in text:
    raise SystemExit('Windows sample verification marker was not found.')
if 'Icod.TermInfo.DatabaseSet.Sample' not in text:
    text = text.replace(marker, section + marker, 1)
cmd.write_text(text, encoding='utf-8', newline='\n')
