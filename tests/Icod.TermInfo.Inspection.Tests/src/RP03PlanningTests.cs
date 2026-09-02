using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP03PlanningTests {
	[Fact]
	public void OrderedCollisionSelectsLeftmostWinningParentOrder() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp03-collision-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetString( StringCapability.Bell, "signal" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp03-collision-left",
				new TerminalDescriptionBuilder( "rp03-collision-left" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 100 )
					.Build()
			),
			CreateCandidate(
				"rp03-collision-right",
				new TerminalDescriptionBuilder( "rp03-collision-right" )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.Bell, "signal" )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateOptions( candidateCount: 2, maximumDepth: 2 )
			);

		Assert.Equal( new[] { 0, 1 }, plan.Score.SelectedCandidateIndices );
		Assert.Same( candidates[ 0 ], plan.SelectedParents[ 0 ] );
		Assert.Same( candidates[ 1 ], plan.SelectedParents[ 1 ] );
		int leftReferenceIndex =
			plan.Source.IndexOf(
				"use=rp03-collision-left,",
				StringComparison.Ordinal
			);
		int rightReferenceIndex =
			plan.Source.IndexOf(
				"use=rp03-collision-right,",
				StringComparison.Ordinal
			);
		Assert.True( leftReferenceIndex >= 0 );
		Assert.True( rightReferenceIndex > leftReferenceIndex );
		Assert.Equal( 5, plan.EvaluatedPlanCount );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void HigherConfiguredDepthCanWin() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp03-depth-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString( StringCapability.Bell, "depth" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp03-depth-a",
				new TerminalDescriptionBuilder( "rp03-depth-a" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build()
			),
			CreateCandidate(
				"rp03-depth-b",
				new TerminalDescriptionBuilder( "rp03-depth-b" )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build()
			),
			CreateCandidate(
				"rp03-depth-c",
				new TerminalDescriptionBuilder( "rp03-depth-c" )
					.SetString( StringCapability.Bell, "depth" )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateOptions( candidateCount: 3, maximumDepth: 3 )
			);

		Assert.Equal( new[] { 0, 1, 2 }, plan.Score.SelectedCandidateIndices );
		Assert.Equal( 0, plan.Score.LocalDirectiveCount );
		Assert.Equal( 16, plan.EvaluatedPlanCount );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void ExtendedOverridesAdditionsAndCancellationsMatchOracle() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp03-extended-target" )
				.SetExtendedString( "XMode", "left" )
				.SetExtendedBoolean( "XNeeded" )
				.SetExtendedNumber( "XNumber", 42 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp03-extended-left",
				new TerminalDescriptionBuilder( "rp03-extended-left" )
					.SetExtendedString( "XMode", "left" )
					.Build()
			),
			CreateCandidate(
				"rp03-extended-right",
				new TerminalDescriptionBuilder( "rp03-extended-right" )
					.SetExtendedString( "XMode", "right" )
					.SetExtendedBoolean( "XNeeded" )
					.SetExtendedNumber( "XNumber", 42 )
					.SetExtendedBoolean( "XOrphan" )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateOptions( candidateCount: 2, maximumDepth: 2 )
			);

		Assert.Equal( new[] { 0, 1 }, plan.Score.SelectedCandidateIndices );
		Assert.Equal( 1, plan.Score.LocalDirectiveCount );
		Assert.Equal( 1, plan.Score.CancellationCount );
		Assert.Contains( "XOrphan@,", plan.Source, StringComparison.Ordinal );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void EqualDescriptionsAndAliasesRemainDistinctCandidatePositions() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp03-alias-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription shared =
			new TerminalDescriptionBuilder( "rp03-alias-base" )
				.AddAlias( "rp03-alias-one" )
				.AddAlias( "rp03-alias-two" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate( "rp03-alias-one", shared ),
			CreateCandidate( "rp03-alias-two", shared ),
			CreateCandidate(
				"rp03-alias-cols",
				new TerminalDescriptionBuilder( "rp03-alias-cols" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			AssertMatchesIndependentOracle(
				target,
				candidates,
				CreateOptions( candidateCount: 3, maximumDepth: 2 )
			);

		Assert.Equal( new[] { 0, 2 }, plan.Score.SelectedCandidateIndices );
		Assert.Equal( "rp03-alias-one", plan.SelectedParents[ 0 ].UseName );
		Assert.Equal( 10, plan.EvaluatedPlanCount );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void BudgetLimitedEnumerationStopsAtFirstLexicographicPair() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp03-budget-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp03-budget-a",
				new TerminalDescriptionBuilder( "rp03-budget-a" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build()
			),
			CreateCandidate(
				"rp03-budget-b",
				new TerminalDescriptionBuilder( "rp03-budget-b" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			),
			CreateCandidate( "rp03-budget-c" ),
		];
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 2
				),
				maximumCandidateCount: 3,
				maximumSelectedParentCount: 2,
				maximumEvaluatedPlanCount: 5,
				allowNonExhaustiveResult: true
			);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				options
			);

		Assert.Equal( new[] { 0, 1 }, plan.Score.SelectedCandidateIndices );
		Assert.Equal( 5, plan.EvaluatedPlanCount );
		Assert.False( plan.IsExhaustive );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void GeneratedSmallSpacesMatchIndependentOracleExactly() {
		for ( int scenario = 0; scenario < 6; scenario++ ) {
			TerminalDescription target = CreateGeneratedDescription(
				$"rp03-generated-target-{scenario}",
				mask: 7
			);
			TerminalDescriptionSourceSynthesisParent[] candidates = [
				CreateCandidate(
					$"rp03-generated-a-{scenario}",
					CreateGeneratedDescription(
						$"rp03-generated-a-{scenario}",
						1 | ( scenario & 2 )
					)
				),
				CreateCandidate(
					$"rp03-generated-b-{scenario}",
					CreateGeneratedDescription(
						$"rp03-generated-b-{scenario}",
						2 | ( scenario & 4 )
					)
				),
				CreateCandidate(
					$"rp03-generated-c-{scenario}",
					CreateGeneratedDescription(
						$"rp03-generated-c-{scenario}",
						4 | ( scenario & 1 )
					)
				),
			];

			TerminalDescriptionSourcePlan plan =
				AssertMatchesIndependentOracle(
					target,
					candidates,
					CreateOptions( candidateCount: 3, maximumDepth: 3 )
				);

			Assert.Equal( 16, plan.EvaluatedPlanCount );
			AssertRoundTrips( target, plan );
		}
	}

	[Fact]
	public void Rp03ImplementationRecordFreezesVersionScopeAndOracle() {
		string implementation =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RP03-ORDERED-MULTI-PARENT-PLANNING.md"
				)
			);

		Assert.Contains( "1.8.0-Alpha-3", implementation, StringComparison.Ordinal );
		Assert.Contains( "ordered permutations", implementation, StringComparison.Ordinal );
		Assert.Contains( "independent oracle", implementation, StringComparison.Ordinal );
		Assert.Contains( "leftmost", implementation, StringComparison.Ordinal );
		Assert.Contains( "distinct candidate positions", implementation, StringComparison.Ordinal );
	}

	private static TerminalDescriptionSourcePlan AssertMatchesIndependentOracle(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		IReadOnlyList<int[]> indexPlans =
			EnumerateIndexPlans(
				candidates.Count,
				options.MaximumSelectedParentCount
			);
		OraclePlan expected =
			CreateIndependentOraclePlan(
				target,
				candidates,
				options,
				indexPlans
			);
		TerminalDescriptionSourcePlan actual =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				options
			);

		Assert.Equal( expected.Source, actual.Source );
		Assert.Equal( expected.Score, actual.Score );
		Assert.Equal( indexPlans.Count, actual.EvaluatedPlanCount );
		Assert.True( actual.IsExhaustive );
		Assert.Equal( candidates.Count, actual.CandidateCount );
		Assert.Equal( expected.Parents.Count, actual.SelectedParents.Count );
		for ( int index = 0; index < expected.Parents.Count; index++ ) {
			Assert.Same( expected.Parents[ index ], actual.SelectedParents[ index ] );
		}

		return actual;
	}

	private static IReadOnlyList<int[]> EnumerateIndexPlans(
		int candidateCount,
		int maximumDepth
	) {
		List<int[]> plans = [ [] ];
		List<int[]> previousDepth = [ [] ];
		for ( int depth = 1;
			depth <= Math.Min( candidateCount, maximumDepth );
			depth++ ) {
			List<int[]> currentDepth = [];
			foreach ( int[] prefix in previousDepth ) {
				for ( int candidateIndex = 0;
					candidateIndex < candidateCount;
					candidateIndex++ ) {
					if ( prefix.Contains( candidateIndex ) ) {
						continue;
					}

					int[] plan = [ .. prefix, candidateIndex ];
					currentDepth.Add( plan );
					plans.Add( plan );
				}
			}
			previousDepth = currentDepth;
		}

		return plans;
	}

	private static OraclePlan CreateIndependentOraclePlan(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options,
		IReadOnlyList<int[]> indexPlans
	) {
		OraclePlan? best = null;
		foreach ( int[] indices in indexPlans ) {
			TerminalDescriptionSourceSynthesisParent[] parents =
				indices.Select( index => candidates[ index ] ).ToArray();
			OraclePlan? current =
				CreateOracleCandidate(
					target,
					parents,
					indices,
					options
				);
			if ( current is not null
				&& ( best is null
					|| current.Score.CompareTo( best.Score ) < 0 ) ) {
				best = current;
			}
		}

		return best
			?? throw new InvalidOperationException(
				"The independent oracle found no valid plan."
			);
	}

	private static OraclePlan? CreateOracleCandidate(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		IReadOnlyList<int> candidateIndices,
		TerminalDescriptionSourcePlanningOptions options
	) {
		string source;
		try {
			source =
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target,
					parents,
					options.SynthesisOptions
				);
		} catch ( ArgumentException ) {
			return null;
		} catch ( InvalidOperationException ) {
			return null;
		}
		if ( source.Length > options.MaximumGeneratedSourceLength ) {
			return null;
		}

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"rp03-independent-oracle.ti"
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
		TerminalDescriptionSourcePlanningScore score =
			new(
				localDirectiveCount,
				cancellationCount,
				parents.Count,
				Encoding.UTF8.GetByteCount( source ),
				candidateIndices
			);

		return new OraclePlan(
			parents,
			source,
			score
		);
	}

	private static void AssertRoundTrips(
		TerminalDescription target,
		TerminalDescriptionSourcePlan plan
	) {
		StringBuilder source = new( plan.Source );
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
				"rp03-roundtrip.ti"
			);
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				target.Name
			);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );
		Assert.True(
			TerminalDescriptionComparer.Compare(
				target,
				resolved.Entry!.ToTerminalDescription()
			).AreEqual
		);
	}

	private static TerminalDescription CreateGeneratedDescription(
		string name,
		int mask
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name );
		if ( ( mask & 1 ) != 0 ) {
			builder.SetBoolean( BooleanCapability.AutoRightMargin );
		}
		if ( ( mask & 2 ) != 0 ) {
			builder.SetNumber( NumericCapability.Columns, 80 );
		}
		if ( ( mask & 4 ) != 0 ) {
			builder.SetString( StringCapability.Bell, "generated" );
		}
		return builder.Build();
	}

	private static TerminalDescriptionSourcePlanningOptions CreateOptions(
		int candidateCount,
		int maximumDepth
	) {
		return new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(
				80,
				maximumParentCount: maximumDepth
			),
			maximumCandidateCount: candidateCount,
			maximumSelectedParentCount: maximumDepth,
			maximumEvaluatedPlanCount: 1_000
		);
	}

	private static TerminalDescriptionSourceSynthesisParent CreateCandidate(
		string name
	) {
		return CreateCandidate(
			name,
			new TerminalDescriptionBuilder( name ).Build()
		);
	}

	private static TerminalDescriptionSourceSynthesisParent CreateCandidate(
		string useName,
		TerminalDescription description
	) {
		return new TerminalDescriptionSourceSynthesisParent(
			useName,
			description
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
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
			IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
			string source,
			TerminalDescriptionSourcePlanningScore score
		) {
			Parents = Array.AsReadOnly( parents.ToArray() );
			Source = source;
			Score = score;
		}

		public IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Parents {
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
