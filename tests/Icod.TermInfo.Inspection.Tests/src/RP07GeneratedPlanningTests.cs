using System.Globalization;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP07GeneratedPlanningTests {
	private const int GeneratedCaseCount = 48;
	private const uint FirstGeneratedSeed = 0x18070001u;
	private const uint SeedStride = 0x9E3779B9u;

	private static readonly BooleanCapability[] GeneratedBooleans = [
		BooleanCapability.AutoRightMargin,
		BooleanCapability.BackColorErase,
		BooleanCapability.HasMetaKey,
	];

	private static readonly NumericCapability[] GeneratedNumbers = [
		NumericCapability.Columns,
		NumericCapability.Lines,
		NumericCapability.Colors,
	];

	private static readonly int[] GeneratedNumericValues = [
		0,
		1,
		80,
		256,
		32_768,
		int.MaxValue,
	];

	private static readonly StringCapability[] GeneratedStrings = [
		StringCapability.Bell,
		StringCapability.ClearScreen,
		StringCapability.CursorAddress,
	];

	private static readonly string[] GeneratedStringValues = [
		"bell\a",
		"comma,value",
		"backslash\\value",
		"\u001b[H\u001b[2J",
		"\u001b[%i%p1%d;%p2%dH",
	];

	[Fact]
	public void SeededGeneratedUniversesMatchIndependentBruteForceOracle() {
		TerminalDescriptionSourceLayout[] layouts =
			Enum.GetValues<TerminalDescriptionSourceLayout>();
		TerminalDescriptionSourceCapabilityOrder[] orders =
			Enum.GetValues<TerminalDescriptionSourceCapabilityOrder>();

		for ( int index = 0; index < GeneratedCaseCount; index++ ) {
			uint seed = unchecked(
				FirstGeneratedSeed
					+ ( (uint)index * SeedStride )
			);
			TerminalDescriptionSourceLayout layout =
				layouts[ index % layouts.Length ];
			TerminalDescriptionSourceCapabilityOrder order =
				orders[ ( index / layouts.Length ) % orders.Length ];
			Exception? failure = Record.Exception(
				() => VerifyGeneratedUniverse(
					seed,
					layout,
					order,
					verifyBudgetPrefix: index % 4 == 0
				)
			);

			Assert.True(
				failure is null,
				$"RP07 generated planning failed; reproducible seed=0x{seed:X8}; layout={layout}; order={order}{Environment.NewLine}{failure}"
			);
		}
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	public void ScoreTiesAdvanceThroughEveryFrozenComponent(
		int decidingComponent
	) {
		TerminalDescriptionSourcePlanningScore preferred;
		TerminalDescriptionSourcePlanningScore other;

		switch ( decidingComponent ) {
			case 0:
				preferred = CreateScore( 1, 1, 1, 10, 0 );
				other = CreateScore( 2, 0, 1, 1, 0 );
				break;
			case 1:
				preferred = CreateScore( 1, 0, 1, 10, 0 );
				other = CreateScore( 1, 1, 1, 1, 0 );
				break;
			case 2:
				preferred = CreateScore( 0, 0, 0, 10 );
				other = CreateScore( 0, 0, 1, 1, 0 );
				break;
			case 3:
				preferred = CreateScore( 0, 0, 1, 10, 0 );
				other = CreateScore( 0, 0, 1, 11, 0 );
				break;
			case 4:
				preferred = CreateScore( 0, 0, 1, 10, 0 );
				other = CreateScore( 0, 0, 1, 10, 1 );
				break;
			default:
				throw new ArgumentOutOfRangeException(
					nameof( decidingComponent )
				);
		}

		Assert.True( preferred.CompareTo( other ) < 0 );
		Assert.True( other.CompareTo( preferred ) > 0 );
	}

	[Fact]
	public void CandidatePermutationsAndEquivalentDescriptionsMatchOracle() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp07-permutation-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 120 )
				.SetExtendedString( "XMode", "target" )
				.Build();
		TerminalDescription shared =
			new TerminalDescriptionBuilder( "rp07-equivalent" )
				.AddAlias( "rp07-equivalent-a" )
				.AddAlias( "rp07-equivalent-b" )
				.SetDescription( "RP07 equivalent description" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			new( "rp07-equivalent-a", shared ),
			new(
				"rp07-columns",
				new TerminalDescriptionBuilder( "rp07-columns" )
					.SetNumber( NumericCapability.Columns, 120 )
					.Build()
			),
			new( "rp07-equivalent-b", shared ),
		];

		foreach ( int[] permutation in EnumeratePermutations( candidates.Length ) ) {
			TerminalDescriptionSourceSynthesisParent[] permuted =
				permutation.Select( index => candidates[ index ] ).ToArray();
			TerminalDescriptionSourcePlanningOptions options =
				CreateOptions(
					candidateCount: permuted.Length,
					maximumDepth: 2,
					maximumEvaluatedPlanCount: 10
				);

			TerminalDescriptionSourcePlan plan = AssertMatchesOracle(
				target,
				permuted,
				options,
				EnumerateIndexPlans( permuted.Length, 2 )
			);

			AssertRoundTrips( target, plan );
			Assert.Contains(
				plan.SelectedParents,
				parent => ReferenceEquals( parent.Description, shared )
			);
		}
	}

	[Fact]
	public void KindChangeAndInheritedCancellationParticipateInWinningScore() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp07-kind-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedNumber( "XKind", 7 )
				.Build();
		TerminalDescription parentDescription =
			new TerminalDescriptionBuilder( "rp07-kind-parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedString( "XKind", "parent" )
				.SetExtendedBoolean( "XCancelled" )
				.Build();
		TerminalDescriptionSourceSynthesisParent parent =
			new( parentDescription.Name, parentDescription );
		TerminalDescriptionSourcePlanningOptions options =
			CreateOptions(
				candidateCount: 1,
				maximumDepth: 1,
				maximumEvaluatedPlanCount: 2
			);

		TerminalDescriptionSourcePlan plan = AssertMatchesOracle(
			target,
			new[] { parent },
			options,
			EnumerateIndexPlans( candidateCount: 1, maximumDepth: 1 )
		);

		Assert.Same( parent, Assert.Single( plan.SelectedParents ) );
		Assert.Equal( 2, plan.Score.LocalDirectiveCount );
		Assert.Equal( 1, plan.Score.CancellationCount );
		Assert.Contains( "XKind#7,", plan.Source, StringComparison.Ordinal );
		Assert.Contains( "XCancelled@,", plan.Source, StringComparison.Ordinal );
		AssertRoundTrips( target, plan );
	}

	[Fact]
	public void CultureInsertionOrderAndRepeatedCallsAreByteStable() {
		TerminalDescription targetForward = CreateInsertionDescription(
			"rp07-determinism-target",
			reverse: false
		);
		TerminalDescription targetReverse = CreateInsertionDescription(
			"rp07-determinism-target",
			reverse: true
		);
		TerminalDescription parentForward = CreateInsertionDescription(
			"rp07-determinism-parent",
			reverse: false
		);
		TerminalDescription parentReverse = CreateInsertionDescription(
			"rp07-determinism-parent",
			reverse: true
		);
		TerminalDescriptionSourcePlanningOptions options =
			CreateOptions(
				candidateCount: 1,
				maximumDepth: 1,
				maximumEvaluatedPlanCount: 2
			);
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			TerminalDescriptionSourcePlan first =
				TerminalDescriptionSourcePlanner.Plan(
					targetForward,
					new[] {
						new TerminalDescriptionSourceSynthesisParent(
							parentForward.Name,
							parentForward
						),
					},
					options
				);

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			TerminalDescriptionSourcePlan second =
				TerminalDescriptionSourcePlanner.Plan(
					targetReverse,
					new[] {
						new TerminalDescriptionSourceSynthesisParent(
							parentReverse.Name,
							parentReverse
						),
					},
					options
				);
			TerminalDescriptionSourcePlan repeated =
				TerminalDescriptionSourcePlanner.Plan(
					targetReverse,
					new[] {
						new TerminalDescriptionSourceSynthesisParent(
							parentReverse.Name,
							parentReverse
						),
					},
					options
				);

			Assert.Equal( first.Source, second.Source );
			Assert.Equal( first.Source, repeated.Source );
			Assert.Equal( first.Score, second.Score );
			Assert.Equal( first.Score, repeated.Score );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void SupportedPlanningMaximaAndOnePastBoundariesAreFrozen() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp07-boundary-target" ).Build();
		TerminalDescriptionSourceSynthesisParent[] maximumCandidates =
			Enumerable.Range(
				0,
				TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount
			)
			.Select(
				index => {
					string name = $"rp07-boundary-{index:D3}";
					TerminalDescription description =
						new TerminalDescriptionBuilder( name ).Build();
					return new TerminalDescriptionSourceSynthesisParent(
						name,
						description
					);
				}
			)
			.ToArray();
		TerminalDescriptionSourcePlanningOptions snapshotOptions =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 0
				),
				maximumCandidateCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount,
				maximumSelectedParentCount: 0,
				maximumEvaluatedPlanCount: 1,
				maximumGeneratedSourceLength:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedGeneratedSourceLength
			);

		TerminalDescriptionSourcePlan boundaryPlan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				maximumCandidates,
				snapshotOptions
			);
		Assert.Equal( maximumCandidates.Length, boundaryPlan.CandidateCount );
		Assert.Equal( 1, boundaryPlan.EvaluatedPlanCount );
		Assert.True( boundaryPlan.IsExhaustive );

		TerminalDescriptionSourcePlanningOptions allMaximumOptions =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount:
						TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount
				),
				maximumCandidateCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount,
				maximumSelectedParentCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedSelectedParentCount,
				maximumEvaluatedPlanCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount,
				maximumGeneratedSourceLength:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedGeneratedSourceLength,
				allowNonExhaustiveResult: true
			);
		Assert.Equal(
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedSelectedParentCount,
			allMaximumOptions.MaximumSelectedParentCount
		);

		Assert.Throws<ArgumentException>(
			() => TerminalDescriptionSourcePlanner.Plan(
				target,
				maximumCandidates.Append(
					new TerminalDescriptionSourceSynthesisParent(
						"rp07-boundary-overflow",
						new TerminalDescriptionBuilder(
							"rp07-boundary-overflow"
						).Build()
					)
				),
				snapshotOptions
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourcePlanningOptions(
				new TerminalDescriptionSourceSynthesisOptions(),
				maximumCandidateCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount + 1
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourcePlanningOptions(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount:
						TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount
				),
				maximumCandidateCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount,
				maximumSelectedParentCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedSelectedParentCount + 1
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourcePlanningOptions(
				new TerminalDescriptionSourceSynthesisOptions(),
				maximumEvaluatedPlanCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount + 1
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourcePlanningOptions(
				new TerminalDescriptionSourceSynthesisOptions(),
				maximumGeneratedSourceLength:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedGeneratedSourceLength + 1
			)
		);
	}

	[Fact]
	public void ImplementationRecordAndDistributionGateFreezeRp07Evidence() {
		string root = FindRepositoryRoot();
		string implementation = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.8.0-RP07-GENERATED-STATE-ORACLE-AND-HARDENING.md"
			)
		);
		string sample = File.ReadAllText(
			Path.Combine(
				root,
				"samples",
				"Icod.TermInfo.Toolchain.Sample",
				"Program.cs"
			)
		);
		string bashGate = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-release-package.sh"
			)
		);
		string windowsGate = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-release-package.cmd"
			)
		);

		Assert.Contains( "1.8.0-Alpha-7", implementation, StringComparison.Ordinal );
		Assert.Contains( "independent brute-force oracle", implementation, StringComparison.Ordinal );
		Assert.Contains( "reproducible seed", implementation, StringComparison.Ordinal );
		Assert.Contains( "ncurses 6.5.20250216", implementation, StringComparison.Ordinal );
		Assert.Contains( "TerminalDescriptionSourcePlanner.Plan", sample, StringComparison.Ordinal );
		Assert.Contains( "CompiledTermInfoDatabaseWriter.Write", sample, StringComparison.Ordinal );
		Assert.Contains( "cmp -s", bashGate, StringComparison.Ordinal );
		Assert.Contains( "fc /b", windowsGate, StringComparison.OrdinalIgnoreCase );
	}

	private static void VerifyGeneratedUniverse(
		uint seed,
		TerminalDescriptionSourceLayout layout,
		TerminalDescriptionSourceCapabilityOrder order,
		bool verifyBudgetPrefix
	) {
		GeneratedPlanningCase generated = CreateGeneratedCase( seed );
		TerminalDescriptionSourcePlanningOptions exhaustiveOptions =
			CreateOptions(
				candidateCount: generated.Candidates.Count,
				maximumDepth: 2,
				maximumEvaluatedPlanCount: 10,
				layout: layout,
				order: order
			);
		IReadOnlyList<int[]> allPlans =
			EnumerateIndexPlans( generated.Candidates.Count, 2 );
		TerminalDescriptionSourcePlan exhaustive = AssertMatchesOracle(
			generated.Target,
			generated.Candidates,
			exhaustiveOptions,
			allPlans
		);
		AssertRoundTrips( generated.Target, exhaustive );

		if ( !verifyBudgetPrefix ) {
			return;
		}

		const int budget = 5;
		TerminalDescriptionSourcePlanningOptions boundedOptions =
			CreateOptions(
				candidateCount: generated.Candidates.Count,
				maximumDepth: 2,
				maximumEvaluatedPlanCount: budget,
				layout: layout,
				order: order,
				allowNonExhaustiveResult: true
			);
		TerminalDescriptionSourcePlan bounded = AssertMatchesOracle(
			generated.Target,
			generated.Candidates,
			boundedOptions,
			allPlans.Take( budget ).ToArray(),
			expectedExhaustive: false
		);
		AssertRoundTrips( generated.Target, bounded );
	}

	private static TerminalDescriptionSourcePlan AssertMatchesOracle(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options,
		IReadOnlyList<int[]> indexPlans,
		bool expectedExhaustive = true
	) {
		OraclePlan expected = CreateOraclePlan(
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
		Assert.Equal( expectedExhaustive, actual.IsExhaustive );
		Assert.Equal( candidates.Count, actual.CandidateCount );
		Assert.Equal( expected.Parents.Count, actual.SelectedParents.Count );
		for ( int index = 0; index < expected.Parents.Count; index++ ) {
			Assert.Same( expected.Parents[ index ], actual.SelectedParents[ index ] );
		}

		return actual;
	}

	private static OraclePlan CreateOraclePlan(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options,
		IReadOnlyList<int[]> indexPlans
	) {
		OraclePlan? best = null;
		foreach ( int[] indices in indexPlans ) {
			TerminalDescriptionSourceSynthesisParent[] parents =
				indices.Select( index => candidates[ index ] ).ToArray();
			OraclePlan? current = CreateOracleCandidate(
				target,
				parents,
				indices,
				options
			);
			if ( current is not null
				&& ( best is null || current.Score.CompareTo( best.Score ) < 0 ) ) {
				best = current;
			}
		}

		return best
			?? throw new InvalidOperationException(
				"The independent brute-force oracle found no valid plan."
			);
	}

	private static OraclePlan? CreateOracleCandidate(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		IReadOnlyList<int> candidateIndices,
		TerminalDescriptionSourcePlanningOptions options
	) {
		if ( parents.Select( parent => parent.UseName ).Distinct(
			StringComparer.Ordinal
		).Count() != parents.Count ) {
			return null;
		}

		string source;
		try {
			source = TerminalDescriptionSourceSynthesizer.Synthesize(
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

		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			source,
			"rp07-independent-oracle.ti"
		);
		Assert.False( parsed.HasErrors, FormatDiagnostics( parsed.Diagnostics ) );
		TermInfoSourceEntry entry = Assert.Single( parsed.Document.Entries );
		int localDirectiveCount = entry.Fields.Count(
			field => field.Kind != TermInfoSourceFieldKind.UseReference
				&& field.Kind != TermInfoSourceFieldKind.DisabledCapability
		);
		int cancellationCount = entry.Fields.Count(
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

		return new OraclePlan( parents, source, score );
	}

	private static IReadOnlyList<int[]> EnumerateIndexPlans(
		int candidateCount,
		int maximumDepth
	) {
		List<int[]> plans = [ [] ];
		if ( maximumDepth == 0 ) {
			return plans;
		}
		for ( int first = 0; first < candidateCount; first++ ) {
			plans.Add( [ first ] );
		}
		if ( maximumDepth == 1 ) {
			return plans;
		}
		for ( int first = 0; first < candidateCount; first++ ) {
			for ( int second = 0; second < candidateCount; second++ ) {
				if ( first != second ) {
					plans.Add( [ first, second ] );
				}
			}
		}
		return plans;
	}

	private static IEnumerable<int[]> EnumeratePermutations(
		int candidateCount
	) {
		for ( int first = 0; first < candidateCount; first++ ) {
			for ( int second = 0; second < candidateCount; second++ ) {
				if ( second == first ) {
					continue;
				}
				for ( int third = 0; third < candidateCount; third++ ) {
					if ( third != first && third != second ) {
						yield return [ first, second, third ];
					}
				}
			}
		}
	}

	private static GeneratedPlanningCase CreateGeneratedCase(
		uint seed
	) {
		DeterministicRandom random = new( seed );
		TerminalDescriptionBuilder targetBuilder =
			new TerminalDescriptionBuilder( $"rp07-{seed:X8}-target" )
				.SetDescription( $"RP07 generated target {seed:X8}" );
		ApplyGeneratedCapabilities( targetBuilder, ref random );
		switch ( random.Next( 3 ) ) {
			case 0:
				targetBuilder.SetExtendedBoolean( "XKind" );
				break;
			case 1:
				targetBuilder.SetExtendedNumber( "XKind", 7 );
				break;
			case 2:
				targetBuilder.SetExtendedString( "XKind", "target" );
				break;
		}

		List<TerminalDescriptionSourceSynthesisParent> candidates = [];
		for ( int index = 0; index < 3; index++ ) {
			string name = $"rp07-{seed:X8}-candidate-{index}";
			TerminalDescriptionBuilder candidateBuilder =
				new TerminalDescriptionBuilder( name )
					.SetDescription(
						$"RP07 generated candidate {index} for {seed:X8}"
					);
			ApplyGeneratedCapabilities( candidateBuilder, ref random );
			if ( index == 0 ) {
				candidateBuilder
					.SetExtendedString( "XKind", "candidate" )
					.SetExtendedBoolean( "XCancelled" );
			} else if ( index == 1 ) {
				candidateBuilder.SetExtendedNumber( "XKind", 9 );
			} else {
				candidateBuilder.SetExtendedBoolean( "XKind" );
			}
			TerminalDescription description = candidateBuilder.Build();
			candidates.Add(
				new TerminalDescriptionSourceSynthesisParent(
					description.Name,
					description
				)
			);
		}

		return new GeneratedPlanningCase(
			targetBuilder.Build(),
			candidates.ToArray()
		);
	}

	private static void ApplyGeneratedCapabilities(
		TerminalDescriptionBuilder builder,
		ref DeterministicRandom random
	) {
		foreach ( BooleanCapability capability in GeneratedBooleans ) {
			if ( random.Next( 2 ) == 1 ) {
				builder.SetBoolean( capability );
			}
		}
		foreach ( NumericCapability capability in GeneratedNumbers ) {
			if ( random.Next( 2 ) == 1 ) {
				builder.SetNumber(
					capability,
					GeneratedNumericValues[ random.Next( GeneratedNumericValues.Length ) ]
				);
			}
		}
		foreach ( StringCapability capability in GeneratedStrings ) {
			if ( random.Next( 2 ) == 1 ) {
				builder.SetString(
					capability,
					GeneratedStringValues[ random.Next( GeneratedStringValues.Length ) ]
				);
			}
		}

		if ( random.Next( 2 ) == 1 ) {
			builder.SetExtendedBoolean( "XFlag" );
		}
		if ( random.Next( 2 ) == 1 ) {
			builder.SetExtendedNumber(
				"XNumber",
				GeneratedNumericValues[ random.Next( GeneratedNumericValues.Length ) ]
			);
		}
		if ( random.Next( 2 ) == 1 ) {
			builder.SetExtendedString(
				"XString",
				GeneratedStringValues[ random.Next( GeneratedStringValues.Length ) ]
			);
		}
	}

	private static TerminalDescription CreateInsertionDescription(
		string name,
		bool reverse
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name )
				.SetDescription( "RP07 insertion-order description" );
		if ( reverse ) {
			builder
				.SetExtendedString( "XZulu", "z" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetExtendedNumber( "XAlpha", 7 )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedBoolean( "XFlag" )
				.SetBoolean( BooleanCapability.AutoRightMargin );
		} else {
			builder
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetExtendedBoolean( "XFlag" )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedNumber( "XAlpha", 7 )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetExtendedString( "XZulu", "z" );
		}
		return builder.Build();
	}

	private static void AssertRoundTrips(
		TerminalDescription target,
		TerminalDescriptionSourcePlan plan
	) {
		StringBuilder source = new( plan.Source );
		HashSet<string> rendered = new( StringComparer.Ordinal );
		foreach ( TerminalDescriptionSourceSynthesisParent parent in plan.SelectedParents ) {
			if ( rendered.Add( parent.Description.Name ) ) {
				source.Append(
					TerminalDescriptionSourceRenderer.Render(
						parent.Description
					)
				);
			}
		}

		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			source.ToString(),
			"rp07-generated-roundtrip.ti"
		);
		Assert.False( parsed.HasErrors, FormatDiagnostics( parsed.Diagnostics ) );
		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			parsed.Document,
			target.Name
		);
		Assert.False( resolved.HasErrors, FormatDiagnostics( resolved.Diagnostics ) );
		Assert.NotNull( resolved.Entry );
		TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
			target,
			resolved.Entry!.ToTerminalDescription()
		);
		Assert.True(
			comparison.AreEqual,
			string.Join(
				Environment.NewLine,
				comparison.Differences.Select(
					difference => difference.ToString()
				)
			)
		);
	}

	private static TerminalDescriptionSourcePlanningOptions CreateOptions(
		int candidateCount,
		int maximumDepth,
		int maximumEvaluatedPlanCount,
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical,
		TerminalDescriptionSourceCapabilityOrder order =
			TerminalDescriptionSourceCapabilityOrder.Database,
		bool allowNonExhaustiveResult = false
	) {
		return new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(
				lineWidth: 40,
				layout: layout,
				capabilityOrder: order,
				maximumParentCount: maximumDepth,
				includeExtendedCapabilities: true
			),
			maximumCandidateCount: candidateCount,
			maximumSelectedParentCount: maximumDepth,
			maximumEvaluatedPlanCount: maximumEvaluatedPlanCount,
			allowNonExhaustiveResult: allowNonExhaustiveResult
		);
	}

	private static TerminalDescriptionSourcePlanningScore CreateScore(
		int localDirectiveCount,
		int cancellationCount,
		int parentCount,
		int renderedUtf8ByteCount,
		params int[] selectedCandidateIndices
	) {
		return new TerminalDescriptionSourcePlanningScore(
			localDirectiveCount,
			cancellationCount,
			parentCount,
			renderedUtf8ByteCount,
			selectedCandidateIndices
		);
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		return string.Join(
			Environment.NewLine,
			diagnostics.Select( diagnostic => diagnostic.Message )
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private sealed record GeneratedPlanningCase(
		TerminalDescription Target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Candidates
	);

	private sealed record OraclePlan(
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Parents,
		string Source,
		TerminalDescriptionSourcePlanningScore Score
	);

	private struct DeterministicRandom {
		private uint _state;

		internal DeterministicRandom(
			uint seed
		) {
			_state = seed == 0
				? 0xA341316Cu
				: seed
			;
		}

		internal int Next(
			int exclusiveMaximum
		) {
			if ( exclusiveMaximum <= 0 ) {
				throw new ArgumentOutOfRangeException(
					nameof( exclusiveMaximum )
				);
			}
			return (int)( NextUInt32() % (uint)exclusiveMaximum );
		}

		private uint NextUInt32() {
			uint value = _state;
			value ^= value << 13;
			value ^= value >> 17;
			value ^= value << 5;
			_state = value;
			return value;
		}
	}
}
