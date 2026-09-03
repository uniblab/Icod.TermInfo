from pathlib import Path

for path, marker, replacement in (
    (
        '.github/scripts/verify-release-package.sh',
        '# Frozen Inspection baselines remain immutable historical evidence:\n',
        '# MI07 and earlier frozen Inspection baselines remain immutable historical evidence:\n',
    ),
    (
        '.github/scripts/verify-release-package.cmd',
        'rem Frozen Inspection baselines remain immutable historical evidence:\n',
        'rem MI07 and earlier frozen Inspection baselines remain immutable historical evidence:\n',
    ),
):
    file = Path(path)
    text = file.read_text(encoding='utf-8')
    if marker not in text:
        raise SystemExit(f'Marker not found in {path}.')
    file.write_text(text.replace(marker, replacement, 1), encoding='utf-8', newline='\n')
