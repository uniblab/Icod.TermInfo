using System.Collections;
using System.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP04BoundedPlanningTests {
	[Fact]
	public void CombinatorialOverflowIsRejectedBeforeSourceEvaluation() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp04-overflow-target" )
				.AddAlias( "rp04-overflow-alias" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates =
			CreateCandidates(
				TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount,
				"rp04-overflow"
			);
		TerminalDescriptionSourcePlanningOptions options =
			CreateOptions(
				candidateCount: candidates.Length,
				maximumDepth: candidates.Length,
				maximumEvaluatedPlanCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount
			);

		TerminalDescriptionSourcePlanner.Plan(
			new TerminalDescriptionBuilder( "rp04-allocation-warmup" ).Build(),
			Array.Empty<TerminalDescriptionSourceSynthesisParent>()
		);
		long allocationStart = GC.GetAllocatedBytesForCurrentThread();
		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() =>
					TerminalDescriptionSourcePlanner.Plan(
						target,
						candidates,
						options
					)
			);
		long allocatedBytes =
			GC.GetAllocatedBytesForCurrentThread() - allocationStart;

		Assert.Contains(
			"Exhaustive ordered planning requires more than",
			exception.Message,
			StringComparison.Ordinal
		);
		Assert.True(
			allocatedBytes < 4 * 1024 * 1024,
			$"Plan-space preflight allocated {allocatedBytes} bytes."
		);
	}

	[Fact]
	public void MaximumCandidateBoundaryStopsAtFirstOverflowPosition() {
		TerminalDescriptionSourceSynthesisParent[] values =
			CreateCandidates(
				TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount + 1,
				"rp04-candidate-bound"
			);
		CountingEnumerable<TerminalDescriptionSourceSynthesisParent> candidates =
			new( values );
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 0
				),
				maximumCandidateCount:
					TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount,
				maximumSelectedParentCount: 0
			);

		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourcePlanner.Plan(
					new TerminalDescriptionBuilder( "rp04-candidate-target" ).Build(),
					candidates,
					options
				)
		);
		Assert.Equal(
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount + 1,
			candidates.YieldCount
		);
	}

	[Fact]
	public void HugeSpaceReturnsExactDeterministicBudgetPrefix() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp04-budget-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates =
			CreateCandidates( 64, "rp04-budget" );
		candidates[ 0 ] =
			CreateCandidate(
				"rp04-budget-000",
				new TerminalDescriptionBuilder( "rp04-budget-000" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build()
			);
		candidates[ 1 ] =
			CreateCandidate(
				"rp04-budget-001",
				new TerminalDescriptionBuilder( "rp04-budget-001" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			);
		TerminalDescriptionSourcePlanningOptions options =
			CreateOptions(
				candidateCount: 64,
				maximumDepth: 3,
				maximumEvaluatedPlanCount: 66,
				allowNonExhaustiveResult: true
			);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				options
			);

		Assert.Equal( 66, plan.EvaluatedPlanCount );
		Assert.False( plan.IsExhaustive );
		Assert.Equal( 64, plan.CandidateCount );
		Assert.Equal( new[] { 0, 1 }, plan.Score.SelectedCandidateIndices );
		Assert.Equal( 0, plan.Score.LocalDirectiveCount );
		Assert.Same( candidates[ 0 ], plan.SelectedParents[ 0 ] );
		Assert.Same( candidates[ 1 ], plan.SelectedParents[ 1 ] );
	}

	[Fact]
	public void GeneratedSourceLengthRejectsOversizedPlanAndKeepsLaterWinner() {
		string value = new( 'x', 512 );
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp04-size-target" )
				.SetString( StringCapability.Bell, value )
				.Build();
		TerminalDescription parentDescription =
			new TerminalDescriptionBuilder( "rp04-size-parent" )
				.SetString( StringCapability.Bell, value )
				.Build();
		TerminalDescriptionSourceSynthesisParent parent =
			CreateCandidate(
				parentDescription.Name,
				parentDescription
			);
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: 1
				),
				maximumCandidateCount: 1,
				maximumSelectedParentCount: 1,
				maximumGeneratedSourceLength: 100
			);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				new[] { parent },
				options
			);

		Assert.Same( parent, Assert.Single( plan.SelectedParents ) );
		Assert.True( plan.Source.Length <= 100 );
		Assert.Equal( 2, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
	}

	[Fact]
	public void RejectedDuplicateReferencePlansRemainInExhaustiveEvidence() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rp04-duplicate-target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] candidates = [
			CreateCandidate(
				"rp04-duplicate",
				new TerminalDescriptionBuilder( "rp04-duplicate" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build()
			),
			CreateCandidate(
				"rp04-duplicate",
				new TerminalDescriptionBuilder( "rp04-duplicate" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			),
		];

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				candidates,
				CreateOptions(
					candidateCount: 2,
					maximumDepth: 2,
					maximumEvaluatedPlanCount: 5
				)
			);

		Assert.Equal( 5, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
		Assert.Equal( new[] { 0 }, plan.Score.SelectedCandidateIndices );
		Assert.Same( candidates[ 0 ], Assert.Single( plan.SelectedParents ) );
	}

	[Fact]
	public void CancellationScheduledAfterSnapshotPublishesNoPartialPlan() {
		using CancellationTokenSource cancellation = new();
		TerminalDescriptionSourceSynthesisParent[] values =
			CreateCandidates( 64, "rp04-cancellation" );
		CancellationStartingEnumerable<TerminalDescriptionSourceSynthesisParent>
			candidates =
				new(
					values,
					cancellation
				);
		TerminalDescriptionSourcePlanningOptions options =
			CreateOptions(
				candidateCount: 64,
				maximumDepth: 3,
				maximumEvaluatedPlanCount: 254_081
			);
		TerminalDescriptionSourcePlan? plan = null;

		Assert.Throws<OperationCanceledException>(
			() =>
				plan = TerminalDescriptionSourcePlanner.Plan(
					new TerminalDescriptionBuilder( "rp04-cancellation-target" ).Build(),
					candidates,
					options,
					cancellation.Token
				)
		);

		Assert.True( candidates.CancellationScheduled );
		Assert.Null( plan );
	}

	[Fact]
	public void DefaultMaximumSpaceCompletesWithinCiRuntimeGuard() {
		TerminalDescriptionSourceSynthesisParent[] candidates =
			CreateCandidates(
				TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount,
				"rp04-runtime"
			);
		Stopwatch stopwatch = Stopwatch.StartNew();

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				new TerminalDescriptionBuilder( "rp04-runtime-target" ).Build(),
				candidates
			);
		stopwatch.Stop();

		Assert.Equal(
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumEvaluatedPlanCount,
			plan.EvaluatedPlanCount
		);
		Assert.True( plan.IsExhaustive );
		Assert.True(
			stopwatch.Elapsed < TimeSpan.FromSeconds( 30 ),
			$"Default bounded planning required {stopwatch.Elapsed}."
		);
	}

	[Fact]
	public void Rp04ImplementationRecordFreezesBoundsCancellationAndEvidence() {
		string implementation =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RP04-BOUNDED-SEARCH-CANCELLATION-AND-EVIDENCE.md"
				)
			);

		Assert.Contains( "1.8.0-Alpha-4", implementation, StringComparison.Ordinal );
		Assert.Contains( "checked arithmetic", implementation, StringComparison.Ordinal );
		Assert.Contains( "lexicographic prefix", implementation, StringComparison.Ordinal );
		Assert.Contains( "stable cancellation boundaries", implementation, StringComparison.Ordinal );
		Assert.Contains( "partial plan", implementation, StringComparison.Ordinal );
	}

	private static TerminalDescriptionSourcePlanningOptions CreateOptions(
		int candidateCount,
		int maximumDepth,
		int maximumEvaluatedPlanCount,
		bool allowNonExhaustiveResult = false
	) {
		return new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(
				80,
				maximumParentCount: maximumDepth
			),
			maximumCandidateCount: candidateCount,
			maximumSelectedParentCount: maximumDepth,
			maximumEvaluatedPlanCount: maximumEvaluatedPlanCount,
			allowNonExhaustiveResult: allowNonExhaustiveResult
		);
	}

	private static TerminalDescriptionSourceSynthesisParent[] CreateCandidates(
		int count,
		string prefix
	) {
		TerminalDescriptionSourceSynthesisParent[] candidates =
			new TerminalDescriptionSourceSynthesisParent[ count ];
		for ( int index = 0; index < count; index++ ) {
			string name = $"{prefix}-{index:D3}";
			candidates[ index ] =
				CreateCandidate(
					name,
					new TerminalDescriptionBuilder( name ).Build()
				);
		}
		return candidates;
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

	private sealed class CountingEnumerable<T> : IEnumerable<T> {
		private readonly IEnumerable<T> _values;

		public CountingEnumerable(
			IEnumerable<T> values
		) {
			ArgumentNullException.ThrowIfNull( values );

			_values = values;
		}

		public int YieldCount {
			get;
			private set;
		}

		public IEnumerator<T> GetEnumerator() {
			foreach ( T value in _values ) {
				YieldCount++;
				yield return value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	private sealed class CancellationStartingEnumerable<T> : IEnumerable<T> {
		private readonly CancellationTokenSource _cancellation;
		private readonly IEnumerable<T> _values;

		public CancellationStartingEnumerable(
			IEnumerable<T> values,
			CancellationTokenSource cancellation
		) {
			ArgumentNullException.ThrowIfNull( values );
			ArgumentNullException.ThrowIfNull( cancellation );

			_values = values;
			_cancellation = cancellation;
		}

		public bool CancellationScheduled {
			get;
			private set;
		}

		public IEnumerator<T> GetEnumerator() {
			foreach ( T value in _values ) {
				yield return value;
			}

			CancellationScheduled = true;
			_cancellation.CancelAfter(
				TimeSpan.FromMilliseconds( 1 )
			);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
