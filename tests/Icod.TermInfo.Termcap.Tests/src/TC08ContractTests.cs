using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC08ContractTests {
	private const string DevelopmentVersion = "1.6.0-Alpha-8";
	private const string HistoricalTc07Version = "1.6.0-Alpha-7";
	private const string TermcapApiSnapshotSha256 =
		"1e24b8a555b506594c58cf58d03bf87b2b60192f6316537cb4200498c6a92ab0";

	[Fact]
	public void Tc08AdvancesCentralVersionWithoutRewritingTc07History() {
		string root = FindRepositoryRoot();
		XDocument buildProperties =
			XDocument.Load(
				Path.Combine( root, "Directory.Build.props" ),
				LoadOptions.None
			);
		Assert.Equal(
			DevelopmentVersion,
			buildProperties
				.Descendants()
				.Single( element => element.Name.LocalName == "IcodTermInfoSuiteVersion" )
				.Value
				.Trim()
		);

		string tc07 =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.6.0-TC07-CONVERSION-TOOLS-AND-DISTRIBUTION.md"
				)
			);
		Assert.Contains( HistoricalTc07Version, tc07 );
	}

	[Fact]
	public void Tc08FreezeArtifactsAndPackageGatesArePresent() {
		string root = FindRepositoryRoot();
		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt"
				)
			);
		Assert.Contains( "TERMCAP-API-DOC-ID-SNAPSHOT-V1", baseline );
		Assert.Contains( TermcapApiSnapshotSha256, baseline );
		Assert.Contains( "T:Icod.TermInfo.Termcap.TermcapSourceParser", baseline );
		Assert.Contains( "M:Icod.TermInfo.Termcap.TermcapRenderer.Render", baseline );

		Assert.True(
			File.Exists(
				Path.Combine(
					root,
					"tools",
					"termcap-package-verifier",
					"Icod.TermInfo.Termcap.PackageVerifier.csproj"
				)
			)
		);
		Assert.True(
			File.Exists(
				Path.Combine(
					root,
					"tools",
					"termcap-package-smoke",
					"Icod.TermInfo.Termcap.PackageSmoke.csproj"
				)
			)
		);

		foreach (
			string relativePath
			in new[] {
				".github/scripts/verify-release-package.cmd",
				".github/scripts/verify-release-package.sh",
			}
		) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				);
			Assert.Contains( "termcap-package-verifier", verifier );
			Assert.Contains( "termcap-package-smoke", verifier );
			Assert.Contains( TermcapApiSnapshotSha256, verifier );
			Assert.Contains( "net8.0", verifier );
			Assert.Contains( "net9.0", verifier );
			Assert.Contains( "net10.0", verifier );
		}
	}

	[Fact]
	public void ReleaseAccountingAndTrustedPublishingPrerequisiteRemainExplicit() {
		string root = FindRepositoryRoot();
		string release =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"release.yaml"
				)
			);
		Assert.Contains( "if (17 -ne $files.Count)", release );
		Assert.Contains( "if (18 -ne $assets.Count)", release );
		Assert.Contains( "Icod.TermInfo.Termcap.$version.nupkg", release );

		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.6.0-TC08-DIFFERENTIAL-VALIDATION-FUZZING-AND-FREEZE.md"
				)
			);
		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "trusted publishing", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "17", implementation );
		Assert.Contains( "18", implementation );
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
