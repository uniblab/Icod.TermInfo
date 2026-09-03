using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Icod.TermInfo.Inspection.PackageVerifier;

internal static class Program {
	private const string PackageId = "Icod.TermInfo.Inspection";
	private const string RuntimePackageId = "Icod.TermInfo";
	private const string SourcePackageId = "Icod.TermInfo.Source";
	private const string CompilerPackageId = "Icod.TermInfo.Compiler";
	private const string RepositoryUrl = "https://github.com/uniblab/Icod.TermInfo";
	private const string ExpectedAssemblyVersion = "1.0.0.0";
	private const string ExpectedJsonSchemaV1Sha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string ExpectedJsonSchemaV2Sha256 =
		"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";
	private static readonly string[] TargetFrameworks = [
		"net8.0",
		"net9.0",
		"net10.0",
	];

	public static int Main(
		string[] args
	) {
		ArgumentNullException.ThrowIfNull( args );

		if ( args.Length > 1 ) {
			Console.Error.WriteLine(
				"Usage: dotnet run --project tools/inspection-package-verifier/Icod.TermInfo.Inspection.PackageVerifier.csproj -- [artifact-directory]"
			);
			return 2;
		}

		try {
			string root =
				FindRepositoryRoot();
			string artifactDirectory =
				args.Length == 0
					? Path.Combine(
						root,
						"artifacts"
					)
					: Path.GetFullPath(
						args[ 0 ],
						root
					);
			string packageVersion =
				ReadPackageVersion(
					root,
					Path.Combine(
						"Icod.TermInfo.Inspection",
						"Icod.TermInfo.Inspection.csproj"
					)
				);
			string runtimeVersion =
				ReadPackageVersion(
					root,
					"Icod.TermInfo.csproj"
				);
			string sourceVersion =
				ReadPackageVersion(
					root,
					Path.Combine(
						"Icod.TermInfo.Source",
						"Icod.TermInfo.Source.csproj"
					)
				);
			string compilerVersion =
				ReadPackageVersion(
					root,
					Path.Combine(
						"Icod.TermInfo.Compiler",
						"Icod.TermInfo.Compiler.csproj"
					)
				);
			Require(
				packageVersion == runtimeVersion
					&& packageVersion == sourceVersion
					&& packageVersion == compilerVersion,
				"Runtime, Source, Compiler, and Inspection PackageVersion values must match."
			);

			string nupkg =
				Path.Combine(
					artifactDirectory,
					$"{PackageId}.{packageVersion}.nupkg"
				);
			string snupkg =
				Path.Combine(
					artifactDirectory,
					$"{PackageId}.{packageVersion}.snupkg"
				);
			Require(
				File.Exists( nupkg ),
				$"Package not found: {nupkg}"
			);
			Require(
				File.Exists( snupkg ),
				$"Symbol package not found: {snupkg}"
			);

			string commit =
				VerifyPackage(
					nupkg,
					packageVersion
				);
			VerifySymbols(
				snupkg,
				commit
			);

			Console.WriteLine(
				$"Verified {PackageId} multi-target package structure, JSON Schemas, exact Runtime/Source dependency boundary, assembly identity, symbols, and Source Link for {packageVersion}."
			);
			return 0;
		}
		catch ( Exception exception ) when (
			exception is IOException
			or UnauthorizedAccessException
			or InvalidDataException
			or JsonException
			or InvalidOperationException
		) {
			Console.Error.WriteLine(
				exception.Message
			);
			return 1;
		}
	}

	private static string VerifyPackage(
		string packagePath,
		string expectedVersion
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( packagePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );

		using ZipArchive package =
			ZipFile.OpenRead(
				packagePath
			);
		HashSet<string> names =
			package.Entries
				.Select(
					entry => entry.FullName
				)
				.ToHashSet(
					StringComparer.Ordinal
				);

		List<string> required = [
			"README.md",
			"docs/Icod.TermInfo.Inspection.schema.json",
			"docs/Icod.TermInfo.Inspection.schema.v2.json",
			"icon.png",
		];
		foreach ( string targetFramework in TargetFrameworks ) {
			required.Add(
				$"lib/{targetFramework}/{PackageId}.dll"
			);
			required.Add(
				$"lib/{targetFramework}/{PackageId}.xml"
			);
		}
		string[] missing =
			required
				.Where(
					name => !names.Contains( name )
				)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();
		Require(
			missing.Length == 0,
			"Inspection package is missing required entries: "
				+ string.Join(
					", ",
					missing
				)
		);

		string[] dlls =
			names
				.Where(
					name =>
						name.EndsWith(
							".dll",
							StringComparison.OrdinalIgnoreCase
						)
				)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();
		string[] expectedDlls =
			TargetFrameworks
				.Select(
					targetFramework =>
						$"lib/{targetFramework}/{PackageId}.dll"
				)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();
		Require(
			dlls.SequenceEqual(
				expectedDlls,
				StringComparer.Ordinal
			),
			"Inspection package contains unexpected DLL payloads: "
				+ string.Join(
					", ",
					dlls
				)
		);
		Require(
			!names.Any(
				name =>
					name.StartsWith(
						"runtimes/",
						StringComparison.Ordinal
					)
			),
			"Inspection package unexpectedly contains a runtimes/ payload."
		);
		VerifyJsonSchema( package );

		foreach ( string targetFramework in TargetFrameworks ) {
			VerifyAssemblyIdentity(
				package,
				targetFramework
			);
			VerifyDocumentation(
				package,
				targetFramework
			);
		}

		ZipArchiveEntry nuspecEntry =
			AssertSingleNuspec(
				package
			);
		using Stream nuspecStream =
			nuspecEntry.Open();
		XDocument nuspec =
			XDocument.Load(
				nuspecStream,
				LoadOptions.None
			);
		XElement metadata =
			nuspec
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName == "metadata"
				);
		Require(
			GetMetadataText(
				metadata,
				"id"
			) == PackageId,
			"Unexpected package id."
		);
		Require(
			GetMetadataText(
				metadata,
				"version"
			) == expectedVersion,
			"Unexpected package version."
		);
		Require(
			GetMetadataText(
				metadata,
				"title"
			) == PackageId,
			"Unexpected package title."
		);
		Require(
			GetMetadataText(
				metadata,
				"authors"
			) == "Timothy J. Bruce",
			"Unexpected package authors."
		);
		Require(
			GetMetadataText(
				metadata,
				"projectUrl"
			) == RepositoryUrl,
			"Unexpected package project URL."
		);
		Require(
			GetMetadataText(
				metadata,
				"readme"
			) == "README.md",
			"Package metadata does not identify README.md."
		);
		Require(
			GetMetadataText(
				metadata,
				"icon"
			) == "icon.png",
			"Package metadata does not identify icon.png."
		);

		XElement? license =
			metadata
				.Elements()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "license"
				);
		Require(
			license is not null,
			"Package metadata has no license element."
		);
		Require(
			license!.Attribute( "type" )?.Value == "expression",
			"Package license is not an expression."
		);
		Require(
			license.Value == "LGPL-3.0-or-later",
			"Unexpected package license expression."
		);

		XElement[] groups =
			metadata
				.Descendants()
				.Where(
					element =>
						element.Name.LocalName == "group"
				)
				.ToArray();
		Require(
			groups.Length == TargetFrameworks.Length,
			"Inspection dependency groups do not match the supported target frameworks."
		);
		foreach ( string targetFramework in TargetFrameworks ) {
			XElement? group =
				groups.SingleOrDefault(
					element =>
						element.Attribute( "targetFramework" )?.Value
							== targetFramework
				);
			Require(
				group is not null,
				$"Missing dependency group for {targetFramework}."
			);
			XElement[] dependencies =
				group!
					.Elements()
					.Where(
						element =>
							element.Name.LocalName == "dependency"
					)
					.OrderBy(
						element =>
							element.Attribute( "id" )?.Value,
						StringComparer.Ordinal
					)
					.ToArray();
			Require(
				dependencies.Length == 2,
				$"{targetFramework} must contain exactly the Runtime and Source dependencies."
			);
			AssertDependency(
				dependencies,
				RuntimePackageId,
				expectedVersion,
				targetFramework
			);
			AssertDependency(
				dependencies,
				SourcePackageId,
				expectedVersion,
				targetFramework
			);
			Require(
				!dependencies.Any(
					dependency =>
						dependency.Attribute( "id" )?.Value
							== CompilerPackageId
				),
				$"{targetFramework} must not depend on {CompilerPackageId}."
			);
		}

		XElement? repository =
			metadata
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "repository"
				);
		Require(
			repository is not null,
			"Package metadata has no repository element."
		);
		Require(
			repository!.Attribute( "url" )?.Value == RepositoryUrl,
			"Unexpected repository URL."
		);
		string commit =
			repository.Attribute( "commit" )?.Value
				?? string.Empty;
		Require(
			Regex.IsMatch(
				commit,
				"^[0-9a-fA-F]{40}$",
				RegexOptions.CultureInvariant
			),
			$"Repository metadata has an invalid commit id: '{commit}'."
		);
		return commit;
	}

	private static void VerifyJsonSchema(
		ZipArchive package
	) {
		ArgumentNullException.ThrowIfNull( package );

		ZipArchiveEntry schemaEntry =
			package.GetEntry(
				"docs/Icod.TermInfo.Inspection.schema.json"
			) ?? throw new InvalidOperationException(
				"Inspection package does not contain the published JSON Schema."
			);
		string schema;
		using ( Stream stream = schemaEntry.Open() )
		using ( StreamReader reader = new( stream, Encoding.UTF8 ) ) {
			schema =
				reader
					.ReadToEnd()
					.Replace( "\r\n", "\n", StringComparison.Ordinal )
					.Replace( '\r', '\n' );
		}
		string schemaSha256 =
			Convert.ToHexString(
				SHA256.HashData(
					Encoding.UTF8.GetBytes( schema )
				)
			).ToLowerInvariant();
		Require(
			schemaSha256 == ExpectedJsonSchemaV1Sha256,
			$"Inspection package JSON Schema fingerprint '{schemaSha256}' does not match the frozen version-1 fingerprint."
		);
		using JsonDocument document = JsonDocument.Parse( schema );
		JsonElement root = document.RootElement;

		Require(
			root.GetProperty( "$schema" ).GetString()
				== "https://json-schema.org/draft/2020-12/schema",
			"Inspection package JSON Schema does not identify draft 2020-12."
		);
		Require(
			root.GetProperty( "$id" ).GetString()
				== "urn:icod:terminfo:inspection:json:1",
			"Inspection package JSON Schema does not identify schema version 1."
		);
		Require(
			root.GetProperty( "oneOf" ).GetArrayLength() == 4,
			"Inspection package JSON Schema does not define all four document kinds."
		);
		string[] documentReferences =
			root
				.GetProperty( "oneOf" )
				.EnumerateArray()
				.Select(
					branch => branch.GetProperty( "$ref" ).GetString()
				)
				.Cast<string>()
				.ToArray();
		Require(
			documentReferences.SequenceEqual(
				new[] {
					"#/$defs/terminalDescriptionDocument",
					"#/$defs/comparisonDocument",
					"#/$defs/sourcePlanDocument",
					"#/$defs/databaseCatalogDocument",
				},
				StringComparer.Ordinal
			),
			"Inspection package JSON Schema does not define the reviewed four document kinds."
		);

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

	}

	private static void VerifyAssemblyIdentity(
		ZipArchive package,
		string targetFramework
	) {
		ArgumentNullException.ThrowIfNull( package );
		ArgumentException.ThrowIfNullOrWhiteSpace( targetFramework );

		string path =
			$"lib/{targetFramework}/{PackageId}.dll";
		ZipArchiveEntry? entry =
			package.GetEntry(
				path
			);
		Require(
			entry is not null,
			$"Inspection package is missing {path}."
		);
		string temporaryPath =
			Path.Combine(
				Path.GetTempPath(),
				$"Icod.TermInfo.Inspection-package-verifier-{Guid.NewGuid():N}.dll"
			);
		try {
			using ( Stream source = entry!.Open() )
			using ( FileStream destination = File.Create( temporaryPath ) ) {
				source.CopyTo(
					destination
				);
			}
			AssemblyName assemblyName =
				AssemblyName.GetAssemblyName(
					temporaryPath
				);
			Require(
				assemblyName.Name == PackageId,
				$"{path} has unexpected assembly name '{assemblyName.Name}'."
			);
			Require(
				assemblyName.Version?.ToString() == ExpectedAssemblyVersion,
				$"{path} has assembly version '{assemblyName.Version}', expected {ExpectedAssemblyVersion}."
			);
			byte[]? publicKeyToken =
				assemblyName.GetPublicKeyToken();
			Require(
				publicKeyToken is null
					|| publicKeyToken.Length == 0,
				$"{path} is unexpectedly strong-name signed."
			);
		}
		finally {
			if ( File.Exists( temporaryPath ) ) {
				File.Delete(
					temporaryPath
				);
			}
		}
	}

	private static void VerifyDocumentation(
		ZipArchive package,
		string targetFramework
	) {
		ArgumentNullException.ThrowIfNull( package );
		ArgumentException.ThrowIfNullOrWhiteSpace( targetFramework );

		string path =
			$"lib/{targetFramework}/{PackageId}.xml";
		ZipArchiveEntry? entry =
			package.GetEntry(
				path
			);
		Require(
			entry is not null
				&& entry.Length > 0,
			$"{path} is missing or empty."
		);
		using Stream stream =
			entry!.Open();
		XDocument documentation =
			XDocument.Load(
				stream,
				LoadOptions.None
			);
		string? assemblyName =
			documentation
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "assembly"
				)
				?.Elements()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "name"
				)
				?.Value;
		Require(
			assemblyName == PackageId,
			$"{path} identifies unexpected assembly '{assemblyName}'."
		);
		Require(
			documentation
				.Descendants()
				.Any(
					element =>
						element.Name.LocalName == "member"
				),
			$"{path} contains no documented members."
		);
	}

	private static void VerifySymbols(
		string packagePath,
		string commit
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( packagePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( commit );

		using ZipArchive symbols =
			ZipFile.OpenRead(
				packagePath
			);
		foreach ( string targetFramework in TargetFrameworks ) {
			string path =
				$"lib/{targetFramework}/{PackageId}.pdb";
			ZipArchiveEntry? entry =
				symbols.GetEntry(
					path
				);
			Require(
				entry is not null,
				$"Symbol package is missing {path}."
			);
			using Stream stream =
				entry!.Open();
			using MemoryStream buffer =
				new();
			stream.CopyTo(
				buffer
			);
			byte[] pdb =
				buffer.ToArray();
			Require(
				pdb.AsSpan().StartsWith( "BSJB"u8 ),
				$"{path} is not a portable PDB."
			);
			Require(
				ContainsAscii(
					pdb,
					"raw.githubusercontent.com/uniblab/Icod.TermInfo/"
				),
				$"{path} does not contain the expected GitHub Source Link mapping."
			);
			Require(
				ContainsAscii(
					pdb,
					commit
				),
				$"{path} Source Link data does not contain the package repository commit."
			);
		}
	}

	private static string ReadPackageVersion(
		string root,
		string relativeProjectPath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativeProjectPath );

		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					relativeProjectPath
				),
				LoadOptions.None
			);
		string? version =
			project
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "Version"
				)
				?.Value
				.Trim();
		string? packageVersion =
			project
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "PackageVersion"
				)
				?.Value
				.Trim();
		const string versionReference =
			"$(IcodTermInfoSuiteVersion)";
		Require(
			version == versionReference
				&& packageVersion == versionReference,
			$"{relativeProjectPath}: Version and PackageVersion must consume IcodTermInfoSuiteVersion."
		);

		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props"
				),
				LoadOptions.None
			);
		string? suiteVersion =
			buildProperties
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName == "IcodTermInfoSuiteVersion"
				)
				?.Value
				.Trim();
		Require(
			!string.IsNullOrWhiteSpace( suiteVersion ),
			"Directory.Build.props must declare IcodTermInfoSuiteVersion."
		);
		return suiteVersion!;
	}

	private static ZipArchiveEntry AssertSingleNuspec(
		ZipArchive package
	) {
		ArgumentNullException.ThrowIfNull( package );

		ZipArchiveEntry[] entries =
			package.Entries
				.Where(
					entry =>
						entry.FullName.EndsWith(
							".nuspec",
							StringComparison.OrdinalIgnoreCase
						)
				)
				.ToArray();
		Require(
			entries.Length == 1,
			$"Expected one nuspec, found {entries.Length}."
		);
		return entries[ 0 ];
	}

	private static void AssertDependency(
		IEnumerable<XElement> dependencies,
		string packageId,
		string expectedVersion,
		string targetFramework
	) {
		ArgumentNullException.ThrowIfNull( dependencies );
		ArgumentException.ThrowIfNullOrWhiteSpace( packageId );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );
		ArgumentException.ThrowIfNullOrWhiteSpace( targetFramework );

		XElement? dependency =
			dependencies.SingleOrDefault(
				element =>
					element.Attribute( "id" )?.Value
						== packageId
			);
		Require(
			dependency is not null,
			$"{targetFramework} does not depend on {packageId}."
		);
		Require(
			dependency!.Attribute( "version" )?.Value == expectedVersion,
			$"{targetFramework} does not depend on the matching {packageId} version."
		);
	}

	private static string? GetMetadataText(
		XElement metadata,
		string name
	) {
		ArgumentNullException.ThrowIfNull( metadata );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return metadata
			.Elements()
			.FirstOrDefault(
				element =>
					element.Name.LocalName == name
			)
			?.Value;
	}

	private static bool ContainsAscii(
		byte[] data,
		string text
	) {
		ArgumentNullException.ThrowIfNull( data );
		ArgumentNullException.ThrowIfNull( text );

		return data
			.AsSpan()
			.IndexOf(
				Encoding.ASCII.GetBytes( text )
			) >= 0;
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				Directory.GetCurrentDirectory()
			);
		while ( current is not null ) {
			if ( File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.csproj"
					)
				) ) {
				return current.FullName;
			}
			current =
				current.Parent;
		}
		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}

	private static void Require(
		bool condition,
		string message
	) {
		ArgumentNullException.ThrowIfNull( message );

		if ( !condition ) {
			throw new InvalidDataException(
				message
			);
		}
	}
}
