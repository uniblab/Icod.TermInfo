from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.
# Verification checkpoint: the repository ternary inventory should now be empty.


def replace_exact(path: str, old: str, new: str) -> None:
    target = Path(path)
    text = target.read_text(encoding="utf-8")
    old_count = text.count(old)
    new_count = text.count(new)
    embedded_old_count = new.count(old)
    if new_count == 1 and old_count == embedded_old_count:
        return
    if new_count == 0 and old_count == 1:
        target.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
        return
    raise SystemExit(
        f"{path}: expected one old target or one normalized target; "
        f"found old={old_count}, new={new_count}"
    )


replace_exact(
    "toe/src/Command.cs",
    """\t\t\t\t\tvar namesInCurrentRoot = duplicateReferences is null\n\t\t\t\t\t\t? null\n\t\t\t\t\t\t: new HashSet<string>( StringComparer.Ordinal );\n""",
    """\t\t\t\t\tvar namesInCurrentRoot = ( duplicateReferences is null )\n\t\t\t\t\t\t? null\n\t\t\t\t\t\t: new HashSet<string>( StringComparer.Ordinal )\n\t\t\t\t\t;\n""",
)
