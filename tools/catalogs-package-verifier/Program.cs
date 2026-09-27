/*
	Icod.TermInfo.Catalogs.PackageVerifier
	Verifies packaged Icod.TermInfo.Catalogs artifacts and metadata.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.IO.Compression;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Icod.TermInfo.Catalogs.PackageVerifier;

internal static class Program {
	private const string PackageId = "Icod.TermInfo.Catalogs";
	private const string RuntimePackageId = "Icod.TermInfo";
	private const string RepositoryUrl =
		"https://github.com/uniblab/Icod.TermInfo";
	private const string ExpectedAssemblyVersion = "1.0.0.0";
	private const string ExpectedAuthors = "Timothy J. Bruce";
	private const string ExpectedCopyright =
		"Copyright (c) 2026 Timothy J. Bruce";
	private const string ExpectedDescription =
		"Immutable unified directory and hashed terminfo catalog models and bounded acquisition contracts.";
	private const string ExpectedTags =
		"terminfo terminal catalog directory berkeleydb ncurses dotnet csharp";
	private const string ExpectedProjectTags =
		"terminfo;terminal;catalog;directory;berkeleydb;ncurses;dotnet;csharp";
	private static readonly string[] TargetFrameworks = [
		"net8.0",
		"net9.0",
		"net10.0",
	];

	internal static int Run(
		string[] args
	) {
		ArgumentNullException.ThrowIfNull( args );

		bool reconstructUc04 = args.Length == 3 && args[0] == "--reconstruct-uc04";
		if ( args.Length > 1 && !reconstructUc04 ) {
			Console.Error.WriteLine(
				"Usage: catalog-package-verifier [artifact-directory] | --reconstruct-uc04 input-manifest output-manifest"
			);
			return 2;
		}

		try {
			string root = FindRepositoryRoot();
			if ( reconstructUc04 ) {
				string reconstructed = CatalogsUc04Compatibility.Reconstruct( File.ReadAllText( args[1] ),
					File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt" ) )
				);
				string baseline = File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) )
					.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' ).TrimEnd( '\n' ) + "\n";
				Require( reconstructed == baseline, "The reconstructed Catalogs API differs from the UC01 baseline." );
				File.WriteAllText( args[2], reconstructed );
				return 0;
			}
			string artifactDirectory =
				( args.Length == 0 )
					? Path.Combine( root, "artifacts" )
					: Path.GetFullPath( args[0], root )
			;
			string packageVersion =
				ReadPackageVersion(
					root,
					Path.Combine(
						"Icod.TermInfo.Catalogs",
						"Icod.TermInfo.Catalogs.csproj"
					),
					verifyTags: true
				);
			string runtimeVersion =
				ReadPackageVersion(
					root,
					"Icod.TermInfo.csproj",
					verifyTags: false
				);
			Require(
				packageVersion == runtimeVersion,
				$"{PackageId} and {RuntimePackageId} PackageVersion values must match."
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
			Require( File.Exists( nupkg ), $"Package not found: {nupkg}" );
			Require( File.Exists( snupkg ), $"Symbol package not found: {snupkg}" );

			string commit = VerifyPackage( nupkg, packageVersion,
				File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) )
			);
			VerifySymbols( snupkg, packageVersion, commit );

			Console.WriteLine(
				$"Verified {PackageId} {packageVersion}: exact managed nupkg/snupkg, Runtime/Inspection/BerkeleyDb dependencies, IL-only assembly identity, portable symbols, Source Link, and no native assets."
			);
			return 0;
		}
		catch ( Exception exception ) when (
			exception is IOException
			or UnauthorizedAccessException
			or InvalidDataException
			or InvalidOperationException
			or XmlException
		) {
			Console.Error.WriteLine( exception.Message );
			return 1;
		}
	}

	internal static string VerifyPackage(
		string packagePath,
		string expectedVersion,
		string apiBaseline
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( packagePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );

		using ZipArchive package = ZipFile.OpenRead( packagePath );
		HashSet<string> names = ReadUniqueNames( package, "Primary package" );

		List<string> required = [
			"LICENSE",
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
				.Where( name => !names.Contains( name ) )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		Require(
			missing.Length == 0,
			"Catalogs package is missing required entries: "
				+ string.Join( ", ", missing )
		);

		string[] expectedLibraryEntries =
			required
				.Where(
					name => name.StartsWith(
						"lib/",
						StringComparison.Ordinal
					)
				)
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		string[] actualLibraryEntries =
			names
				.Where(
					name => name.StartsWith(
						"lib/",
						StringComparison.Ordinal
					)
				)
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		Require(
			actualLibraryEntries.SequenceEqual(
				expectedLibraryEntries,
				StringComparer.Ordinal
			),
			"Catalogs package library payload is not exact: "
				+ string.Join( ", ", actualLibraryEntries )
		);

		string[] expectedDlls =
			TargetFrameworks
				.Select(
					targetFramework =>
						$"lib/{targetFramework}/{PackageId}.dll"
				)
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		string[] actualDlls =
			names
				.Where(
					name => name.EndsWith(
						".dll",
						StringComparison.OrdinalIgnoreCase
					)
				)
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		Require(
			actualDlls.SequenceEqual(
				expectedDlls,
				StringComparer.Ordinal
			),
			"Catalogs package contains unexpected DLL payloads: "
				+ string.Join( ", ", actualDlls )
		);
		RequireNoNativePayload( names, "Catalogs package" );

		foreach ( string targetFramework in TargetFrameworks ) {
			VerifyAssemblyIdentity( package, targetFramework );
			using Stream assemblyStream = package.GetEntry( $"lib/{targetFramework}/{PackageId}.dll" )!.Open();
			using MemoryStream assemblyBytes = new();
			assemblyStream.CopyTo( assemblyBytes );
			assemblyBytes.Position = 0;
			var context = new System.Runtime.Loader.AssemblyLoadContext( "catalog-api-" + Guid.NewGuid(), isCollectible: true );
			try {
				Assembly assembly = context.LoadFromStream( assemblyBytes );
				string actual = Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( assembly );
				string additionPath = Path.Combine( FindRepositoryRoot(), "docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt" );
				string reconstructed = CatalogsUc04Compatibility.Reconstruct(
					actual, File.ReadAllText( additionPath )
				);
				Require( reconstructed == apiBaseline.Replace( "\r\n", "\n", StringComparison.Ordinal ).TrimEnd( '\n' ) + "\n", "Catalogs packaged public API differs from the UC01 baseline." );
			} finally { context.Unload(); }
			VerifyDocumentation( package, targetFramework );
		}

		XDocument nuspec = ReadNuspec( package );
		XElement metadata = ReadMetadata( nuspec );
		VerifyPrimaryMetadata( metadata, expectedVersion );
		VerifyDependencyGroups( metadata, expectedVersion );
		return ReadRepositoryCommit( metadata, expectedCommit: null );
	}

	private static void VerifyPrimaryMetadata(
		XElement metadata,
		string expectedVersion
	) {
		ArgumentNullException.ThrowIfNull( metadata );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );

		RequireMetadata( metadata, "id", PackageId );
		RequireMetadata( metadata, "version", expectedVersion );
		RequireMetadata( metadata, "title", PackageId );
		RequireMetadata( metadata, "authors", ExpectedAuthors );
		RequireMetadata( metadata, "projectUrl", RepositoryUrl );
		RequireMetadata( metadata, "readme", "README.md" );
		RequireMetadata( metadata, "icon", "icon.png" );
		RequireMetadata(
			metadata,
			"description",
			ExpectedDescription
		);
		RequireMetadata(
			metadata,
			"copyright",
			ExpectedCopyright
		);
		RequireMetadata(
			metadata,
			"requireLicenseAcceptance",
			"true"
		);
		Require(
			NormalizeWhitespace(
				GetMetadataText( metadata, "tags" ) ?? string.Empty
			) == ExpectedTags,
			"Unexpected package tags."
		);

		XElement? license =
			metadata.Elements()
				.FirstOrDefault(
					element => element.Name.LocalName == "license"
				);
		Require( license is not null, "Package metadata has no license element." );
		Require(
			license!.Attribute( "type" )?.Value == "expression",
			"Package license is not an expression."
		);
		Require(
			license.Value == "LGPL-3.0-or-later",
			"Unexpected package license expression."
		);
	}

	private static void VerifyAssemblyIdentity(
		ZipArchive package,
		string targetFramework
	) {
		ArgumentNullException.ThrowIfNull( package );
		ArgumentException.ThrowIfNullOrWhiteSpace( targetFramework );

		string path = $"lib/{targetFramework}/{PackageId}.dll";
		ZipArchiveEntry? entry = package.GetEntry( path );
		Require( entry is not null, $"Catalogs package is missing {path}." );
		string temporaryPath =
			Path.Combine(
				Path.GetTempPath(),
				$"Icod.TermInfo.Catalogs-package-verifier-{Guid.NewGuid():N}.dll"
			);
		try {
			using ( Stream source = entry!.Open() ) {
				using FileStream destination = File.Create( temporaryPath );
				source.CopyTo( destination );
			}

			AssemblyName assemblyName =
				AssemblyName.GetAssemblyName( temporaryPath );
			Require(
				assemblyName.Name == PackageId,
				$"{path} has unexpected assembly name '{assemblyName.Name}'."
			);
			Require(
				assemblyName.Version?.ToString() == ExpectedAssemblyVersion,
				$"{path} has assembly version '{assemblyName.Version}', expected {ExpectedAssemblyVersion}."
			);
			byte[]? publicKeyToken = assemblyName.GetPublicKeyToken();
			Require(
				publicKeyToken is null || publicKeyToken.Length == 0,
				$"{path} is unexpectedly strong-name signed."
			);

			using FileStream assembly = File.OpenRead( temporaryPath );
			using PEReader peReader = new( assembly );
			Require( peReader.HasMetadata, $"{path} has no managed metadata." );
			CorHeader? corHeader = peReader.PEHeaders.CorHeader;
			Require( corHeader is not null, $"{path} has no CLR header." );
			Require(
				(corHeader!.Flags & CorFlags.ILOnly) == CorFlags.ILOnly,
				$"{path} is not IL-only."
			);
			Require(
				corHeader.EntryPointTokenOrRelativeVirtualAddress == 0,
				$"{path} has an unexpected native entry point."
			);
		}
		finally {
			if ( File.Exists( temporaryPath ) ) {
				File.Delete( temporaryPath );
			}
		}
	}

	private static void VerifyDocumentation(
		ZipArchive package,
		string targetFramework
	) {
		ArgumentNullException.ThrowIfNull( package );
		ArgumentException.ThrowIfNullOrWhiteSpace( targetFramework );

		string path = $"lib/{targetFramework}/{PackageId}.xml";
		ZipArchiveEntry? entry = package.GetEntry( path );
		Require(
			entry is not null && entry.Length > 0,
			$"{path} is missing or empty."
		);
		using Stream stream = entry!.Open();
		XDocument documentation = XDocument.Load( stream, LoadOptions.None );
		string? assemblyName =
			documentation.Descendants()
				.FirstOrDefault(
					element => element.Name.LocalName == "assembly"
				)
				?.Elements()
				.FirstOrDefault(
					element => element.Name.LocalName == "name"
				)
				?.Value;
		Require(
			assemblyName == PackageId,
			$"{path} identifies unexpected assembly '{assemblyName}'."
		);
		Require(
			documentation.Descendants()
				.Any( element => element.Name.LocalName == "member" ),
			$"{path} contains no documented members."
		);
	}

	private static void VerifySymbols(
		string packagePath,
		string expectedVersion,
		string commit
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( packagePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );
		ArgumentException.ThrowIfNullOrWhiteSpace( commit );

		using ZipArchive symbols = ZipFile.OpenRead( packagePath );
		HashSet<string> names = ReadUniqueNames( symbols, "Symbol package" );
		string[] expectedPdbs =
			TargetFrameworks
				.Select(
					targetFramework =>
						$"lib/{targetFramework}/{PackageId}.pdb"
				)
				.OrderBy( path => path, StringComparer.Ordinal )
				.ToArray();
		string[] actualPdbs =
			names
				.Where(
					name => name.EndsWith(
						".pdb",
						StringComparison.OrdinalIgnoreCase
					)
				)
				.OrderBy( path => path, StringComparer.Ordinal )
				.ToArray();
		Require(
			actualPdbs.SequenceEqual(
				expectedPdbs,
				StringComparer.Ordinal
			),
			"Symbol package PDB payload is not exact: "
				+ string.Join( ", ", actualPdbs )
		);
		Require(
			!names.Any(
				name => name.EndsWith(
					".dll",
					StringComparison.OrdinalIgnoreCase
				)
			),
			"Symbol package unexpectedly contains a DLL."
		);
		RequireNoNativePayload( names, "Symbol package" );

		foreach ( string path in expectedPdbs ) {
			ZipArchiveEntry? entry = symbols.GetEntry( path );
			Require( entry is not null, $"Symbol package is missing {path}." );
			using Stream stream = entry!.Open();
			using MemoryStream buffer = new();
			stream.CopyTo( buffer );
			byte[] pdb = buffer.ToArray();
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
				ContainsAscii( pdb, commit ),
				$"{path} Source Link data does not contain the package repository commit."
			);
		}

		XDocument nuspec = ReadNuspec( symbols );
		XElement metadata = ReadMetadata( nuspec );
		RequireMetadata( metadata, "id", PackageId );
		RequireMetadata( metadata, "version", expectedVersion );
		XElement[] packageTypes =
			metadata.Descendants()
				.Where(
					element => element.Name.LocalName == "packageType"
				)
				.ToArray();
		Require(
			packageTypes.Length == 1
				&& packageTypes[0].Attribute( "name" )?.Value
					== "SymbolsPackage",
			"Symbol package type is not exactly SymbolsPackage."
		);
		VerifyDependencyGroups( metadata, expectedVersion );
		ReadRepositoryCommit( metadata, commit );
	}

	private static void VerifyDependencyGroups(
		XElement metadata,
		string expectedVersion
	) {
		ArgumentNullException.ThrowIfNull( metadata );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );

		XElement? dependenciesElement =
			metadata.Elements()
				.SingleOrDefault(
					element => element.Name.LocalName == "dependencies"
				);
		Require(
			dependenciesElement is not null,
			"Package metadata has no dependencies element."
		);
		XElement[] groups =
			dependenciesElement!.Elements()
				.Where( element => element.Name.LocalName == "group" )
				.ToArray();
		Require(
			groups.Length == TargetFrameworks.Length,
			"Dependency groups do not match the supported target frameworks."
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
				group!.Elements()
					.Where(
						element => element.Name.LocalName == "dependency"
					)
					.ToArray();
			string[] expectedIds = [ RuntimePackageId, "Icod.TermInfo.BerkeleyDb", "Icod.TermInfo.Inspection" ];
			Require( dependencies.Select( dependency => dependency.Attribute( "id" )?.Value )
				.OrderBy( id => id, StringComparer.Ordinal ).SequenceEqual( expectedIds, StringComparer.Ordinal ),
				$"{targetFramework} must contain exactly Runtime, BerkeleyDb, and Inspection dependencies."
			);
			Require( dependencies.All( dependency => dependency.Attribute( "version" )?.Value == expectedVersion
				&& dependency.Attribute( "exclude" )?.Value == "Build,Analyzers"
			),
				$"{targetFramework} dependencies must use the coordinated version and standard exclusions."
			);
		}
	}

	private static string ReadRepositoryCommit(
		XElement metadata,
		string? expectedCommit
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		XElement? repository =
			metadata.Elements()
				.SingleOrDefault(
					element => element.Name.LocalName == "repository"
				);
		Require(
			repository is not null,
			"Package metadata has no repository element."
		);
		Require(
			repository!.Attribute( "type" )?.Value == "git",
			"Repository metadata is not git."
		);
		Require(
			repository.Attribute( "url" )?.Value == RepositoryUrl,
			"Unexpected repository URL."
		);
		string commit = repository.Attribute( "commit" )?.Value ?? string.Empty;
		Require(
			Regex.IsMatch(
				commit,
				"^[0-9a-fA-F]{40}$",
				RegexOptions.CultureInvariant
			),
			$"Repository metadata has an invalid commit id: '{commit}'."
		);
		Require(
			expectedCommit is null || commit == expectedCommit,
			"Symbol package repository commit does not match the primary package."
		);
		return commit;
	}

	private static string ReadPackageVersion(
		string root,
		string relativeProjectPath,
		bool verifyTags
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativeProjectPath );

		XDocument project =
			XDocument.Load(
				Path.Combine( root, relativeProjectPath ),
				LoadOptions.None
			);
		string? version =
			project.Descendants()
				.FirstOrDefault(
					element => element.Name.LocalName == "Version"
				)
				?.Value
				.Trim();
		string? packageVersion =
			project.Descendants()
				.FirstOrDefault(
					element => element.Name.LocalName == "PackageVersion"
				)
				?.Value
				.Trim();
		const string versionReference = "$(IcodTermInfoSuiteVersion)";
		Require(
			version == versionReference
				&& packageVersion == versionReference,
			$"{relativeProjectPath}: Version and PackageVersion must consume IcodTermInfoSuiteVersion."
		);
		if ( verifyTags ) {
			string? tags =
				project.Descendants()
					.FirstOrDefault(
						element => element.Name.LocalName == "PackageTags"
					)
					?.Value
					.Trim();
			Require(
				tags == ExpectedProjectTags,
				$"{relativeProjectPath}: PackageTags do not match the UC01 authority."
			);
		}

		XDocument buildProperties =
			XDocument.Load(
				Path.Combine( root, "Directory.Build.props" ),
				LoadOptions.None
			);
		string? suiteVersion =
			buildProperties.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName
							== "IcodTermInfoSuiteVersion"
				)
				?.Value
				.Trim();
		Require(
			!string.IsNullOrWhiteSpace( suiteVersion ),
			"Directory.Build.props must declare IcodTermInfoSuiteVersion."
		);
		return suiteVersion!;
	}

	private static HashSet<string> ReadUniqueNames(
		ZipArchive package,
		string description
	) {
		ArgumentNullException.ThrowIfNull( package );
		ArgumentException.ThrowIfNullOrWhiteSpace( description );

		HashSet<string> names =
			package.Entries
				.Select( entry => entry.FullName )
				.ToHashSet( StringComparer.Ordinal );
		Require(
			names.Count == package.Entries.Count,
			$"{description} contains duplicate entry names."
		);
		return names;
	}

	private static XDocument ReadNuspec(
		ZipArchive package
	) {
		ArgumentNullException.ThrowIfNull( package );

		ZipArchiveEntry[] entries =
			package.Entries
				.Where(
					entry => entry.FullName.EndsWith(
						".nuspec",
						StringComparison.OrdinalIgnoreCase
					)
				)
				.ToArray();
		Require(
			entries.Length == 1,
			$"Expected one nuspec, found {entries.Length}."
		);
		using Stream stream = entries[0].Open();
		return XDocument.Load( stream, LoadOptions.None );
	}

	private static XElement ReadMetadata(
		XDocument nuspec
	) {
		ArgumentNullException.ThrowIfNull( nuspec );

		XElement[] elements =
			nuspec.Descendants()
				.Where( element => element.Name.LocalName == "metadata" )
				.ToArray();
		Require(
			elements.Length == 1,
			$"Expected one metadata element, found {elements.Length}."
		);
		return elements[0];
	}

	private static void RequireMetadata(
		XElement metadata,
		string name,
		string expected
	) {
		ArgumentNullException.ThrowIfNull( metadata );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( expected );

		string? actual = GetMetadataText( metadata, name );
		Require(
			actual == expected,
			$"Unexpected package {name}: '{actual}'."
		);
	}

	private static string? GetMetadataText(
		XElement metadata,
		string name
	) {
		ArgumentNullException.ThrowIfNull( metadata );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return metadata.Elements()
			.SingleOrDefault(
				element => element.Name.LocalName == name
			)
			?.Value;
	}

	private static string NormalizeWhitespace(
		string value
	) {
		ArgumentNullException.ThrowIfNull( value );
		return string.Join(
			" ",
			value.Split(
				(char[]?)null,
				StringSplitOptions.RemoveEmptyEntries
			)
		);
	}

	private static void RequireNoNativePayload(
		IEnumerable<string> names,
		string description
	) {
		ArgumentNullException.ThrowIfNull( names );
		ArgumentException.ThrowIfNullOrWhiteSpace( description );

		string[] native =
			names
				.Where(
					name =>
						name.StartsWith( "runtimes/", StringComparison.Ordinal )
						|| HasNativeExtension( name )
				)
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		Require(
			native.Length == 0,
			$"{description} contains native/runtime-specific payloads: "
				+ string.Join( ", ", native )
		);
	}

	private static bool HasNativeExtension(
		string name
	) {
		ArgumentNullException.ThrowIfNull( name );

		return name.EndsWith( ".so", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".dylib", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".a", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".lib", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".o", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".obj", StringComparison.OrdinalIgnoreCase )
			|| name.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase );
	}

	private static bool ContainsAscii(
		byte[] data,
		string text
	) {
		ArgumentNullException.ThrowIfNull( data );
		ArgumentNullException.ThrowIfNull( text );

		return data.AsSpan().IndexOf( Encoding.ASCII.GetBytes( text ) ) >= 0;
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( Directory.GetCurrentDirectory() );
		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine( current.FullName, "Icod.TermInfo.csproj" )
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
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
			throw new InvalidDataException( message );
		}
	}
}
