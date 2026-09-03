from pathlib import Path

path = Path("infocmp/src/InfoCmpOptions.cs")
text = path.read_text(encoding="utf-8")
old = '''\t\t\t\t\tcase "--candidate-root":\n\t\t\t\t\tif ( !TryReadValue(\n\t\t\t\t\t\targs,\n\t\t\t\t\t\tref index,\n\t\t\t\t\t\targument,\n\t\t\t\t\t\tout string? candidateRoot,\n\t\t\t\t\t\tout string? candidateRootError\n\t\t\t\t\t) ) {\n\t\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure( candidateRootError! );\n\t\t\t\t\t}\n\t\t\t\t\tcandidateRoots.Add( candidateRoot! );\n\t\t\t\t\tbreak;\n\n\t\t\t\tcase "--max-parents":'''
new = '''\t\t\t\t\tcase "--candidate-root":\n\t\t\t\t\t\tif ( !TryReadValue(\n\t\t\t\t\t\t\targs,\n\t\t\t\t\t\t\tref index,\n\t\t\t\t\t\t\targument,\n\t\t\t\t\t\t\tout string? candidateRoot,\n\t\t\t\t\t\t\tout string? candidateRootError\n\t\t\t\t\t\t) ) {\n\t\t\t\t\t\t\treturn InfoCmpOptionsParseResult.Failure(\n\t\t\t\t\t\t\t\tcandidateRootError!\n\t\t\t\t\t\t\t);\n\t\t\t\t\t\t}\n\t\t\t\t\t\tcandidateRoots.Add( candidateRoot! );\n\t\t\t\t\t\tbreak;\n\n\t\t\t\t\tcase "--max-parents":'''
if text.count(old) != 1:
    raise RuntimeError(f"expected one DA06 indentation block, found {text.count(old)}")
path.write_text(text.replace(old, new, 1), encoding="utf-8", newline="\n")
