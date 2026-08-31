using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class TC07ContractTests {
	private const string DevelopmentVersion =
		"1.6.0-Alpha-7";
	private const string VersionReference =
		"$(IcodTermInfoSuiteVersion)";

	[Fact]
	public void Tc07VersionAndCentralVersionWiringRemainRecorded() {
		string root =
			FindRepositoryRoot();
		XDocument router =
			LoadProject(
				root,
				"icod-terminfo/Icod.TermInfo.Router.csproj"
			);
		string implementation =
			ReadRepositoryFile(
				root,
				"docs/1.6.0-TC07-CONVERSION-TOOLS-AND-DISTRIBUTION.md"
			);

		Assert.Contains(
			DevelopmentVersion,
			implementation
		);
		Assert.Equal(
			VersionReference,
			ReadRequiredProperty(
				router,
				"Version"
			)
		);
		Assert.Equal(
			VersionReference,
			ReadRequiredProperty(
				router,
				"PackageVersion"
			)
		);
	}

	[Fact]
	public void ConversionCommandsAreStandaloneNonPackableCompositionProjects() {
		string root =
			FindRepositoryRoot();

		XDocument capToInfo =
			LoadProject(
				root,
				"captoinfo/Icod.TermInfo.CapToInfo.csproj"
			);
		XDocument infoToCap =
			LoadProject(
				root,
				"infotocap/Icod.TermInfo.InfoToCap.csproj"
			);

		AssertCommandProject(
			capToInfo,
			new[] {
				@"..\Icod.TermInfo.Inspection\Icod.TermInfo.Inspection.csproj",
				@"..\Icod.TermInfo.Termcap\Icod.TermInfo.Termcap.csproj",
			}
		);
		AssertCommandProject(
			infoToCap,
			new[] {
				@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
				@"..\Icod.TermInfo.Termcap\Icod.TermInfo.Termcap.csproj",
			}
		);

		Assert.DoesNotContain(
			ProjectReferences( capToInfo ),
			reference =>
				reference.Contains(
					@"\infotocap\",
					StringComparison.OrdinalIgnoreCase
				)
		);
		Assert.DoesNotContain(
			ProjectReferences( infoToCap ),
			reference =>
				reference.Contains(
					@"\captoinfo\",
					StringComparison.OrdinalIgnoreCase
				)
		);
	}

	[Fact]
	public void RouterAndDistributionEnumerateBothConversionCommands() {
		string root =
			FindRepositoryRoot();
		string router =
			System.IO.File.ReadAllText(
				System.IO.Path.Combine(
					root,
					"icod-terminfo",
					"src",
					"Command.cs"
				)
			);
		string archiveBuilder =
			ReadRepositoryFile(
				root,
				".github/scripts/build-tool-archives.sh"
			);
		string archiveVerifier =
			ReadRepositoryFile(
				root,
				".github/scripts/verify-tool-archives.sh"
			);
		string packageVerifier =
			ReadRepositoryFile(
				root,
				"tools/tool-package-verifier/Program.cs"
			);

		foreach (
			string command
			in new[] {
				"captoinfo",
				"infotocap",
			}
		) {
			Assert.Contains( command, router, StringComparison.Ordinal );
			Assert.Contains( command, archiveBuilder, StringComparison.Ordinal );
			Assert.Contains( command, archiveVerifier, StringComparison.Ordinal );
			Assert.Contains( command, packageVerifier, StringComparison.Ordinal );
		}
		Assert.Contains(
			"Icod.TermInfo.Termcap.dll",
			packageVerifier,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void TagReleaseAccountsForTermcapAndTc07Commands() {
		string root =
			FindRepositoryRoot();
		string release =
			ReadRepositoryFile(
				root,
				".github/workflows/release.yaml"
			);

		Assert.Contains(
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
			release,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"captoinfo/Icod.TermInfo.CapToInfo.csproj",
			release,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"infotocap/Icod.TermInfo.InfoToCap.csproj",
			release,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.Termcap.$version.nupkg",
			release,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void Tc07ImplementationRecordFreezesTheSelectedTopology() {
		string root =
			FindRepositoryRoot();
		string implementationPath =
			System.IO.Path.Combine(
				root,
				"docs",
				"1.6.0-TC07-CONVERSION-TOOLS-AND-DISTRIBUTION.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		string implementation =
			File.ReadAllText( implementationPath );
		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "captoinfo", implementation );
		Assert.Contains( "infotocap", implementation );
		Assert.Contains( "effective resolved state", implementation );
		Assert.Contains( "TC08", implementation );
	}

	private static void AssertCommandProject(
		XDocument project,
		IReadOnlyList<string> expectedReferences
	) {
		ArgumentNullException.ThrowIfNull( project );
		ArgumentNullException.ThrowIfNull( expectedReferences );

		Assert.Equal(
			VersionReference,
			ReadRequiredProperty(
				project,
				"Version"
			)
		);
		Assert.Equal(
			"false",
			ReadRequiredProperty(
				project,
				"IsPackable"
			)
		);
		Assert.Equal(
			"false",
			ReadRequiredProperty(
				project,
				"UseAppHost"
			)
		);
		Assert.Equal(
			expectedReferences
				.OrderBy(
					value => value,
					StringComparer.Ordinal
				)
				.ToArray(),
			ProjectReferences( project )
		);
	}

	private static string[] ProjectReferences(
		XDocument project
	) {
		ArgumentNullException.ThrowIfNull( project );

		return project
			.Descendants()
			.Where(
				element =>
					element.Name.LocalName == "ProjectReference"
			)
			.Select(
				element =>
					element.Attribute( "Include" )?.Value
						?? string.Empty
			)
			.OrderBy(
				value => value,
				StringComparer.Ordinal
			)
			.ToArray();
	}

	private static XDocument LoadProject(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return XDocument.Load(
			System.IO.Path.Combine(
				root,
				relativePath.Replace(
					'/',
					System.IO.Path.DirectorySeparatorChar
				)
			),
			LoadOptions.None
		);
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string propertyName
	) {
		ArgumentNullException.ThrowIfNull( project );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );

		return project
			.Descendants()
			.First(
				element =>
					element.Name.LocalName == propertyName
			)
			.Value
			.Trim();
	}

	private static string ReadRepositoryFile(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return File.ReadAllText(
			System.IO.Path.Combine(
				root,
				relativePath.Replace(
					'/',
					System.IO.Path.DirectorySeparatorChar
				)
			)
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);

		while ( current is not null ) {
			if (
				System.IO.File.Exists(
					System.IO.Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
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
}
