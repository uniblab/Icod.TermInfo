from pathlib import Path

sh = Path('.github/scripts/verify-release-package.sh')
text = sh.read_text(encoding='utf-8')
marker = '''# DA08 freezes the exact complete 1.10 Inspection public surface independently
# on all three shipped target frameworks. Earlier baselines remain historical.
'''
replacement = '''# Frozen Inspection baselines remain immutable historical evidence:
# docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
# docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
# docs/1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt
# DA08 freezes the exact complete 1.10 Inspection public surface independently
# on all three shipped target frameworks. Earlier baselines remain historical.
'''
if marker not in text:
    raise SystemExit('POSIX DA08 marker not found.')
sh.write_text(text.replace(marker, replacement, 1), encoding='utf-8', newline='\n')

cmd = Path('.github/scripts/verify-release-package.cmd')
text = cmd.read_text(encoding='utf-8')
marker = 'rem DA08 freezes the exact complete 1.10 Inspection surface on every shipped framework.\n'
replacement = '''rem Frozen Inspection baselines remain immutable historical evidence:
rem docs\\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem docs\\1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem docs\\1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem DA08 freezes the exact complete 1.10 Inspection surface on every shipped framework.
'''
if marker not in text:
    raise SystemExit('Windows DA08 marker not found.')
cmd.write_text(text.replace(marker, replacement, 1), encoding='utf-8', newline='\n')
