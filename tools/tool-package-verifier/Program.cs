using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace Icod.TermInfo.Tools.PackageVerifier;

internal static class Program {
	private const string PackageId = "Icod.TermInfo.Tools";
	private const string RepositoryUrl =
		"https://github.com/uniblab/Icod.TermInfo";
	private const string ToolRoot =
		"tools/net10.0/any/";

	public static int Main(
		string[] args
	) {
		ArgumentNullException.ThrowIfNull( args );

		if ( args.Length > 1 ) {
			Console.Error.WriteLine(
				"Usage: dotnet run --project tools/tool-package-verifier/"
				+ "Icod.TermInfo.Tools.PackageVerifier.csproj -- "
				+ "[artifact-directory]"
			);
			return 2;
		}

		try {
			string root =
				FindRepositoryRoot();
			string artifactDirectory =
				( args.Length == 0 )
					? Path.Combine( root, "artifacts" )
					: Path.GetFullPath( args[0], root )
				;
			string version =
				ReadSuiteVersion( root );
			string packagePath =
				Path.Combine(
					artifactDirectory,
					$"{PackageId}.{version}.nupkg"
				);

			Require(
				File.Exists( packagePath ),
				$"Router package not found: {packagePath}"
			);
			VerifyPackage(
				packagePath,
				version
			);

			Console.WriteLine(
				$"Verified host-neutral {PackageId} {version} package structure."
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

	private static string FindRepositoryRoot() {
		string[] starts = [
			Directory.GetCurrentDirectory(),
			AppContext.BaseDirectory,
		];

		foreach ( string start in starts ) {
			DirectoryInfo? current =
				new( start );
			while ( current is not null ) {
				if (
					File.Exists(
						Path.Combine(
							current.FullName,
							"Directory.Build.props"
						)
					)
					&& File.Exists(
						Path.Combine(
							current.FullName,
							"icod-terminfo",
							"Icod.TermInfo.Router.csproj"
						)
					)
				) {
					return current.FullName;
				}

				current = current.Parent;
			}
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}

	private static string ReadSuiteVersion(
		string root
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );

		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props"
				),
				LoadOptions.None
			);
		XElement? versionNode =
			buildProperties
				.Descendants()
				.FirstOrDefault(
					element =>
						element.Name.LocalName
							== "IcodTermInfoSuiteVersion"
				);
		string version =
			versionNode?.Value.Trim()
			?? string.Empty;
		Require(
			!string.IsNullOrWhiteSpace( version ),
			"Directory.Build.props must declare IcodTermInfoSuiteVersion."
		);
		return version;
	}

	private static void VerifyPackage(
		string packagePath,
		string expectedVersion
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( packagePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedVersion );

		using ZipArchive package =
			ZipFile.OpenRead( packagePath );
		HashSet<string> names =
			package
				.Entries
				.Select(
					entry => entry.FullName
				)
				.ToHashSet(
					StringComparer.Ordinal
				);

		string[] requiredEntries = [
			"LICENSE",
			"README.md",
			"icon.png",
			ToolRoot + "DotnetToolSettings.xml",
			ToolRoot + "icod-terminfo.dll",
			ToolRoot + "tic.dll",
			ToolRoot + "infocmp.dll",
			ToolRoot + "toe.dll",
			ToolRoot + "Icod.TermInfo.dll",
			ToolRoot + "Icod.TermInfo.Source.dll",
			ToolRoot + "Icod.TermInfo.Compiler.dll",
			ToolRoot + "Icod.TermInfo.Inspection.dll",
		];
		foreach ( string requiredEntry in requiredEntries ) {
			Require(
				names.Contains( requiredEntry ),
				$"Router package is missing required entry '{requiredEntry}'."
			);
		}

		string[] forbiddenAppHosts = [
			ToolRoot + "icod-terminfo",
			ToolRoot + "icod-terminfo.exe",
			ToolRoot + "tic",
			ToolRoot + "tic.exe",
			ToolRoot + "infocmp",
			ToolRoot + "infocmp.exe",
			ToolRoot + "toe",
			ToolRoot + "toe.exe",
		];
		foreach ( string forbiddenAppHost in forbiddenAppHosts ) {
			Require(
				!names.Contains( forbiddenAppHost ),
				$"Host-specific apphost leaked into router package: '{forbiddenAppHost}'."
			);
		}

		Require(
			!names.Any(
				name =>
					name.StartsWith(
						"runtimes/",
						StringComparison.Ordinal
					)
			),
			"Router package unexpectedly contains a runtimes/ payload."
		);
		Require(
			!names.Any(
				name =>
					name.StartsWith(
						"lib/",
						StringComparison.Ordinal
					)
					|| name.StartsWith(
						"ref/",
						StringComparison.Ordinal
					)
			),
			"Router package must remain a tool package rather than a reusable library package."
		);

		ZipArchiveEntry nuspecEntry =
			package
				.Entries
				.Single(
					entry =>
						entry.FullName.EndsWith(
							".nuspec",
							StringComparison.Ordinal
						)
				);
		XDocument nuspec =
			ReadXml( nuspecEntry );
		Require(
			ReadRequiredElement( nuspec, "id" ) == PackageId,
			"Router package ID is not Icod.TermInfo.Tools."
		);
		Require(
			ReadRequiredElement( nuspec, "version" ) == expectedVersion,
			"Router package version does not match IcodTermInfoSuiteVersion."
		);
		XElement packageType =
			nuspec
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName == "packageType"
				);
		Require(
			packageType.Attribute( "name" )?.Value == "DotnetTool",
			"Router package type is not DotnetTool."
		);
		XElement repository =
			nuspec
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName == "repository"
				);
		Require(
			repository.Attribute( "url" )?.Value == RepositoryUrl,
			"Router package repository URL is incorrect."
		);
		Require(
			!string.IsNullOrWhiteSpace(
				repository.Attribute( "commit" )?.Value
			),
			"Router package repository commit metadata is missing."
		);

		ZipArchiveEntry settingsEntry =
			package.GetEntry(
				ToolRoot + "DotnetToolSettings.xml"
			)
			?? throw new InvalidDataException(
				"Router package is missing DotnetToolSettings.xml."
			);
		XDocument settings =
			ReadXml( settingsEntry );
		XElement[] commands =
			settings
				.Descendants()
				.Where(
					element =>
						element.Name.LocalName == "Command"
				)
				.ToArray();
		Require(
			commands.Length == 1,
			"Router package must expose exactly one .NET tool command."
		);
		Require(
			commands[0].Attribute( "Name" )?.Value == "icod-terminfo"
				&& commands[0].Attribute( "EntryPoint" )?.Value == "icod-terminfo.dll"
				&& commands[0].Attribute( "Runner" )?.Value == "dotnet",
			"Router DotnetToolSettings.xml does not expose the expected icod-terminfo command."
		);
	}

	private static XDocument ReadXml(
		ZipArchiveEntry entry
	) {
		ArgumentNullException.ThrowIfNull( entry );

		using Stream stream =
			entry.Open();
		return XDocument.Load(
			stream,
			LoadOptions.None
		);
	}

	private static string ReadRequiredElement(
		XDocument document,
		string localName
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentException.ThrowIfNullOrWhiteSpace( localName );

		return document
			.Descendants()
			.Single(
				element =>
					element.Name.LocalName == localName
			)
			.Value
			.Trim();
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
