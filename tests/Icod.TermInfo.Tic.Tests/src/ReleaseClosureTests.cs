using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class ReleaseClosureTests {
	private const string StableReleaseVersion = "1.5.0";
	private const string DevelopmentVersion = "1.6.0-Alpha-2";
	private const string VersionReference = "$(IcodTermInfoSuiteVersion)";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void CoordinatedProjectsConsumeCentralDevelopmentVersion() {
		string root = FindRepositoryRoot();
		XDocument buildProperties =
			LoadProject( root, "Directory.Build.props" );
		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				buildProperties,
				"IcodTermInfoSuiteVersion"
			)
		);

		foreach (
			string relativePath
			in new string[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
				"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
				"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
				"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			}
		) {
			XDocument project = LoadProject( root, relativePath );
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "PackageVersion" )
			);
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty( project, "AssemblyVersion" )
			);
		}

		foreach (
			string relativePath
			in new string[] {
				"tic/Icod.TermInfo.Tic.csproj",
				"infocmp/Icod.TermInfo.InfoCmp.csproj",
				"toe/Icod.TermInfo.Toe.csproj",
			}
		) {
			XDocument project = LoadProject( root, relativePath );
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				"false",
				ReadRequiredProperty( project, "IsPackable" )
			);
		}

		XDocument router =
			LoadProject(
				root,
				"icod-terminfo/Icod.TermInfo.Router.csproj"
			);
		Assert.Equal(
			VersionReference,
			ReadRequiredProperty( router, "Version" )
		);
		Assert.Equal(
			VersionReference,
			ReadRequiredProperty( router, "PackageVersion" )
		);
		Assert.Equal(
			"true",
			ReadRequiredProperty( router, "PackAsTool" )
		);
		Assert.Equal(
			"Icod.TermInfo.Tools",
			ReadRequiredProperty( router, "PackageId" )
		);
		Assert.Equal(
			"icod-terminfo",
			ReadRequiredProperty( router, "ToolCommandName" )
		);
	}

	[Fact]
	public void MainAndTagWorkflowsSmokeBothToolDistributions() {
		string root = FindRepositoryRoot();
		string archiveSmoke = ReadRepositoryFile(
			root,
			".github/scripts/smoke-tool-archive.ps1"
		);
		string packageSmoke = ReadRepositoryFile(
			root,
			".github/scripts/smoke-tool-package.ps1"
		);

		foreach (
			string marker
			in new string[] {
				"tic",
				"infocmp",
				"toe",
				"release-smoke",
			}
		) {
			Assert.Contains(
				marker,
				archiveSmoke,
				StringComparison.Ordinal
			);
			Assert.Contains(
				marker,
				packageSmoke,
				StringComparison.Ordinal
			);
		}
		Assert.Contains(
			"Icod.TermInfo.Tools",
			packageSmoke,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"icod-terminfo",
			packageSmoke,
			StringComparison.Ordinal
		);

		foreach (
			string workflow
			in new string[] {
				".github/workflows/push-main.yaml",
				".github/workflows/release.yaml",
			}
		) {
			string workflowText =
				ReadRepositoryFile( root, workflow );
			Assert.Contains(
				"smoke-tool-archive.ps1",
				workflowText,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"smoke-tool-package.ps1",
				workflowText,
				StringComparison.Ordinal
			);
		}

		string release = ReadRepositoryFile(
			root,
			".github/workflows/release.yaml"
		);
		Assert.Contains(
			"needs: [metadata, validate, tool-archives, smoke-tool-archives, smoke-tool-package]",
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
					"1.5.0-RELEASE-AUDIT.md"
				)
			)
		);

		foreach (
			string relativePath
			in new string[] {
				"README.md",
				"Icod.TermInfo.Source/README.md",
				"Icod.TermInfo.Compiler/README.md",
				"Icod.TermInfo.Inspection/README.md",
				"icod-terminfo/README.md",
			}
		) {
			Assert.Contains(
				StableReleaseVersion,
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
