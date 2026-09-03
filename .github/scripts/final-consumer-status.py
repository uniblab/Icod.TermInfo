from pathlib import Path


def replace_once(path, old, new):
    file = Path(path)
    text = file.read_text(encoding='utf-8')
    if old not in text:
        raise SystemExit(f'Required marker not found in {path}.')
    file.write_text(text.replace(old, new, 1), encoding='utf-8', newline='\n')


replace_once(
    'README.md',
    '''Stable `1.10.0` remains a promotion-only version/documentation transition after\nthe final post-documentation Staging gate is green. See\n`docs/1.10.0-RELEASE-AUDIT.md`.\n''',
    '''The final post-documentation Staging gate is green (`33736812176`, head\n`b312c946e2e003f2d00761dff3d49957dbfbbeaf`). Stable `1.10.0` now remains a\npromotion-only version/documentation/package-metadata transition. See\n`docs/1.10.0-RELEASE-AUDIT.md`.\n''',
)

replace_once(
    'docs/1.10.0-RELEASE-AUDIT.md',
    '**Status:** DA08 complete and frozen; exact Alpha-8 release gate green; stable promotion pending final consumer-documentation validation',
    '**Status:** DA08 and consumer documentation/sample closure complete and green; stable promotion pending',
)
replace_once(
    'docs/1.10.0-RELEASE-AUDIT.md',
    '''The consumer documentation/sample closure is additive release-facing evidence above that frozen product contract. A final clean-head Staging run after the documentation closure remains required before the stable version bump.\n''',
    '''The consumer documentation/sample closure is additive release-facing evidence above that frozen product contract. Final clean-head PR Staging run `33736812176` at head `b312c946e2e003f2d00761dff3d49957dbfbbeaf` completed successfully across Windows, Linux, and macOS, including the permanent three-TFM database-set sample fixture gate, installed tool package automation, and all six archive RIDs.\n''',
)
replace_once(
    'docs/1.10.0-RELEASE-AUDIT.md',
    '''Before stable promotion, the final post-documentation head SHALL additionally pass the permanent three-TFM `Icod.TermInfo.DatabaseSet.Sample` fixture gate introduced by the consumer closure.\n\nStable promotion is version/documentation-only.''',
    '''The final post-documentation head passed the permanent three-TFM `Icod.TermInfo.DatabaseSet.Sample` fixture gate introduced by the consumer closure.\n\nStable promotion is version/documentation-only.''',
)

roadmap = Path('Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md')
text = roadmap.read_text(encoding='utf-8')
old = '**Status:** DA08 release contract frozen at `1.10.0-Alpha-8`; stable promotion pending exact-head validation'
new = '**Status:** DA08 and consumer documentation/sample closure complete and validated at `1.10.0-Alpha-8`; stable promotion pending'
if old not in text:
    raise SystemExit('Roadmap status marker not found.')
roadmap.write_text(text.replace(old, new, 1), encoding='utf-8', newline='\n')
