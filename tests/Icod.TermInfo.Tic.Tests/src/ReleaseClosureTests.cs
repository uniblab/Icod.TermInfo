using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class ReleaseClosureTests {
	private const string ReleaseVersion = "1.4.0";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void AllSevenProjectsAreAtStableOneFour() {
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
			XDocument project = LoadProject( root, relativePath );
			Assert.Equal(
				ReleaseVersion,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				ReleaseVersion,
				ReadRequiredProperty( project, "PackageVersion" )
			);
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty( project, "AssemblyVersion" )
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
			XDocument project = LoadProject( root, relativePath );
			Assert.Equal(
				ReleaseVersion,
				ReadRequiredProperty( project, "Version" )
			);
		}
	}

	[Fact]
	public void MainAndTagWorkflowsSmokeUnpackedToolArchives() {
		string root = FindRepositoryRoot();
		string smoke = ReadRepositoryFile(
			root,
			".github/scripts/smoke-tool-archive.ps1"
		);

		foreach (
			string marker
			in new[] {
				"tic",
				"infocmp",
				"toe",
				"--version",
				"release-smoke",
			}
		) {
			Assert.Contains(
				marker,
				smoke,
				StringComparison.Ordinal
			);
		}

		foreach (
			string workflow
			in new[] {
				".github/workflows/push-main.yaml",
				".github/workflows/release.yaml",
			}
		) {
			Assert.Contains(
				"smoke-tool-archive.ps1",
				ReadRepositoryFile( root, workflow ),
				StringComparison.Ordinal
			);
		}

		string release = ReadRepositoryFile(
			root,
			".github/workflows/release.yaml"
		);
		Assert.Contains(
			"needs: [metadata, validate, tool-archives, smoke-tool-archives]",
			release,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void StableReleaseAuditAndPackageFacingDocumentationArePresent() {
		string root = FindRepositoryRoot();

		Assert.True(
			File.Exists(
				System.IO.Path.Combine(
					root,
					"docs",
					"1.4.0-RELEASE-AUDIT.md"
				)
			)
		);

		foreach (
			string relativePath
			in new[] {
				"README.md",
				"Icod.TermInfo.Source/README.md",
				"Icod.TermInfo.Compiler/README.md",
				"Icod.TermInfo.Inspection/README.md",
			}
		) {
			Assert.Contains(
				"1.4.0",
				ReadRepositoryFile( root, relativePath ),
				StringComparison.Ordinal
			);
		}
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
