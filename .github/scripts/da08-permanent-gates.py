from pathlib import Path

sh = Path('.github/scripts/verify-release-package.sh')
text = sh.read_text(encoding='utf-8')
old = '''# MI07 froze the exact 1.9 Inspection surface. The frozen 1.7, 1.8, and 1.9
# Inspection baselines remain immutable historical evidence during additive 1.10
# development. Cross-framework equality above remains active throughout
# DA01-DA07; DA08 freezes the complete 1.10 surface.
# docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
# docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
# docs/1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt
'''
new = '''# DA08 freezes the exact complete 1.10 Inspection public surface independently
# on all three shipped target frameworks. Earlier baselines remain historical.
for inspection_framework in net8.0 net9.0 net10.0; do
  dotnet run \\
    --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj \\
    -c "${configuration}" \\
    --no-build \\
    -- --check \\
    docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt \\
    Icod.TermInfo.Inspection/bin/${configuration}/${inspection_framework}/Icod.TermInfo.Inspection.dll
done
'''
if old not in text:
    raise SystemExit('POSIX Inspection freeze marker was not found.')
sh.write_text(text.replace(old, new, 1), encoding='utf-8', newline='\n')

cmd = Path('.github/scripts/verify-release-package.cmd')
text = cmd.read_text(encoding='utf-8')
marker = '''rem The frozen 1.7 and 1.8 Inspection baselines remain immutable historical
rem evidence. MI07 freezes the complete additive 1.9 JSON surface independently.
rem docs\\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem docs\\1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
echo.
rem MI07 froze the exact 1.9 Inspection surface. The frozen 1.7, 1.8, and 1.9 baselines remain immutable historical evidence.
rem docs\\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem docs\\1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem docs\\1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt
rem DA01 begins additive 1.10 development. Cross-framework equality remains active through DA07; DA08 freezes 1.10.
'''
replacement = '''rem DA08 freezes the exact complete 1.10 Inspection surface on every shipped framework.
echo.
echo === Verify approved Icod.TermInfo.Inspection 1.10 public API baseline (%CONFIGURATION%) ===
for %%F in (net8.0 net9.0 net10.0) do (
    dotnet run --project tools\\public-api-snapshot\\Icod.TermInfo.PublicApiSnapshot.csproj -c %CONFIGURATION% --no-build -- --check docs\\1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt Icod.TermInfo.Inspection\\bin\\%CONFIGURATION%\\%%F\\Icod.TermInfo.Inspection.dll
    if errorlevel 1 goto fail
)
'''
if marker not in text:
    raise SystemExit('Windows Inspection freeze marker was not found.')
cmd.write_text(text.replace(marker, replacement, 1), encoding='utf-8', newline='\n')

verifier = Path('tools/inspection-package-verifier/Program.cs')
text = verifier.read_text(encoding='utf-8')
old_const = '''\tprivate const string ExpectedJsonSchemaSha256 =
\t\t"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
'''
new_const = '''\tprivate const string ExpectedJsonSchemaV1Sha256 =
\t\t"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
\tprivate const string ExpectedJsonSchemaV2Sha256 =
\t\t"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";
'''
if old_const not in text:
    raise SystemExit('Inspection schema constant marker was not found.')
text = text.replace(old_const, new_const, 1)
text = text.replace(
    'schemaSha256 == ExpectedJsonSchemaSha256,',
    'schemaSha256 == ExpectedJsonSchemaV1Sha256,',
    1,
)
insert_marker = '\n\tprivate static void VerifyAssemblyIdentity('
v2 = r'''

		ZipArchiveEntry schemaV2Entry =
			package.GetEntry(
				"docs/Icod.TermInfo.Inspection.schema.v2.json"
			) ?? throw new InvalidOperationException(
				"Inspection package does not contain the database automation JSON Schema."
			);
		string schemaV2;
		using ( Stream stream = schemaV2Entry.Open() )
		using ( StreamReader reader = new( stream, Encoding.UTF8 ) ) {
			schemaV2 =
				reader
					.ReadToEnd()
					.Replace( "\r\n", "\n", StringComparison.Ordinal )
					.Replace( '\r', '\n' );
		}
		string schemaV2Sha256 =
			Convert.ToHexString(
				SHA256.HashData(
					Encoding.UTF8.GetBytes( schemaV2 )
				)
			).ToLowerInvariant();
		Require(
			schemaV2Sha256 == ExpectedJsonSchemaV2Sha256,
			$"Inspection package database automation JSON Schema fingerprint '{schemaV2Sha256}' does not match the frozen version-2 fingerprint."
		);
		using JsonDocument documentV2 = JsonDocument.Parse( schemaV2 );
		JsonElement rootV2 = documentV2.RootElement;
		Require(
			rootV2.GetProperty( "$schema" ).GetString()
				== "https://json-schema.org/draft/2020-12/schema",
			"Inspection package database automation JSON Schema does not identify draft 2020-12."
		);
		Require(
			rootV2.GetProperty( "$id" ).GetString()
				== "urn:icod:terminfo:inspection:json:2",
			"Inspection package database automation JSON Schema does not identify schema version 2."
		);
		Require(
			rootV2.GetProperty( "oneOf" ).GetArrayLength() == 3,
			"Inspection package database automation JSON Schema does not define all three document kinds."
		);
		string[] documentReferencesV2 =
			rootV2
				.GetProperty( "oneOf" )
				.EnumerateArray()
				.Select(
					branch => branch.GetProperty( "$ref" ).GetString()
				)
				.Cast<string>()
				.ToArray();
		Require(
			documentReferencesV2.SequenceEqual(
				new[] {
					"#/$defs/databaseSetDocument",
					"#/$defs/databaseSetComparisonDocument",
					"#/$defs/databaseSetPlanDocument",
				},
				StringComparer.Ordinal
			),
			"Inspection package database automation JSON Schema does not define the frozen three document kinds."
		);
'''
closing = '\n\t}\n' + insert_marker
if closing not in text:
    raise SystemExit('VerifyJsonSchema closing marker was not found.')
text = text.replace(closing, v2 + '\n\t}\n' + insert_marker, 1)
text = text.replace(
    'multi-target package structure, JSON Schema, exact Runtime/Source dependency boundary',
    'multi-target package structure, JSON Schemas, exact Runtime/Source dependency boundary',
    1,
)
verifier.write_text(text, encoding='utf-8', newline='\n')
