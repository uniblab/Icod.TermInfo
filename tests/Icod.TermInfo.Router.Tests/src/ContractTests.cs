using System.Xml.Linq;
using Path = global::System.IO.Path;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class ContractTests {
	private const string VersionReference =
		"$(IcodTermInfoSuiteVersion)";

	[Fact]
	public void CoordinatedProjectsConsumeCentralVersionAuthority() {
		string root =
			FindRepositoryRoot();
		XDocument buildProperties =
			LoadProject(
				root,
				"Directory.Build.props"
			);
		Assert.Equal(
			"1.8.0-Alpha-8",
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
			XDocument project =
				LoadProject(
					root,
					relativePath
				);
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "PackageVersion" )
			);
			Assert.Equal(
				"1.0.0.0",
				ReadRequiredProperty( project, "AssemblyVersion" )
			);
		}

		foreach (
			string relativePath
			in new string[] {
				"tic/Icod.TermInfo.Tic.csproj",
				"infocmp/Icod.TermInfo.InfoCmp.csproj",
				"toe/Icod.TermInfo.Toe.csproj",
				"captoinfo/Icod.TermInfo.CapToInfo.csproj",
				"infotocap/Icod.TermInfo.InfoToCap.csproj",
			}
		) {
			XDocument project =
				LoadProject(
					root,
					relativePath
				);
			Assert.Equal(
				VersionReference,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				"false",
				ReadRequiredProperty( project, "IsPackable" )
			);
			Assert.Equal(
				"false",
				ReadRequiredProperty( project, "UseAppHost" )
			);
		}
	}

	[Fact]
	public void RouterIsPackableDistributionOnlyFanIn() {
		string root =
			FindRepositoryRoot();
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

		XElement[] projectReferences =
			router
				.Descendants()
				.Where(
					element =>
						element.Name.LocalName == "ProjectReference"
				)
				.ToArray();
		Assert.All(
			projectReferences,
			element =>
				Assert.Null(
					element.Attribute( "AdditionalProperties" )
				)
		);
		string[] references =
			projectReferences
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
		Assert.Equal(
			new string[] {
				@"..\captoinfo\Icod.TermInfo.CapToInfo.csproj",
				@"..\infocmp\Icod.TermInfo.InfoCmp.csproj",
				@"..\infotocap\Icod.TermInfo.InfoToCap.csproj",
				@"..\tic\Icod.TermInfo.Tic.csproj",
				@"..\toe\Icod.TermInfo.Toe.csproj",
			},
			references
		);
	}

	[Fact]
	public void RouterPackageHasStructuralAndHostNeutralityGates() {
		string root =
			FindRepositoryRoot();
		string shellVerifier =
			File.ReadAllText(
				System.IO.Path.Combine(
					root,
					".github",
					"scripts",
					"verify-release-package.sh"
				)
			);
		string commandVerifier =
			System.IO.File.ReadAllText(
				System.IO.Path.Combine(
					root,
					".github",
					"scripts",
					"verify-release-package.cmd"
				)
			);
		string archiveBuilder =
			System.IO.File.ReadAllText(
				System.IO.Path.Combine(
					root,
					".github",
					"scripts",
					"build-tool-archives.sh"
				)
			);

		Assert.Contains( "tool-package-verifier", shellVerifier );
		Assert.Contains( "tool-package-verifier", commandVerifier );
		Assert.Contains( "-p:UseAppHost=true", archiveBuilder );
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

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
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
