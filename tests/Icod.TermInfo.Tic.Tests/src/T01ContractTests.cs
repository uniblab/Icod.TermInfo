using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T01ContractTests {
	private const string DevelopmentVersion = "1.4.0-Alpha-5";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void CoordinatedLibrariesAdvanceWithoutChangingAssemblyIdentity() {
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
				DevelopmentVersion,
				ReadRequiredProperty( project, "Version" )
			);
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty( project, "PackageVersion" )
			);
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty( project, "AssemblyVersion" )
			);
			Assert.Equal(
				"net8.0;net9.0;net10.0",
				ReadRequiredProperty( project, "TargetFrameworks" )
			);
			Assert.DoesNotContain(
				project.Descendants(),
				element =>
					element.Name.LocalName == "PackageReference"
					&& string.Equals(
						element.Attribute( "Include" )?.Value,
						"Icod.CommandFramework",
						StringComparison.Ordinal
					)
			);
		}
	}

	[Fact]
	public void CommandProjectsAreNetTenAndHaveNoCommandToCommandDependencies() {
		string root = FindRepositoryRoot();

		foreach (
			(string relativePath, string assemblyName) in new[] {
				("tic/Icod.TermInfo.Tic.csproj", "tic"),
				("infocmp/Icod.TermInfo.InfoCmp.csproj", "infocmp"),
				("toe/Icod.TermInfo.Toe.csproj", "toe"),
			}
		) {
			XDocument project = LoadProject( root, relativePath );
			Assert.Equal( "net10.0", ReadRequiredProperty( project, "TargetFramework" ) );
			Assert.Equal( DevelopmentVersion, ReadRequiredProperty( project, "Version" ) );
			Assert.Equal( assemblyName, ReadRequiredProperty( project, "AssemblyName" ) );

			XElement frameworkReference = Assert.Single(
				project.Descendants(),
				element =>
					element.Name.LocalName == "PackageReference"
					&& string.Equals(
						element.Attribute( "Include" )?.Value,
						"Icod.CommandFramework",
						StringComparison.Ordinal
					)
			);
			Assert.Equal( "2.0.0", frameworkReference.Attribute( "Version" )?.Value );

			foreach (
				XElement reference
				in project.Descendants().Where(
					element => element.Name.LocalName == "ProjectReference"
				)
			) {
				string include = reference.Attribute( "Include" )?.Value ?? string.Empty;
				Assert.DoesNotContain( "Icod.TermInfo.Tic.csproj", include, StringComparison.Ordinal );
				Assert.DoesNotContain( "Icod.TermInfo.InfoCmp.csproj", include, StringComparison.Ordinal );
				Assert.DoesNotContain( "Icod.TermInfo.Toe.csproj", include, StringComparison.Ordinal );
			}
		}
	}

	[Fact]
	public void OneFourInspectionBaselineRetainsFrozenOneThreeSurface() {
		string root = FindRepositoryRoot();
		string oneThree = File.ReadAllText(
			System.IO.Path.Combine( root, "docs", "1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt" )
		);
		string oneFour = File.ReadAllText(
			System.IO.Path.Combine( root, "docs", "1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt" )
		);

		string withoutT02 = NormalizeLineEndings( oneFour );
		foreach (
			string typeHeader
			in new[] {
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseCatalog [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseCatalogEntry [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseCatalogIssue [sealed]",
				"TYPE enum Icod.TermInfo.Inspection.TermInfoDatabaseCatalogIssueKind [sealed]",
				"TYPE enum Icod.TermInfo.Inspection.TermInfoDatabaseCatalogKind [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseInspector [static]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseLocation [sealed]",
				"TYPE enum Icod.TermInfo.Inspection.TermInfoDatabaseLocationKind [sealed]",
			}
		) {
			withoutT02 = RemoveTypeBlock( withoutT02, typeHeader );
		}

		Assert.Equal(
			NormalizeLineEndings( oneThree ),
			withoutT02
		);
	}

	[Fact]
	public void SolutionContainsAllThreeCommandsAndTests() {
		string root = FindRepositoryRoot();
		string solution = System.IO.File.ReadAllText( System.IO.Path.Combine( root, "Icod.TermInfo.sln" ) );

		foreach (
			string projectName
			in new[] {
				"Icod.TermInfo.Tic",
				"Icod.TermInfo.Tic.Tests",
				"Icod.TermInfo.InfoCmp",
				"Icod.TermInfo.InfoCmp.Tests",
				"Icod.TermInfo.Toe",
				"Icod.TermInfo.Toe.Tests",
			}
		) {
			Assert.Contains( projectName, solution );
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
				element => element.Name.LocalName == propertyName
			)
			.Value
			.Trim();
	}

	private static string RemoveTypeBlock(
		string baseline,
		string typeHeader
	) {
		ArgumentNullException.ThrowIfNull( baseline );
		ArgumentException.ThrowIfNullOrWhiteSpace( typeHeader );

		int start = baseline.IndexOf( typeHeader, StringComparison.Ordinal );
		Assert.True( start >= 0 );

		const string terminator = "\nEND\n\n";
		int terminatorStart = baseline.IndexOf(
			terminator,
			start,
			StringComparison.Ordinal
		);
		Assert.True( terminatorStart >= 0 );

		return baseline.Remove(
			start,
			terminatorStart + terminator.Length - start
		);
	}

	private static string NormalizeLineEndings( string value ) {
		ArgumentNullException.ThrowIfNull( value );

		return value
			.Replace( "\r\n", "\n" )
			.Replace( '\r', '\n' );
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( System.IO.File.Exists( System.IO.Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}

			current = current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
