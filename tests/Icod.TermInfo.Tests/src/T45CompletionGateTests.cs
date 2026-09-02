using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T45CompletionGateTests {
	[Fact]
	public void AssemblyRetainsStableIdentityForOneEightDevelopment() {
		Assembly assembly =
			typeof( TerminalDescription ).Assembly;
		AssemblyName assemblyName =
			assembly.GetName();
		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;

		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			assemblyName.Version );
		Assert.NotNull(
			informationalVersion );

		string semanticVersion =
			informationalVersion!
				.Split(
					'+',
					2 )[ 0 ];

		Assert.Equal(
			"1.8.0-Alpha-8",
			semanticVersion );
	}

	[Fact]
	public void ProjectMetadataIdentifiesOneEightDevelopmentAndStableAssembly() {
		string root =
			FindRepositoryRoot();
		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.csproj" ),
				LoadOptions.None );
		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props" ),
				LoadOptions.None );

		Assert.Equal(
			"1.8.0-Alpha-8",
			ReadRequiredProperty(
				buildProperties,
				"IcodTermInfoSuiteVersion" ) );
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				project,
				"Version" ) );
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				project,
				"PackageVersion" ) );
		Assert.Equal(
			"1.0.0.0",
			ReadRequiredProperty(
				project,
				"AssemblyVersion" ) );
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			ReadRequiredProperty(
				project,
				"TargetFrameworks" ) );
	}

	[Fact]
	public void FinalReadmeUsesStablePackageVersionAndPolicies() {
		string root =
			FindRepositoryRoot();
		string readme =
			File.ReadAllText(
				Path.Combine(
					root,
					"README.md" ) );

		Assert.Contains(
			"dotnet add package Icod.TermInfo --version 1.7.0",
			readme );
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Source --version 1.7.0",
			readme );
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Termcap --version 1.7.0",
			readme );
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Compiler --version 1.7.0",
			readme );
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Inspection --version 1.7.0",
			readme );
		Assert.Contains(
			"dotnet tool install --global Icod.TermInfo.Tools --version 1.7.0",
			readme );
		Assert.DoesNotContain(
			"1.6.0-Alpha-8 is the current",
			readme );
		Assert.Contains(
			"docs/VERSIONING.md",
			readme );
		Assert.Contains(
			"docs/COMPATIBILITY.md",
			readme );
		Assert.Contains(
			"docs/1.6.0-RELEASE-AUDIT.md",
			readme );
		Assert.Contains(
			"docs/1.7.0-RELEASE-AUDIT.md",
			readme );
	}

	[Fact]
	public void ReleaseVerifierRetainsAllFinalCompatibilityGates() {
		string root =
			FindRepositoryRoot();
		string termcapPackageVerifier =
			File.ReadAllText(
				Path.Combine(
					root,
					"tools",
					"termcap-package-verifier",
					"Program.cs" ) );

		Assert.Contains(
			"docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt",
			termcapPackageVerifier );
		Assert.Contains(
			"TERMCAP-API-DOC-ID-SNAPSHOT-V1",
			termcapPackageVerifier );

		foreach (
			string relativePath
			in new[]
			{
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.cmd"),
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.sh"),
			} ) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath ) );

			Assert.Contains(
				"--check",
				verifier );
			Assert.Contains(
				"--compare",
				verifier );
			Assert.Contains(
				"net8.0",
				verifier );
			Assert.Contains(
				"net9.0",
				verifier );
			Assert.Contains(
				"net10.0",
				verifier );
			Assert.Contains(
				"1.1.0-SOURCE-PUBLIC-API-BASELINE.txt",
				verifier );
			Assert.Contains(
				"1.2.0-COMPILER-PUBLIC-API-BASELINE.txt",
				verifier );
			Assert.Contains(
				"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier );
			Assert.Contains(
				"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier );
			Assert.True(
				verifier.Contains(
					"termcap-package-verifier",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"termcap-package-smoke",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"package-smoke",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"source-package-smoke",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"compiler-package-smoke",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"inspection-package-smoke",
					StringComparison.OrdinalIgnoreCase ) );
			Assert.True(
				verifier.Contains(
					"inspection-package-verifier",
					StringComparison.OrdinalIgnoreCase ) );

			string restoreMarker;
			if (
				relativePath.EndsWith(
					".cmd",
					StringComparison.OrdinalIgnoreCase
				)
			) {
				restoreMarker =
					"set \"NUGET_PACKAGES=%OLD_NUGET_PACKAGES%\"";
			} else {
				restoreMarker =
					"unset NUGET_PACKAGES";
			}
			int restoreIndex =
				verifier.IndexOf(
					restoreMarker,
					StringComparison.Ordinal
				);
			int toolchainIndex =
				verifier.IndexOf(
					"Icod.TermInfo.Toolchain.Sample",
					StringComparison.Ordinal
				);
			int toolchainNoBuildIndex =
				verifier.IndexOf(
					"--no-build",
					toolchainIndex,
					StringComparison.Ordinal
				);

			Assert.True( restoreIndex >= 0 );
			Assert.True( toolchainIndex > restoreIndex );
			Assert.True( toolchainNoBuildIndex > toolchainIndex );
		}
	}

	[Fact]
	public void OneThreeReleaseAuditDefinesTagBasedReleaseSignOff() {
		string root =
			FindRepositoryRoot();
		string audit =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.3.0-RELEASE-AUDIT.md" ) );

		Assert.Contains(
			"Final release candidate",
			audit );
		Assert.Contains(
			"docs/1.0.0-PUBLIC-API-BASELINE.txt",
			audit );
		Assert.Contains(
			"docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt",
			audit );
		Assert.Contains(
			"docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt",
			audit );
		Assert.Contains(
			"docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			audit );
		Assert.Contains(
			"verify-release-package",
			audit );
		Assert.Contains(
			"release.yaml",
			audit );
		Assert.Contains(
			"v1.3.0",
			audit );
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string name ) {
		ArgumentNullException.ThrowIfNull(
			project );
		ArgumentNullException.ThrowIfNull(
			name );

		return project
			.Descendants()
			.Single(
				element =>
					element.Name.LocalName
						== name )
			.Value
			.Trim();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.csproj" ) ) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root." );
	}
}
