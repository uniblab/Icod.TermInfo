using System.Xml.Linq;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S09ContractTests {
	private const string DevelopmentVersion = "1.4.0-Alpha-5";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]	public void SourceAndRuntimePackagesAdvanceTogetherWithoutChangingAssemblyIdentity() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			}
		) {
			XDocument project =
				XDocument.Load(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
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
				StableAssemblyVersion,
				ReadRequiredProperty(
					project,
					"AssemblyVersion"
				)
			);
		}

		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			typeof( TermInfoSourceParser )
				.Assembly
				.GetName()
				.Version
		);
	}

	[Fact]
	public void DuplicateSourceIdentityDiagnosticCodesAreStable() {
		Assert.Equal(
			"TIS0025",
			TermInfoSourceDiagnosticCodes.DuplicateSourceEntryName
		);
		Assert.Equal(
			"TIS0026",
			TermInfoSourceDiagnosticCodes.DuplicateSourceAlias
		);
	}

	[Fact]
	public void SourcePublicApiBaselineIncludesS09DiagnosticCodes() {
		string root = FindRepositoryRoot();
		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.1.0-SOURCE-PUBLIC-API-BASELINE.txt"
				)
			);

		Assert.Contains(
			"FIELD public static const System.String DuplicateSourceEntryName null=not-null/not-null value=\"TIS0025\"",
			baseline
		);
		Assert.Contains(
			"FIELD public static const System.String DuplicateSourceAlias null=not-null/not-null value=\"TIS0026\"",
			baseline
		);
	}

	[Fact]
	public void S09ImplementationRecordAndRoadmapLinkArePresent() {
		string root = FindRepositoryRoot();
		string recordPath =
			Path.Combine(
				root,
				"docs",
				"1.1.0-S09-CORPUS-FUZZING-COMPATIBILITY.md"
			);
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-Post-1.0-Development-Roadmap.md"
				)
			);

		Assert.True( File.Exists( recordPath ) );
		string record =
			File.ReadAllText( recordPath );
		Assert.Contains( "1.1.0-Alpha-9", record );
		Assert.Contains( "TIS0025", record );
		Assert.Contains( "TIS0026", record );
		Assert.Contains( "deterministic", record );
		Assert.Contains( "`tic`", record );
		Assert.Contains(
			"1.1.0-S09-CORPUS-FUZZING-COMPATIBILITY.md",
			roadmap
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
					element.Name.LocalName
						== propertyName
			)
			.Value
			.Trim();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
				Path.Combine(
					current.FullName,
					"Icod.TermInfo.sln"
				)
			) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
