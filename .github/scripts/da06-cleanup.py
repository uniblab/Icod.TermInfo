from pathlib import Path

path = Path("infocmp/src/InfoCmpOptions.cs")
text = path.read_text(encoding="utf-8")

replacements = [
    (
        "\t\tvar terminalNames = new List<string>();\n\t\tvar candidateRoots = new List<string>();",
        "\t\tList<string> terminalNames = [];\n\t\tList<string> candidateRoots = [];",
    ),
    (
        '''\t\tif ( planning ) {\n\t\tif ( candidateRoots.Count != 0 && ( !planning || !allCandidates ) ) {\n\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t"option '--candidate-root' requires '--plan-use --all-candidates'"\n\t\t\t);\n\t\t}\n\t\tif ( allCandidates ) {\n\t\t\tif ( !planning ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires '--plan-use'"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( comparisonDatabaseDirectory is not null && candidateRoots.Count != 0 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"options '-B' and '--candidate-root' are mutually exclusive for all-candidates planning"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( comparisonDatabaseDirectory is null && candidateRoots.Count == 0 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires one explicit '-B' directory or at least one '--candidate-root' directory"\n\t\t\t\t);\n\t\t\t}\n\t\t\tif ( terminalNames.Count != 1 ) {\n\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t"option '--all-candidates' requires exactly one target terminal"\n\t\t\t\t);\n\t\t\t}\n\t\t} else if ( terminalNames.Count < 2 ) {''',
        '''\t\tif ( planning ) {\n\t\t\tif ( allCandidates ) {\n\t\t\t\tif ( comparisonDatabaseDirectory is not null\n\t\t\t\t\t&& candidateRoots.Count != 0 ) {\n\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t\t"options '-B' and '--candidate-root' are mutually exclusive for all-candidates planning"\n\t\t\t\t\t);\n\t\t\t\t}\n\t\t\t\tif ( comparisonDatabaseDirectory is null\n\t\t\t\t\t&& candidateRoots.Count == 0 ) {\n\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t\t"option '--all-candidates' requires one explicit '-B' directory or at least one '--candidate-root' directory"\n\t\t\t\t\t);\n\t\t\t\t}\n\t\t\t\tif ( terminalNames.Count != 1 ) {\n\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t\t"option '--all-candidates' requires exactly one target terminal"\n\t\t\t\t\t);\n\t\t\t\t}\n\t\t\t} else if ( terminalNames.Count < 2 ) {''',
    ),
]

for old, new in replacements:
    if text.count(old) != 1:
        raise RuntimeError(f"expected one cleanup match, found {text.count(old)}")
    text = text.replace(old, new, 1)

path.write_text(text, encoding="utf-8", newline="\n")
