using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T10SuiteContractTests {
	private const string DevelopmentVersion = "1.4.1";

	[Fact]
	public void AllProjectsAdvanceTogetherAndLibraryAssemblyIdentityRemainsStable() {
		string root = FindRepositoryRoot();
		string[] libraries = [
			"Icod.TermInfo.csproj",
			"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
		];
		string[] commands = [
			"tic/Icod.TermInfo.Tic.csproj",
			"infocmp/Icod.TermInfo.InfoCmp.csproj",
			"toe/Icod.TermInfo.Toe.csproj",
		];

		foreach ( string relativePath in libraries ) {
			XDocument project = XDocument.Load(
				System.IO.Path.Combine(
					root,
					relativePath.Replace(
						'/',
						System.IO.Path.DirectorySeparatorChar
					)
				),
				LoadOptions.None
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
				"1.0.0.0",
				ReadRequiredProperty(
					project,
					"AssemblyVersion"
				)
			);
		}

		foreach ( string relativePath in commands ) {
			XDocument project = XDocument.Load(
				System.IO.Path.Combine(
					root,
					relativePath.Replace(
						'/',
						System.IO.Path.DirectorySeparatorChar
					)
				),
				LoadOptions.None
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
	public void CommandReadmesCoverTheT10DocumentationContract() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"tic/README.md",
				"infocmp/README.md",
				"toe/README.md",
			}
		) {
			string text = File.ReadAllText(
				System.IO.Path.Combine(
					root,
					relativePath.Replace(
						'/',
						System.IO.Path.DirectorySeparatorChar
					)
				)
			);

			foreach (
				string heading
				in new[] {
					"## Synopsis",
					"## Options",
					"## Operands",
					"## Environment",
					"## Exit statuses",
					"## Examples",
					"## Compatibility",
					"## Non-goals",
				}
			) {
				Assert.Contains(
					heading,
					text,
					StringComparison.Ordinal
				);
			}
		}
	}

	[Fact]
	public void ArchiveBuilderNamesAllSixFrameworkDependentSuiteArchives() {
		string root = FindRepositoryRoot();
		string script = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				".github",
				"scripts",
				"build-tool-archives.sh"
			)
		);

		foreach (
			string suffix
			in new[] {
				"win-x64.zip",
				"win-arm64.zip",
				"linux-x64.tar.gz",
				"linux-arm64.tar.gz",
				"osx-x64.tar.gz",
				"osx-arm64.tar.gz",
			}
		) {
			Assert.Contains(
				$"Icod.TermInfo.Tools.${{version}}.{suffix}",
				script,
				StringComparison.Ordinal
			);
		}
		Assert.Contains(
			"--self-contained false",
			script,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReleaseWorkflowPublishesToolArchivesWithPackageAssets() {
		string root = FindRepositoryRoot();
		string workflow = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				".github",
				"workflows",
				"release.yaml"
			)
		);

		Assert.Contains(
			"release-tool-archives",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"build-tool-archives.sh Release",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"artifacts/release-assets",
			workflow,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void RootReadmeDocumentsTheToolSuite() {
		string root = FindRepositoryRoot();
		string readme = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				"README.md"
			)
		);

		Assert.Contains(
			"## Tool Suite",
			readme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"tic",
			readme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"infocmp",
			readme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"toe",
			readme,
			StringComparison.Ordinal
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
