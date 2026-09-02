using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic selection of ordered parents for relative terminfo
/// source synthesis.
/// </summary>
public sealed class TerminalDescriptionSourcePlanningOptions {
	/// <summary>
	/// The default maximum number of accepted candidate positions.
	/// </summary>
	public const int DefaultMaximumCandidateCount = 64;

	/// <summary>
	/// The largest supported caller-selected candidate-count limit.
	/// </summary>
	public const int MaximumSupportedCandidateCount = 256;

	/// <summary>
	/// The default maximum number of selected ordered parents.
	/// </summary>
	public const int DefaultMaximumSelectedParentCount = 2;

	/// <summary>
	/// The largest supported caller-selected parent-count limit.
	/// </summary>
	public const int MaximumSupportedSelectedParentCount =
		TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount;

	/// <summary>
	/// The default maximum number of plans which may be evaluated.
	/// </summary>
	public const int DefaultMaximumEvaluatedPlanCount = 4_097;

	/// <summary>
	/// The largest supported caller-selected plan-evaluation limit.
	/// </summary>
	public const int MaximumSupportedEvaluatedPlanCount = 1_000_000;

	/// <summary>
	/// The default maximum generated source length in UTF-16 code units.
	/// </summary>
	public const int DefaultMaximumGeneratedSourceLength =
		TermInfoSourceLexerOptions.DefaultMaximumSourceLength;

	/// <summary>
	/// The largest supported generated source length in UTF-16 code units.
	/// </summary>
	public const int MaximumSupportedGeneratedSourceLength =
		TermInfoSourceLexerOptions.MaximumSupportedSourceLength;

	/// <summary>
	/// Initializes the canonical bounded planning policy.
	/// </summary>
	public TerminalDescriptionSourcePlanningOptions()
		: this(
			new TerminalDescriptionSourceSynthesisOptions(),
			DefaultMaximumCandidateCount,
			DefaultMaximumSelectedParentCount,
			DefaultMaximumEvaluatedPlanCount,
			DefaultMaximumGeneratedSourceLength,
			allowNonExhaustiveResult: false
		) {
	}

	/// <summary>
	/// Initializes an explicit bounded planning policy.
	/// </summary>
	/// <param name="synthesisOptions">
	/// The immutable relative-source synthesis policy used to evaluate plans.
	/// </param>
	/// <param name="maximumCandidateCount">
	/// The maximum number of non-self candidate positions accepted from one
	/// caller-owned sequence.
	/// </param>
	/// <param name="maximumSelectedParentCount">
	/// The maximum number of distinct candidate positions selected by one plan.
	/// </param>
	/// <param name="maximumEvaluatedPlanCount">
	/// The maximum number of candidate plans which may be evaluated.
	/// </param>
	/// <param name="maximumGeneratedSourceLength">
	/// The maximum accepted generated source length in UTF-16 code units.
	/// </param>
	/// <param name="allowNonExhaustiveResult">
	/// Whether the planner may return the best evaluated plan after its plan-
	/// evaluation budget is exhausted.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="synthesisOptions"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// A scalar limit lies outside its supported range, the selected-parent limit
	/// exceeds the candidate limit, or it exceeds the synthesis parent limit.
	/// </exception>
	public TerminalDescriptionSourcePlanningOptions(
		TerminalDescriptionSourceSynthesisOptions synthesisOptions,
		int maximumCandidateCount = DefaultMaximumCandidateCount,
		int maximumSelectedParentCount = DefaultMaximumSelectedParentCount,
		int maximumEvaluatedPlanCount = DefaultMaximumEvaluatedPlanCount,
		int maximumGeneratedSourceLength = DefaultMaximumGeneratedSourceLength,
		bool allowNonExhaustiveResult = false
	) {
		ArgumentNullException.ThrowIfNull( synthesisOptions );
		if ( maximumCandidateCount < 0
			|| maximumCandidateCount > MaximumSupportedCandidateCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumCandidateCount ),
				maximumCandidateCount,
				$"The maximum candidate count must be between 0 and {MaximumSupportedCandidateCount}."
			);
		}
		if ( maximumSelectedParentCount < 0
			|| maximumSelectedParentCount > MaximumSupportedSelectedParentCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSelectedParentCount ),
				maximumSelectedParentCount,
				$"The maximum selected-parent count must be between 0 and {MaximumSupportedSelectedParentCount}."
			);
		}
		if ( maximumSelectedParentCount > maximumCandidateCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSelectedParentCount ),
				maximumSelectedParentCount,
				"The maximum selected-parent count cannot exceed the maximum candidate count."
			);
		}
		if ( maximumSelectedParentCount > synthesisOptions.MaximumParentCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSelectedParentCount ),
				maximumSelectedParentCount,
				"The maximum selected-parent count cannot exceed the synthesis parent limit."
			);
		}
		if ( maximumEvaluatedPlanCount < 1
			|| maximumEvaluatedPlanCount > MaximumSupportedEvaluatedPlanCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumEvaluatedPlanCount ),
				maximumEvaluatedPlanCount,
				$"The maximum evaluated-plan count must be between 1 and {MaximumSupportedEvaluatedPlanCount}."
			);
		}
		if ( maximumGeneratedSourceLength < 1
			|| maximumGeneratedSourceLength > MaximumSupportedGeneratedSourceLength ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumGeneratedSourceLength ),
				maximumGeneratedSourceLength,
				$"The maximum generated source length must be between 1 and {MaximumSupportedGeneratedSourceLength} UTF-16 code units."
			);
		}

		SynthesisOptions = synthesisOptions;
		MaximumCandidateCount = maximumCandidateCount;
		MaximumSelectedParentCount = maximumSelectedParentCount;
		MaximumEvaluatedPlanCount = maximumEvaluatedPlanCount;
		MaximumGeneratedSourceLength = maximumGeneratedSourceLength;
		AllowNonExhaustiveResult = allowNonExhaustiveResult;
	}

	/// <summary>
	/// Gets the relative-source synthesis policy used to evaluate plans.
	/// </summary>
	public TerminalDescriptionSourceSynthesisOptions SynthesisOptions {
		get;
	}

	/// <summary>
	/// Gets the maximum accepted non-self candidate count.
	/// </summary>
	public int MaximumCandidateCount {
		get;
	}

	/// <summary>
	/// Gets the maximum number of selected ordered parents.
	/// </summary>
	public int MaximumSelectedParentCount {
		get;
	}

	/// <summary>
	/// Gets the maximum number of plans which may be evaluated.
	/// </summary>
	public int MaximumEvaluatedPlanCount {
		get;
	}

	/// <summary>
	/// Gets the maximum generated source length in UTF-16 code units.
	/// </summary>
	public int MaximumGeneratedSourceLength {
		get;
	}

	/// <summary>
	/// Gets whether a budget-limited, non-exhaustive result may be returned.
	/// </summary>
	public bool AllowNonExhaustiveResult {
		get;
	}
}
