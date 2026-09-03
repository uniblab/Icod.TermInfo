using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA07GeneratedStateHardeningTests {
	[Fact]
	public void GeneratedDatabaseSetIsCultureDeterministicAndPreservesSemanticEvidence() {
		using TemporaryDirectory first = new( "culture-first" );
		using TemporaryDirectory second = new( "culture-second" );

		CompiledTermInfoDatabaseWriter.Write(
			first.Path,
			[
				CreateTerminal( "shared", 80, "first-shared" ),
				CreateTerminal( "alias-owner-a", 90, "collision" ),
			]
		);
		CompiledTermInfoDatabaseWriter.Write(
			second.Path,
			[
				CreateTerminal( "shared", 132, "second-shared" ),
				CreateTerminal( "alias-owner-b", 100, "collision" ),
			]
		);

		TermInfoDatabaseSet set = TermInfoDatabaseInspector.InspectSet(
			[ first.Path, second.Path ]
		);
		TermInfoDatabaseSetSemanticAnalysis analysis = set.AnalyzeSemantics();

		Assert.True( set.IsComplete );
		Assert.Equal( 2, set.Entries.Count );
		Assert.Contains(
			analysis.RepeatedIdentities,
			identity => identity.Identity.Name == "shared"
				&& identity.Relationship
					== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		);
		Assert.Contains(
			analysis.Aliases,
			alias => alias.Alias == "collision"
				&& alias.HasMultipleCanonicalOwners
				&& alias.Relationship
					== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		);

		string baseline = TermInfoJsonRenderer.Render( set );
		Assert.Equal( baseline, RenderUnderCulture( set, "tr-TR" ) );
		Assert.Equal( baseline, RenderUnderCulture( set, "ar-SA" ) );
	}

	[Fact]
	public void MissingEarlierGeneratedRootKeepsLaterLookupIndeterminate() {
		using TemporaryDirectory later = new( "indeterminate-later" );
		CompiledTermInfoDatabaseWriter.Write(
			later.Path,
			CreateTerminal( "later-terminal", 80 )
		);
		string missing = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-da07-missing-{Guid.NewGuid():N}"
		);

		TermInfoDatabaseSet set = TermInfoDatabaseInspector.InspectSet(
			[ missing, later.Path ]
		);
		TermInfoDatabaseSetLookupResult lookup = set.LookupCanonicalName(
			"later-terminal"
		);

		Assert.False( set.IsComplete );
		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, lookup.Status );
		Assert.NotNull( lookup.Winner );
		Assert.Equal( 1, lookup.Winner!.DatabaseIndex );
	}

	[Fact]
	public void RepeatedPhysicalRootRemainsExplicitOrderedProvenance() {
		using TemporaryDirectory root = new( "repeated-root" );
		CompiledTermInfoDatabaseWriter.Write(
			root.Path,
			CreateTerminal( "repeated", 80 )
		);

		TermInfoDatabaseSet set = TermInfoDatabaseInspector.InspectSet(
			[ root.Path, root.Path ]
		);
		TermInfoDatabaseSetIdentity identity = Assert.Single(
			set.Identities,
			candidate => candidate.Name == "repeated"
		);

		Assert.Equal( 2, set.Entries.Count );
		Assert.Equal( root.Path, set.Entries[ 0 ].Catalog.Root );
		Assert.Equal( root.Path, set.Entries[ 1 ].Catalog.Root );
		Assert.Equal(
			new[] { 0, 1 },
			identity.Occurrences.Select( occurrence => occurrence.DatabaseIndex ).ToArray()
		);
	}

	[Fact]
	public void GeneratedLargeCandidateUniverseRemainsBoundedAndDeterministic() {
		using TemporaryDirectory root = new( "large-candidates" );
		TerminalDescription[] candidates = Enumerable.Range( 0, 32 )
			.Select(
				index => CreateTerminal(
					$"candidate-{index:D2}",
					40 + index
				)
			)
			.ToArray();
		CompiledTermInfoDatabaseWriter.Write( root.Path, candidates );

		TermInfoDatabaseSet set = TermInfoDatabaseInspector.InspectSet(
			[ root.Path ]
		);
		TerminalDescription target = new TerminalDescriptionBuilder( "target" )
			.SetDescription( "DA07 large-candidate target" )
			.SetNumber( NumericCapability.Columns, 71 )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.Build();
		TermInfoDatabaseSetSourcePlanningResult first =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );
		TermInfoDatabaseSetSourcePlanningResult second =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );

		Assert.Equal( first.Candidates.Count, second.Candidates.Count );
		Assert.Equal( first.Plan.Source, second.Plan.Source );
		Assert.Equal(
			first.Candidates.Select( candidate => candidate.CanonicalName ).ToArray(),
			second.Candidates.Select( candidate => candidate.CanonicalName ).ToArray()
		);
		Assert.InRange( first.Candidates.Count, 1, 32 );

		Assert.Throws<ArgumentException>(
			() => TermInfoDatabaseInspector.CreateSet(
				set.Entries.Select( entry => entry.Catalog ),
				new TermInfoDatabaseSetOptions(
					maximumDatabaseCount: 1,
					maximumTotalEntryCount: 1
				)
			)
		);
	}

	[Fact]
	public void Da07HardeningDoesNotAddPublicTypes() {
		Assert.Equal(
			51,
			typeof( TermInfoDatabaseSet ).Assembly.GetExportedTypes().Length
		);
	}

	private static string RenderUnderCulture(
		TermInfoDatabaseSet set,
		string cultureName
	) {
		ArgumentNullException.ThrowIfNull( set );
		ArgumentException.ThrowIfNullOrWhiteSpace( cultureName );

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo culture = CultureInfo.GetCultureInfo( cultureName );
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
			return TermInfoJsonRenderer.Render( set );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static TerminalDescription CreateTerminal(
		string name,
		int columns,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA07 {name}" )
			.SetNumber( NumericCapability.Columns, columns );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private sealed class TemporaryDirectory : IDisposable {
		public TemporaryDirectory(
			string suffix
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( suffix );
			Path = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				$"icod-terminfo-da07-{suffix}-{Guid.NewGuid():N}"
			);
			Directory.CreateDirectory( Path );
		}

		public string Path { get; }

		public void Dispose() {
			if ( Directory.Exists( Path ) ) {
				Directory.Delete( Path, recursive: true );
			}
		}
	}
}
