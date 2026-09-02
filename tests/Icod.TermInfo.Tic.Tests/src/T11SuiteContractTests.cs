using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T11SuiteContractTests {
	private const string DevelopmentVersion = "$(IcodTermInfoSuiteVersion)";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void CoordinatedProjectsAreAtStableOneFourAndLibraryIdentityIsFrozen() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
				"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
				"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			}
		) {
			XDocument project = LoadProject(
				root,
				relativePath
			);
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"Version"
				)
			);
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"PackageVersion"
				)
			);
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty(
					project,
					"AssemblyVersion"
				)
			);
		}

		foreach (
			string relativePath
			in new[] {
				"tic/Icod.TermInfo.Tic.csproj",
				"infocmp/Icod.TermInfo.InfoCmp.csproj",
				"toe/Icod.TermInfo.Toe.csproj",
			}
		) {
			XDocument project = LoadProject(
				root,
				relativePath
			);
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"Version"
				)
			);
		}
	}

	[Fact]
	public void CheckedInNcursesCorpusRemainsNormalCiEvidence() {
		string root = FindRepositoryRoot();
		string fixtureRoot = System.IO.Path.Combine(
			root,
			"tests",
			"Icod.TermInfo.Tests",
			"fixtures",
			"compiled-terminfo"
		);
		string readme = File.ReadAllText(
			System.IO.Path.Combine(
				fixtureRoot,
				"README.md"
			)
		);

		Assert.Contains(
			"Normal tests consume these checked-in assets",
			readme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"do not require `tic`, ncurses",
			readme,
			StringComparison.Ordinal
		);

		foreach (
			string fixtureStem
			in new[] {
				"t29-extended",
				"t29-extended32",
				"t29-legacy-alignment",
				"t29-legacy-edge",
				"t29-legacy-minimal",
			}
		) {
			Assert.True(
				File.Exists(
					System.IO.Path.Combine(
						fixtureRoot,
						"source",
						fixtureStem + ".ti"
					)
				)
			);
			Assert.True(
				File.Exists(
					System.IO.Path.Combine(
						fixtureRoot,
						"compiled",
						fixtureStem + ".bin"
					)
				)
			);
		}
	}

	[Fact]
	public void ToolArchiveVerificationIsRequiredByPrMainAndReleaseCi() {
		string root = FindRepositoryRoot();
		string verifier = ReadRepositoryFile(
			root,
			".github/scripts/verify-tool-archives.sh"
		);
		string archiveBuilder = ReadRepositoryFile(
			root,
			"packaging/BuildToolArchives.ps1"
		);

		foreach (
			string marker
			in new[] {
				"win-x64",
				"win-arm64",
				"linux-x64",
				"linux-arm64",
				"osx-x64",
				"osx-arm64",
				"TOOL-SUITE.txt",
				"Framework: net10.0",
				"Deployment: framework-dependent",
				"*.pdb",
				"*.csproj",
				"*.sln",
			}
		) {
			Assert.Contains(
				marker,
				verifier,
				StringComparison.Ordinal
			);
		}

		Assert.Contains(
			"verify-tool-archives.sh",
			archiveBuilder,
			StringComparison.Ordinal
		);
		foreach (
			string relativePath
			in new[] {
				".github/workflows/pull-request.yaml",
				".github/workflows/main.yaml",
				".github/workflows/release.yaml",
			}
		) {
			Assert.Contains(
				"BuildToolArchives.ps1",
				ReadRepositoryFile(
					root,
					relativePath
				),
				StringComparison.Ordinal
			);
		}
	}

	[Fact]
	public void ReleaseArtifactModelIsSixPackagesSixArchivesAndManifest() {
		string root = FindRepositoryRoot();
		string release = ReadRepositoryFile(
			root,
			".github/workflows/release.yaml"
		);

		Assert.Contains(
			"if (17 -ne $files.Count)",
			release,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"if (18 -ne $assets.Count)",
			release,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"SHA256SUMS.txt",
			release,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void T11ImplementationRecordIsPresent() {
		string root = FindRepositoryRoot();

		Assert.True(
			File.Exists(
				System.IO.Path.Combine(
					root,
					"docs",
					"1.4.0-T11-DIFFERENTIAL-VALIDATION-HOSTILE-INPUT-AND-FREEZE.md"
				)
			)
		);
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
					string.Equals(
						element.Name.LocalName,
						propertyName,
						StringComparison.Ordinal
					)
			)
			.Value;
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
		DirectoryInfo? current = new(
			AppContext.BaseDirectory
		);
		while ( current is not null ) {
			if (
				File.Exists(
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

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
