using System.Globalization;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP02PlanningTests {
	[Fact]
	public void StandardPlanningMatchesIndependentZeroAndSingleParentOracle() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-standard-target" )
				.SetDescription( "RP02 standard target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "bell" )
				.Build();
		TerminalDescription exact =
			new TerminalDescriptionBuilder( "rp02-standard-base" )
				.AddAlias( "rp02-standard-base-alias" )
				.SetDescription( "RP02 standard exact base" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "bell" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp02-standard-near",
				new TerminalDescriptionBuilder( "rp02-standard-near" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			),
			CreateCandidate( "rp02-standard-base-alias", exact ),
			CreateCandidate(
				"rp02-standard-other",
				new TerminalDescriptionBuilder( "rp02-standard-other" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 100 )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateSingleParentOptions()
			);

		TerminalDescriptionSourceSynthesisParent selected =
			Assert.Single( plan.SelectedParents );
		Assert.Equal( "rp02-standard-base-alias", selected.UseName );
		Assert.Contains(
			"use=rp02-standard-base-alias,",
			plan.Source,
			StringComparison.Ordinal
		);
		Assert.Equal( 4, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void ExtendedPlanningMatchesIndependentZeroAndSingleParentOracle() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-extended-target" )
				.SetDescription( "RP02 extended target" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescription exact =
			new TerminalDescriptionBuilder( "rp02-extended-base" )
				.SetDescription( "RP02 extended base" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp02-extended-near",
				new TerminalDescriptionBuilder( "rp02-extended-near" )
					.SetExtendedNumber( "XNum", 7 )
					.SetExtendedString( "InheritedOnly", "cancel" )
					.Build()
			),
			CreateCandidate( exact.Name, exact ),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateSingleParentOptions()
			);

		Assert.Same( candidates[ 1 ], Assert.Single( plan.SelectedParents ) );
		Assert.Equal( 0, plan.Score.LocalDirectiveCount );
		Assert.Equal( 0, plan.Score.CancellationCount );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void BaselineWinsWhenSingleParentDoesNotImproveFrozenScore() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-baseline-target" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent candidate =
			CreateCandidate(
				"rp02-empty-parent",
				new TerminalDescriptionBuilder( "rp02-empty-parent" ).Build()
			);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				new[] { candidate }
			);

		Assert.Empty( plan.SelectedParents );
		Assert.Equal( 0, plan.Score.ParentCount );
		Assert.Equal( 2, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
		Assert.Equal(
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				Array.Empty<TerminalDescriptionSourceSynthesisParent>()
			),
			plan.Source
		);
	}

	[Fact]
	public void WinningSingleParentScoreCountsInheritedCancellation() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-cancellation-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent candidate =
			CreateCandidate(
				"rp02-cancellation-parent",
				new TerminalDescriptionBuilder( "rp02-cancellation-parent" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetNumber( NumericCapability.Lines, 24 )
					.Build()
			);

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				new[] { candidate },
				new TerminalDescriptionSourcePlanningOptions()
			);

		Assert.Same( candidate, Assert.Single( plan.SelectedParents ) );
		Assert.Equal( 1, plan.Score.LocalDirectiveCount );
		Assert.Equal( 1, plan.Score.CancellationCount );
		Assert.Contains( "lines@,", plan.Source, StringComparison.Ordinal );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void InvalidPlansAreRejectedWithoutApproximation() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-representation-target" )
				.SetExtendedString( "XRequired", "value" )
				.Build();
		TerminalDescriptionSourceSynthesisParent invalid =
			CreateCandidate(
				"rp02-invalid-parent",
				new TerminalDescriptionBuilder( "rp02-invalid-parent" ).Build()
			);
		TerminalDescriptionSourceSynthesisParent valid =
			CreateCandidate(
				"rp02-valid-parent",
				new TerminalDescriptionBuilder( "rp02-valid-parent" )
					.SetExtendedString( "XRequired", "value" )
					.Build()
			);
		TerminalDescriptionSourceSynthesisOptions synthesisOptions =
			new(
				80,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.Database,
				maximumParentCount: 1,
				includeExtendedCapabilities: false
			);
		TerminalDescriptionSourcePlanningOptions options =
			new(
				synthesisOptions,
				maximumCandidateCount: 2,
				maximumSelectedParentCount: 1
			);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				new[] { invalid, valid },
				options
			);

		Assert.Same( valid, Assert.Single( plan.SelectedParents ) );
		Assert.Equal( 3, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
		Assert.Equal( 0, plan.Score.LocalDirectiveCount );
		AssertRoundTrips( target, plan );

		TerminalDescriptionSourcePlanningOptions impossibleLength =
			new(
				new TerminalDescriptionSourceSynthesisOptions(),
				maximumCandidateCount: 2,
				maximumSelectedParentCount: 1,
				maximumGeneratedSourceLength: 1
			);
		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourcePlanner.Plan(
					target,
					new[] { invalid, valid },
					impossibleLength
				)
		);
	}

	[Fact]
	public void EvaluationBudgetHonorsExhaustiveAndOptInPartialPolicies() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-budget-target" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate( "rp02-budget-a" ),
			CreateCandidate( "rp02-budget-b" ),
			CreateCandidate( "rp02-budget-c" ),
		];
		TerminalDescriptionSourceSynthesisOptions synthesisOptions =
			new(
				80,
				maximumParentCount: 1
			);
		TerminalDescriptionSourcePlanningOptions exhaustive =
			new(
				synthesisOptions,
				maximumCandidateCount: 3,
				maximumSelectedParentCount: 1,
				maximumEvaluatedPlanCount: 2
			);

		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourcePlanner.Plan(
					target,
					candidates,
					exhaustive
				)
		);

		TerminalDescriptionSourcePlanningOptions partial =
			new(
				synthesisOptions,
				maximumCandidateCount: 3,
				maximumSelectedParentCount: 1,
				maximumEvaluatedPlanCount: 2,
				allowNonExhaustiveResult: true
			);
		TerminalDescriptionSourcePlan partialPlan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				partial
			);
		Assert.Equal( 2, partialPlan.EvaluatedPlanCount );
		Assert.False( partialPlan.IsExhaustive );
		Assert.Equal( 3, partialPlan.CandidateCount );

		TerminalDescriptionSourcePlanningOptions baselineOnly =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 0
				),
				maximumCandidateCount: 3,
				maximumSelectedParentCount: 0,
				maximumEvaluatedPlanCount: 1
			);
		TerminalDescriptionSourcePlan baselinePlan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				baselineOnly
			);
		Assert.Empty( baselinePlan.SelectedParents );
		Assert.Equal( 1, baselinePlan.EvaluatedPlanCount );
		Assert.True( baselinePlan.IsExhaustive );
		Assert.Equal( 3, baselinePlan.CandidateCount );
	}

	[Fact]
	public void MultiParentLegalSpaceIsRejectedUntilRp03() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-depth-target" ).Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate( "rp02-depth-a" ),
			CreateCandidate( "rp02-depth-b" ),
		];

		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() =>
					TerminalDescriptionSourcePlanner.Plan(
						target,
						candidates
					)
			);
		Assert.Contains( "RP03", exception.Message, StringComparison.Ordinal );

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				CreateSingleParentOptions()
			);
		Assert.Equal( 3, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
	}

	[Fact]
	public void PlanningIsDeterministicAcrossOrderCultureAndRepeatedCalls() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-deterministic-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString( StringCapability.Bell, "signal" )
				.SetExtendedNumber( "Index", 7 )
				.Build();
		TerminalDescription exact =
			new TerminalDescriptionBuilder( "rp02-deterministic-base" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString( StringCapability.Bell, "signal" )
				.SetExtendedNumber( "Index", 7 )
				.Build();
		TerminalDescriptionSourceSynthesisParent winner =
			CreateCandidate( exact.Name, exact );
		TerminalDescriptionSourceSynthesisParent farther =
			CreateCandidate(
				"rp02-deterministic-farther",
				new TerminalDescriptionBuilder( "rp02-deterministic-farther" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			);

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			TerminalDescriptionSourcePlan first =
				TerminalDescriptionSourcePlanner.Plan(
					target,
					new[] { farther, winner },
					CreateSingleParentOptions()
				);

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			TerminalDescriptionSourcePlan second =
				TerminalDescriptionSourcePlanner.Plan(
					target,
					new[] { winner, farther },
					CreateSingleParentOptions()
				);
			TerminalDescriptionSourcePlan repeated =
				TerminalDescriptionSourcePlanner.Plan(
					target,
					new[] { winner, farther },
					CreateSingleParentOptions()
				);

			Assert.Same( winner, Assert.Single( first.SelectedParents ) );
			Assert.Same( winner, Assert.Single( second.SelectedParents ) );
			Assert.Equal( first.Source, second.Source );
			Assert.Equal( second.Source, repeated.Source );
			Assert.Equal( second.Score, repeated.Score );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void EqualSingleParentScoresUseCallerCandidatePosition() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp02-tie-target" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription shared =
			new TerminalDescriptionBuilder( "rp02-tie-base" )
				.AddAlias( "rp02-tie-left" )
				.AddAlias( "rp02-tie-rght" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent left =
			CreateCandidate( "rp02-tie-left", shared );
		TerminalDescriptionSourceSynthesisParent right =
			CreateCandidate( "rp02-tie-rght", shared );

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				new[] { right, left },
				CreateSingleParentOptions()
			);

		Assert.Same( right, Assert.Single( plan.SelectedParents ) );
		Assert.Equal( new[] { 0 }, plan.Score.SelectedCandidateIndices );
	}

	[Fact]
	public void Rp02ImplementationRecordFreezesVersionScopeAndEvidence() {
		string implementation =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RP02-ZERO-AND-SINGLE-PARENT-PLANNING.md"
				)
			);

		Assert.Contains( "1.8.0-Alpha-2", implementation, StringComparison.Ordinal );
		Assert.Contains( "zero-parent baseline", implementation, StringComparison.Ordinal );
		Assert.Contains( "every legal single candidate", implementation, StringComparison.Ordinal );
		Assert.Contains( "independent oracle", implementation, StringComparison.Ordinal );
		Assert.Contains( "does not reparse", implementation, StringComparison.Ordinal );
	}

	private static TerminalDescriptionSourcePlan AssertMatchesIndependentOracle(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		OraclePlan expected =
			CreateIndependentOraclePlan(
				target,
				candidates,
				options
			);
		TerminalDescriptionSourcePlan actual =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				options
			);

		Assert.Equal( expected.Source, actual.Source );
		Assert.Equal( expected.Score, actual.Score );
		Assert.Equal( candidates.Count + 1, actual.EvaluatedPlanCount );
		Assert.True( actual.IsExhaustive );
		Assert.Equal( candidates.Count, actual.CandidateCount );
		if ( expected.Parent is null ) {
			Assert.Empty( actual.SelectedParents );
		} else {
			Assert.Same( expected.Parent, Assert.Single( actual.SelectedParents ) );
		}

		return actual;
	}

	private static OraclePlan CreateIndependentOraclePlan(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		OraclePlan? best =
			CreateOracleCandidate(
				target,
				parent: null,
				candidateIndex: null,
				options
			);
		if ( options.MaximumSelectedParentCount != 0 ) {
			for ( int candidateIndex = 0;
				candidateIndex < candidates.Count;
				candidateIndex++ ) {
				OraclePlan? current =
					CreateOracleCandidate(
						target,
						candidates[ candidateIndex ],
						candidateIndex,
						options
					);
				if ( current is not null
					&& ( best is null
						|| current.Score.CompareTo( best.Score ) < 0 ) ) {
					best = current;
				}
			}
		}

		return best
			?? throw new InvalidOperationException(
				"The independent oracle found no valid plan."
			);
	}

	private static OraclePlan? CreateOracleCandidate(
		TerminalDescription target,
		TerminalDescriptionSourceSynthesisParent? parent,
		int? candidateIndex,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( options );

		TerminalDescriptionSourceSynthesisParent[] parents =
			parent is null
				? []
				: [ parent ];
		string source;
		try {
			source =
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target,
					parents,
					options.SynthesisOptions
				);
		} catch ( InvalidOperationException ) {
			return null;
		}
		if ( source.Length > options.MaximumGeneratedSourceLength ) {
			return null;
		}

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"rp02-independent-oracle.ti"
			);
		Assert.False( parsed.HasErrors );
		TermInfoSourceEntry entry = Assert.Single( parsed.Document.Entries );
		int localDirectiveCount =
			entry.Fields.Count(
				field => field.Kind != TermInfoSourceFieldKind.UseReference
					&& field.Kind != TermInfoSourceFieldKind.DisabledCapability
			);
		int cancellationCount =
			entry.Fields.Count(
				field => field.Kind == TermInfoSourceFieldKind.CancelledCapability
			);
		int[] selectedCandidateIndices =
			candidateIndex.HasValue
				? [ candidateIndex.Value ]
				: [];
		TerminalDescriptionSourcePlanningScore score =
			new(
				localDirectiveCount,
				cancellationCount,
				parents.Length,
				Encoding.UTF8.GetByteCount( source ),
				selectedCandidateIndices
			);

		return new OraclePlan(
			parent,
			source,
			score
		);
	}

	private static void AssertRoundTrips(
		TerminalDescription target,
		TerminalDescriptionSourcePlan plan
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( plan );

		StringBuilder source =
			new( plan.Source );
		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in plan.SelectedParents
		) {
			source.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source.ToString(),
				"rp02-roundtrip.ti"
			);
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				target.Name
			);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				target,
				resolved.Entry!.ToTerminalDescription()
			);
		Assert.True( comparison.AreEqual );
	}

	private static TerminalDescriptionSourceSynthesisParent CreateCandidate(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return CreateCandidate(
			name,
			new TerminalDescriptionBuilder( name ).Build()
		);
	}

	private static TerminalDescriptionSourcePlanningOptions
		CreateSingleParentOptions() {
		return new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(),
			maximumSelectedParentCount: 1
		);
	}

	private static TerminalDescriptionSourceSynthesisParent CreateCandidate(
		string useName,
		TerminalDescription description
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( useName );
		ArgumentNullException.ThrowIfNull( description );

		return new TerminalDescriptionSourceSynthesisParent(
			useName,
			description
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);
		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
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
			"The repository root could not be located from the test output path."
		);
	}

	private sealed class OraclePlan {
		public OraclePlan(
			TerminalDescriptionSourceSynthesisParent? parent,
			string source,
			TerminalDescriptionSourcePlanningScore score
		) {
			ArgumentNullException.ThrowIfNull( source );
			ArgumentNullException.ThrowIfNull( score );

			Parent = parent;
			Source = source;
			Score = score;
		}

		public TerminalDescriptionSourceSynthesisParent? Parent {
			get;
		}

		public string Source {
			get;
		}

		public TerminalDescriptionSourcePlanningScore Score {
			get;
		}
	}
}
