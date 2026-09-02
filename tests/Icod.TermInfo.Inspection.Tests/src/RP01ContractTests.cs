using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP01ContractTests {
	private const string DevelopmentVersion = "1.8.0-Alpha-1";

	[Fact]
	public void PlanningOptionsFreezeBoundedDefaults() {
		TerminalDescriptionSourcePlanningOptions options =
			new();

		Assert.Equal( 64, options.MaximumCandidateCount );
		Assert.Equal( 2, options.MaximumSelectedParentCount );
		Assert.Equal( 4_097, options.MaximumEvaluatedPlanCount );
		Assert.Equal(
			TermInfoSourceLexerOptions.DefaultMaximumSourceLength,
			options.MaximumGeneratedSourceLength
		);
		Assert.False( options.AllowNonExhaustiveResult );
		Assert.Equal(
			TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
			options.SynthesisOptions.MaximumParentCount
		);

		Assert.Equal(
			64,
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount
		);
		Assert.Equal(
			256,
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount
		);
		Assert.Equal(
			2,
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumSelectedParentCount
		);
		Assert.Equal(
			256,
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedSelectedParentCount
		);
		Assert.Equal(
			4_097,
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumEvaluatedPlanCount
		);
		Assert.Equal(
			1_000_000,
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedEvaluatedPlanCount
		);
		Assert.Equal(
			TermInfoSourceLexerOptions.MaximumSupportedSourceLength,
			TerminalDescriptionSourcePlanningOptions.MaximumSupportedGeneratedSourceLength
		);
	}

	[Fact]
	public void PlanningOptionsRetainExplicitImmutablePolicy() {
		TerminalDescriptionSourceSynthesisOptions synthesisOptions =
			new(
				100,
				TerminalDescriptionSourceLayout.SingleLine,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName,
				maximumParentCount: 3,
				includeExtendedCapabilities: false
			);
		TerminalDescriptionSourcePlanningOptions options =
			new(
				synthesisOptions,
				maximumCandidateCount: 7,
				maximumSelectedParentCount: 3,
				maximumEvaluatedPlanCount: 50,
				maximumGeneratedSourceLength: 8_192,
				allowNonExhaustiveResult: true
			);

		Assert.Same( synthesisOptions, options.SynthesisOptions );
		Assert.Equal( 7, options.MaximumCandidateCount );
		Assert.Equal( 3, options.MaximumSelectedParentCount );
		Assert.Equal( 50, options.MaximumEvaluatedPlanCount );
		Assert.Equal( 8_192, options.MaximumGeneratedSourceLength );
		Assert.True( options.AllowNonExhaustiveResult );
	}

	[Fact]
	public void PlanningOptionsRejectInvalidLimits() {
		TerminalDescriptionSourceSynthesisOptions synthesisOptions =
			new(
				80,
				maximumParentCount: 2
			);

		Assert.Throws<ArgumentNullException>(
			() => new TerminalDescriptionSourcePlanningOptions( null! )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumCandidateCount: -1
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumCandidateCount:
						TerminalDescriptionSourcePlanningOptions.MaximumSupportedCandidateCount + 1
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumCandidateCount: 1,
					maximumSelectedParentCount: 2
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumSelectedParentCount: 3
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumEvaluatedPlanCount: 0
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourcePlanningOptions(
					synthesisOptions,
					maximumGeneratedSourceLength: 0
				)
		);
	}

	[Fact]
	public void PlanningScoreFreezesLexicographicOrder() {
		TerminalDescriptionSourcePlanningScore baseline =
			CreateScore( 1, 0, 1, 100, 3 );

		Assert.True( baseline.CompareTo( CreateScore( 2, 0, 0, 1 ) ) < 0 );
		Assert.True( baseline.CompareTo( CreateScore( 1, 1, 0, 1 ) ) < 0 );
		Assert.True( baseline.CompareTo( CreateScore( 1, 0, 2, 1, 0, 1 ) ) < 0 );
		Assert.True( baseline.CompareTo( CreateScore( 1, 0, 1, 101, 0 ) ) < 0 );
		Assert.True( baseline.CompareTo( CreateScore( 1, 0, 1, 100, 4 ) ) < 0 );
		Assert.True( baseline.CompareTo( null ) > 0 );
	}

	[Fact]
	public void PlanningScoreCopiesIndicesAndUsesComponentEquality() {
		int[] indices = [ 4, 1 ];
		TerminalDescriptionSourcePlanningScore first =
			new(
				3,
				1,
				2,
				200,
				indices
			);
		indices[ 0 ] = 99;
		TerminalDescriptionSourcePlanningScore second =
			CreateScore( 3, 1, 2, 200, 4, 1 );

		Assert.Equal( new[] { 4, 1 }, first.SelectedCandidateIndices );
		Assert.Equal( first, second );
		Assert.Equal( first.GetHashCode(), second.GetHashCode() );
		Assert.Equal( 0, first.CompareTo( second ) );
		Assert.False( first.Equals( null ) );
		Assert.False( first.Equals( new object() ) );
	}

	[Fact]
	public void PlanningScoreRejectsInvalidComponents() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => CreateScore( -1, 0, 0, 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => CreateScore( 1, 2, 0, 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => CreateScore( 0, 0, -1, 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => CreateScore( 0, 0, 0, -1 )
		);
		Assert.Throws<ArgumentException>(
			() => CreateScore( 0, 0, 2, 0, 1 )
		);
		Assert.Throws<ArgumentException>(
			() => CreateScore( 0, 0, 1, 0, -1 )
		);
		Assert.Throws<ArgumentException>(
			() => CreateScore( 0, 0, 2, 0, 1, 1 )
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				new TerminalDescriptionSourcePlanningScore(
					0,
					0,
					0,
					0,
					null!
				)
		);
	}

	[Fact]
	public void PlanningRequestSnapshotsCallerSequenceOnceAndPreservesPositions() {
		TerminalDescription target =
			CreateTerminal( "target" );
		TerminalDescriptionSourceSynthesisParent first =
			CreateCandidate( "same", "first" );
		TerminalDescriptionSourceSynthesisParent second =
			CreateCandidate( "same", "second" );
		SingleUseEnumerable<TerminalDescriptionSourceSynthesisParent> candidates =
			new(
				[ first, second ]
			);

		TerminalDescriptionSourcePlanningRequest request =
			TerminalDescriptionSourcePlanner.CreateRequest(
				target,
				candidates,
				new TerminalDescriptionSourcePlanningOptions()
			);

		Assert.Equal( 1, candidates.EnumerationCount );
		Assert.Same( target, request.Target );
		Assert.Equal( 2, request.Candidates.Count );
		Assert.Same( first, request.Candidates[ 0 ] );
		Assert.Same( second, request.Candidates[ 1 ] );
	}

	[Fact]
	public void PlanningRequestExcludesOnlyOrdinalTargetSelfReferences() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "target" )
				.AddAlias( "target-alias" )
				.SetDescription( "RP01 target" )
				.Build();

		TerminalDescriptionSourcePlanningRequest request =
			TerminalDescriptionSourcePlanner.CreateRequest(
				target,
				new[] {
					CreateCandidate( "target", "self-name" ),
					CreateCandidate( "target-alias", "self-alias" ),
					CreateCandidate( "Target", "case-distinct" ),
				},
				new TerminalDescriptionSourcePlanningOptions()
			);

		Assert.Single( request.Candidates );
		Assert.Equal( "Target", request.Candidates[ 0 ].UseName );
	}

	[Fact]
	public void PlanningRequestStopsAtCandidateLimitPlusOne() {
		CountingEnumerable<TerminalDescriptionSourceSynthesisParent> candidates =
			new(
				new[] {
					CreateCandidate( "first", "first" ),
					CreateCandidate( "second", "second" ),
					CreateCandidate( "third", "third" ),
				}
			);
		TerminalDescriptionSourcePlanningOptions options =
			new(
				new TerminalDescriptionSourceSynthesisOptions(),
				maximumCandidateCount: 1,
				maximumSelectedParentCount: 1
			);

		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourcePlanner.CreateRequest(
					CreateTerminal( "target" ),
					candidates,
					options
				)
		);
		Assert.Equal( 2, candidates.YieldCount );
	}

	[Fact]
	public void PlannerValidatesObservesCancellationAndRetainsRp01Inputs() {
		TerminalDescription target =
			CreateTerminal( "target" );
		TerminalDescriptionSourcePlanningOptions options =
			new();

		Assert.Throws<ArgumentNullException>(
			() => TerminalDescriptionSourcePlanner.Plan( null!, Array.Empty<TerminalDescriptionSourceSynthesisParent>() )
		);
		Assert.Throws<ArgumentNullException>(
			() => TerminalDescriptionSourcePlanner.Plan( target, null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TerminalDescriptionSourcePlanner.Plan( target, [], null! )
		);
		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourcePlanner.Plan(
					target,
					new TerminalDescriptionSourceSynthesisParent[] { null! },
					options
				)
		);

		using var cancellation =
			new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() =>
				TerminalDescriptionSourcePlanner.Plan(
					target,
					[],
					options,
					cancellation.Token
				)
		);

		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan( target, [] );
		Assert.Empty( plan.SelectedParents );
		Assert.Equal( 1, plan.EvaluatedPlanCount );
		Assert.True( plan.IsExhaustive );
	}

	[Fact]
	public void PlanResultRetainsImmutableSelectionAndSearchEvidence() {
		TerminalDescriptionSourceSynthesisParent parent =
			CreateCandidate( "parent", "parent" );
		TerminalDescriptionSourceSynthesisParent[] parents = [ parent ];
		TerminalDescriptionSourcePlanningScore score =
			CreateScore( 2, 1, 1, 123, 0 );
		TerminalDescriptionSourcePlan plan =
			new(
				parents,
				"target,\n    use=parent,\n",
				score,
				evaluatedPlanCount: 3,
				isExhaustive: false,
				candidateCount: 2
			);
		parents[ 0 ] = CreateCandidate( "other", "other" );

		Assert.Single( plan.SelectedParents );
		Assert.Same( parent, plan.SelectedParents[ 0 ] );
		Assert.Equal( "target,\n    use=parent,\n", plan.Source );
		Assert.Same( score, plan.Score );
		Assert.Equal( 3, plan.EvaluatedPlanCount );
		Assert.False( plan.IsExhaustive );
		Assert.Equal( 2, plan.CandidateCount );
	}

	[Fact]
	public void Rp01ImplementationRecordFreezesVersionScoreAndBoundary() {
		string root =
			FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.8.0-RP01-PLANNING-CONTRACT-AND-API-FOUNDATION.md"
				)
			);

		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "4,097", implementation );
		Assert.Contains( "lexicographic", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "snapshot", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "RP02", implementation );
		Assert.Contains( "1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt", implementation );

		byte[] baseline =
			File.ReadAllBytes(
				Path.Combine(
					root,
					"docs",
					"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt"
				)
			);
		string normalizedBaseline =
			Encoding.UTF8
				.GetString( baseline )
				.Replace(
					"\r\n",
					"\n",
					StringComparison.Ordinal
				)
				.Replace(
					'\r',
					'\n'
				);
		Assert.Equal(
			"ba87cb17abe4d2c2a89851b3f9205f95bfd1116022e8b46d2883941c378f5811",
			Convert.ToHexString(
				SHA256.HashData(
					Encoding.UTF8.GetBytes( normalizedBaseline )
				)
			).ToLowerInvariant()
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

	private static TerminalDescriptionSourceSynthesisParent CreateCandidate(
		string useName,
		string descriptionName
	) {
		return new TerminalDescriptionSourceSynthesisParent(
			useName,
			CreateTerminal( descriptionName )
		);
	}

	private static TerminalDescription CreateTerminal(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( "RP01 test terminal" )
			.Build();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Unable to locate the repository root."
		);
	}

	private sealed class SingleUseEnumerable<T>(
		IEnumerable<T> values
	) : IEnumerable<T> {
		private readonly IEnumerable<T> _values =
			values ?? throw new ArgumentNullException( nameof( values ) );

		internal int EnumerationCount {
			get;
			private set;
		}

		public IEnumerator<T> GetEnumerator() {
			EnumerationCount++;
			if ( EnumerationCount != 1 ) {
				throw new InvalidOperationException( "The sequence was enumerated more than once." );
			}
			return _values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	private sealed class CountingEnumerable<T>(
		IEnumerable<T> values
	) : IEnumerable<T> {
		private readonly IEnumerable<T> _values =
			values ?? throw new ArgumentNullException( nameof( values ) );

		internal int YieldCount {
			get;
			private set;
		}

		public IEnumerator<T> GetEnumerator() {
			foreach ( T value in _values ) {
				YieldCount++;
				yield return value;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
