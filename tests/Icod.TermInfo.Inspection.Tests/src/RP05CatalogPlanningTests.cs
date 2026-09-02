using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP05CatalogPlanningTests {
	[Fact]
	public void ExplicitCatalogUsesCanonicalCandidatesInOrdinalOrder() {
		string root = CreateTemporaryDirectory();
		TerminalDescription target = CreateTarget();

		try {
			WriteControlledDatabase(
				root,
				target,
				reverseOrder: true
			);
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			TerminalDescriptionSourcePlan catalogPlan =
				TerminalDescriptionSourcePlanner.PlanFromCatalog(
					target,
					catalog
				);
			TerminalDescriptionSourcePlan directoryPlan =
				TerminalDescriptionSourcePlanner.PlanFromDirectory(
					target,
					root
				);

			Assert.Equal( 6, catalog.Entries.Count );
			Assert.Equal(
				new[] {
					"rp05-alpha",
					"rp05-beta",
				},
				catalogPlan.SelectedParents
					.Select( parent => parent.UseName )
					.ToArray()
			);
			Assert.Equal( new[] { 0, 1 }, catalogPlan.Score.SelectedCandidateIndices );
			Assert.Equal( 3, catalogPlan.CandidateCount );
			Assert.Equal( 10, catalogPlan.EvaluatedPlanCount );
			Assert.True( catalogPlan.IsExhaustive );
			Assert.Contains( "    use=rp05-alpha,\n", catalogPlan.Source, StringComparison.Ordinal );
			Assert.Contains( "    use=rp05-beta,\n", catalogPlan.Source, StringComparison.Ordinal );
			Assert.DoesNotContain( "use=rp05-alpha-alias", catalogPlan.Source, StringComparison.Ordinal );
			Assert.DoesNotContain( "use=rp05-self-reference", catalogPlan.Source, StringComparison.Ordinal );
			Assert.Equal( catalogPlan.Source, directoryPlan.Source );
			Assert.Equal(
				catalogPlan.Score.SelectedCandidateIndices.ToArray(),
				directoryPlan.Score.SelectedCandidateIndices.ToArray()
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void DirectoryPlanningIsIndependentOfPublicationOrder() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		TerminalDescription target = CreateTarget();

		try {
			WriteControlledDatabase(
				firstRoot,
				target,
				reverseOrder: false
			);
			WriteControlledDatabase(
				secondRoot,
				target,
				reverseOrder: true
			);

			TerminalDescriptionSourcePlan first =
				TerminalDescriptionSourcePlanner.PlanFromDirectory(
					target,
					firstRoot
				);
			TerminalDescriptionSourcePlan second =
				TerminalDescriptionSourcePlanner.PlanFromDirectory(
					target,
					secondRoot
				);

			Assert.Equal( first.Source, second.Source );
			Assert.Equal(
				first.SelectedParents.Select( parent => parent.UseName ).ToArray(),
				second.SelectedParents.Select( parent => parent.UseName ).ToArray()
			);
			Assert.Equal(
				first.Score.SelectedCandidateIndices.ToArray(),
				second.Score.SelectedCandidateIndices.ToArray()
			);
			Assert.Equal( first.CandidateCount, second.CandidateCount );
			Assert.Equal( first.EvaluatedPlanCount, second.EvaluatedPlanCount );
			Assert.Equal( first.IsExhaustive, second.IsExhaustive );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public void CatalogIssuesPreventFalseCompleteness() {
		string root = CreateTemporaryDirectory();
		TerminalDescription target = CreateTarget();
		TerminalDescription valid = CreateAlphaParent();

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				valid
			);
			WriteCandidate(
				root,
				"62",
				"broken-rp05",
				[ 0x1a, 0x01, 0x00 ]
			);
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			InvalidOperationException exception =
				Assert.Throws<InvalidOperationException>(
					() => TerminalDescriptionSourcePlanner.PlanFromCatalog(
						target,
						catalog
					)
				);

			Assert.True( catalog.HasIssues );
			Assert.Contains( "issue-free catalog", exception.Message, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void MissingRootIsNotTreatedAsAnEmptyCompleteCatalog() {
		string parent = CreateTemporaryDirectory();
		string root = Path.Combine( parent, "missing" );

		try {
			InvalidOperationException exception =
				Assert.Throws<InvalidOperationException>(
					() => TerminalDescriptionSourcePlanner.PlanFromDirectory(
						CreateTarget(),
						root
					)
				);

			Assert.Contains( "reported as Missing", exception.Message, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public void ConflictingDuplicateCanonicalEntriesAreRejected() {
		string root = CreateTemporaryDirectory();
		TerminalDescription first =
			new TerminalDescriptionBuilder( "rp05-conflict" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "rp05-conflict" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();

		try {
			WriteCandidate(
				root,
				"r",
				first.Name,
				CompiledTermInfoWriter.Write( first )
			);
			WriteCandidate(
				root,
				"72",
				second.Name,
				CompiledTermInfoWriter.Write( second )
			);
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			InvalidOperationException exception =
				Assert.Throws<InvalidOperationException>(
					() => TerminalDescriptionSourcePlanner.PlanFromCatalog(
						CreateTarget(),
						catalog
					)
				);

			Assert.Empty( catalog.Issues );
			Assert.Equal(
				new[] { first.Name },
				catalog.DuplicateCanonicalNames.ToArray()
			);
			Assert.Contains( "conflicting physical entries", exception.Message, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void DirectoryPlanningPreservesParserResourceLimits() {
		string root = CreateTemporaryDirectory();
		TerminalDescription candidate = CreateAlphaParent();

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				candidate
			);

			InvalidOperationException exception =
				Assert.Throws<InvalidOperationException>(
					() => TerminalDescriptionSourcePlanner.PlanFromDirectory(
						CreateTarget(),
						root,
						parserOptions: new CompiledTermInfoParserOptions( 16 )
					)
				);

			Assert.Contains( "issue-free catalog", exception.Message, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void CatalogPlanningPreservesCandidateBounds() {
		string root = CreateTemporaryDirectory();
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 2
				),
				maximumCandidateCount: 2,
				maximumSelectedParentCount: 2
			);

		try {
			foreach (
				TerminalDescription candidate
				in new[] {
					CreateAlphaParent(),
					CreateBetaParent(),
					CreateDecoyParent(),
				}
			) {
				CompiledTermInfoDatabaseWriter.Write(
					root,
					candidate
				);
			}
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			ArgumentException exception =
				Assert.Throws<ArgumentException>(
					() => TerminalDescriptionSourcePlanner.PlanFromCatalog(
						CreateTarget(),
						catalog,
						options
					)
				);

			Assert.Contains( "2 canonical non-self candidates", exception.Message, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void PreCanceledDirectoryPlanningDoesNotInspectTheRoot() {
		string root = CreateTemporaryDirectory();
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		try {
			Assert.Throws<OperationCanceledException>(
				() => TerminalDescriptionSourcePlanner.PlanFromDirectory(
					CreateTarget(),
					root,
					cancellationToken: cancellation.Token
				)
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void Rp05ImplementationRecordFreezesExplicitCatalogPolicy() {
		string implementation =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RP05-EXPLICIT-DATABASE-CATALOG-PLANNING.md"
				)
			);

		Assert.Contains( "1.8.0-Alpha-5", implementation, StringComparison.Ordinal );
		Assert.Contains( "canonical-name-only", implementation, StringComparison.Ordinal );
		Assert.Contains( "ordinal canonical-name order", implementation, StringComparison.Ordinal );
		Assert.Contains( "conflicting physical entries", implementation, StringComparison.Ordinal );
		Assert.Contains( "issue-free", implementation, StringComparison.Ordinal );
		Assert.Contains( "implicit host discovery", implementation, StringComparison.Ordinal );
	}

	private static void WriteControlledDatabase(
		string root,
		TerminalDescription target,
		bool reverseOrder
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( target );

		TerminalDescription[] entries =
			reverseOrder
				? [
					CreateDecoyParent(),
					CreateBetaParent(),
					CreateSelfReferenceCandidate( target.Aliases[ 0 ] ),
					CreateAlphaParent(),
				]
				: [
					CreateAlphaParent(),
					CreateSelfReferenceCandidate( target.Aliases[ 0 ] ),
					CreateBetaParent(),
					CreateDecoyParent(),
				]
		;

		foreach ( TerminalDescription entry in entries ) {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				entry
			);
		}
	}

	private static TerminalDescription CreateTarget() {
		return new TerminalDescriptionBuilder( "rp05-target" )
			.AddAlias( "rp05-target-alias" )
			.SetDescription( "RP05 target" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();
	}

	private static TerminalDescription CreateAlphaParent() {
		return new TerminalDescriptionBuilder( "rp05-alpha" )
			.AddAlias( "rp05-alpha-alias" )
			.SetDescription( "RP05 alpha" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.Build();
	}

	private static TerminalDescription CreateBetaParent() {
		return new TerminalDescriptionBuilder( "rp05-beta" )
			.SetDescription( "RP05 beta" )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();
	}

	private static TerminalDescription CreateDecoyParent() {
		return new TerminalDescriptionBuilder( "rp05-decoy" )
			.SetDescription( "RP05 decoy" )
			.SetBoolean( BooleanCapability.AutoLeftMargin )
			.Build();
	}

	private static TerminalDescription CreateSelfReferenceCandidate(
		string targetIdentity
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( targetIdentity );

		return new TerminalDescriptionBuilder( "rp05-self-reference" )
			.AddAlias( targetIdentity )
			.SetDescription( "RP05 obvious self-reference" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();
	}

	private static string WriteCandidate(
		string root,
		string directoryName,
		string fileName,
		byte[] data
	) {
		string directory =
			Path.Combine(
				root,
				directoryName
			);
		Directory.CreateDirectory( directory );
		string path =
			Path.Combine(
				directory,
				fileName
			);
		File.WriteAllBytes(
			path,
			data
		);
		return path;
	}

	private static string CreateTemporaryDirectory() {
		string path =
			Path.Combine(
				Path.GetTempPath(),
				$"icod-terminfo-rp05-{Guid.NewGuid():N}"
			);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		if ( !Directory.Exists( path ) ) {
			return;
		}

		try {
			Directory.Delete(
				path,
				recursive: true
			);
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory =
			new DirectoryInfo( AppContext.BaseDirectory );
		while ( directory is not null ) {
			if ( File.Exists(
				Path.Combine(
					directory.FullName,
					"Icod.TermInfo.sln"
				)
			) ) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"The repository root could not be located."
		);
	}
}
