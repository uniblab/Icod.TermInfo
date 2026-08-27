using System.IO.Compression;
using System.Reflection;
using System.Text;
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
				$"Verified {PackageId} multi-target package structure, exact Runtime/Source dependency boundary, assembly identity, symbols, and Source Link for {packageVersion}."
			);
			return 0;
		}
		catch ( Exception exception ) when (
			exception is IOException
			or UnauthorizedAccessException
			or InvalidDataException
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
		Require(
			!string.IsNullOrWhiteSpace( version )
				&& version == packageVersion,
			$"{relativeProjectPath}: Version and PackageVersion must be present and identical."
		);
		return packageVersion!;
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
