from pathlib import Path

# Temporary branch-scoped driver; removed after the convention gate is green.


def replace_exact(path: str, old: str, new: str) -> None:
    target = Path(path)
    text = target.read_text(encoding="utf-8")
    old_count = text.count(old)
    new_count = text.count(new)
    if old_count == 1 and new_count == 0:
        target.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
        return
    if old_count == 0 and new_count == 1:
        return
    raise SystemExit(
        f"{path}: expected one old target or one normalized target; "
        f"found old={old_count}, new={new_count}"
    )


convention_test = "tests/Icod.TermInfo.Tests/src/CodingConventionTests.cs"

replace_exact(
    convention_test,
    """\t\t\tint semicolonIndex = maskedSource.IndexOf( ';', colonIndex + 1 );\n\t\t\tif ( semicolonIndex < 0 ) {\n\t\t\t\treturn colonIndex;\n\t\t\t}\n\n\t\t\tint semicolonLineStart =\n\t\t\t\tmaskedSource.LastIndexOf( '\\n', semicolonIndex - 1 ) + 1;\n\t\t\tfor ( int i = semicolonLineStart; i < semicolonIndex; i++ ) {\n\t\t\t\tif ( !char.IsWhiteSpace( source[i] ) ) {\n\t\t\t\t\treturn semicolonIndex;\n\t\t\t\t}\n\t\t\t}\n""",
    """\t\t\tif ( IsNestedInParenthesesOrBrackets( maskedSource, questionIndex ) ) {\n\t\t\t\tcontinue;\n\t\t\t}\n\n\t\t\tint semicolonIndex = maskedSource.IndexOf( ';', colonIndex + 1 );\n\t\t\tif ( semicolonIndex < 0 ) {\n\t\t\t\treturn colonIndex;\n\t\t\t}\n\n\t\t\tint semicolonLineStart =\n\t\t\t\tmaskedSource.LastIndexOf( '\\n', semicolonIndex - 1 ) + 1;\n\t\t\tfor ( int i = semicolonLineStart; i < semicolonIndex; i++ ) {\n\t\t\t\tif ( !char.IsWhiteSpace( source[i] ) ) {\n\t\t\t\t\treturn semicolonIndex;\n\t\t\t\t}\n\t\t\t}\n""",
)

replace_exact(
    convention_test,
    """\tprivate static bool IsCallLikeOpenParenthesis(\n""",
    """\tprivate static bool IsNestedInParenthesesOrBrackets(\n\t\tstring source,\n\t\tint index\n\t) {\n\t\tArgumentNullException.ThrowIfNull( source );\n\t\tArgumentOutOfRangeException.ThrowIfNegative( index );\n\n\t\tint parenthesisDepth = 0;\n\t\tint bracketDepth = 0;\n\t\tfor ( int i = 0; i < index; i++ ) {\n\t\t\tif ( source[i] == '(' ) {\n\t\t\t\tparenthesisDepth++;\n\t\t\t} else if ( source[i] == ')' ) {\n\t\t\t\tparenthesisDepth--;\n\t\t\t} else if ( source[i] == '[' ) {\n\t\t\t\tbracketDepth++;\n\t\t\t} else if ( source[i] == ']' ) {\n\t\t\t\tbracketDepth--;\n\t\t\t}\n\t\t}\n\n\t\treturn parenthesisDepth > 0 || bracketDepth > 0;\n\t}\n\n\tprivate static bool IsCallLikeOpenParenthesis(\n""",
)
