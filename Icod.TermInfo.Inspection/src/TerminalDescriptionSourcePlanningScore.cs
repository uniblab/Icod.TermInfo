namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents the immutable lexicographic cost of one relative-source plan.
/// </summary>
public sealed class TerminalDescriptionSourcePlanningScore
	: IComparable<TerminalDescriptionSourcePlanningScore>,
		IEquatable<TerminalDescriptionSourcePlanningScore> {
	/// <summary>
	/// Initializes one planning score.
	/// </summary>
	/// <param name="localDirectiveCount">
	/// The number of emitted local capability directives, including cancellations.
	/// </param>
	/// <param name="cancellationCount">
	/// The number of emitted standard and extended capability cancellations.
	/// </param>
	/// <param name="parentCount">The number of selected ordered parents.</param>
	/// <param name="renderedUtf8ByteCount">
	/// The UTF-8 byte count of the generated LF source without a byte-order mark.
	/// </param>
	/// <param name="selectedCandidateIndices">
	/// The selected distinct zero-based candidate positions in emitted parent order.
	/// </param>
	/// <exception cref="ArgumentException">
	/// The candidate-index count differs from <paramref name="parentCount"/>, an
	/// index is negative, or a candidate position occurs more than once.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="selectedCandidateIndices"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// A count is negative or <paramref name="cancellationCount"/> exceeds
	/// <paramref name="localDirectiveCount"/>.
	/// </exception>
	public TerminalDescriptionSourcePlanningScore(
		int localDirectiveCount,
		int cancellationCount,
		int parentCount,
		int renderedUtf8ByteCount,
		IEnumerable<int> selectedCandidateIndices
	) {
		ArgumentNullException.ThrowIfNull( selectedCandidateIndices );
		if ( localDirectiveCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( localDirectiveCount ),
				localDirectiveCount,
				"The local directive count cannot be negative."
			);
		}
		if ( cancellationCount < 0
			|| cancellationCount > localDirectiveCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( cancellationCount ),
				cancellationCount,
				"The cancellation count must be nonnegative and cannot exceed the local directive count."
			);
		}
		if ( parentCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( parentCount ),
				parentCount,
				"The parent count cannot be negative."
			);
		}
		if ( renderedUtf8ByteCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( renderedUtf8ByteCount ),
				renderedUtf8ByteCount,
				"The rendered UTF-8 byte count cannot be negative."
			);
		}

		int[] candidateIndices =
			selectedCandidateIndices.ToArray();
		if ( candidateIndices.Length != parentCount ) {
			throw new ArgumentException(
				"The selected candidate-index count must equal the parent count.",
				nameof( selectedCandidateIndices )
			);
		}

		HashSet<int> seenIndices = [];
		foreach ( int candidateIndex in candidateIndices ) {
			if ( candidateIndex < 0 ) {
				throw new ArgumentException(
					"Selected candidate indices cannot be negative.",
					nameof( selectedCandidateIndices )
				);
			}
			if ( !seenIndices.Add( candidateIndex ) ) {
				throw new ArgumentException(
					"A candidate position cannot occur more than once in one plan.",
					nameof( selectedCandidateIndices )
				);
			}
		}

		LocalDirectiveCount = localDirectiveCount;
		CancellationCount = cancellationCount;
		ParentCount = parentCount;
		RenderedUtf8ByteCount = renderedUtf8ByteCount;
		SelectedCandidateIndices = Array.AsReadOnly( candidateIndices );
	}

	/// <summary>
	/// Gets the emitted local capability directive count.
	/// </summary>
	public int LocalDirectiveCount {
		get;
	}

	/// <summary>
	/// Gets the emitted cancellation count.
	/// </summary>
	public int CancellationCount {
		get;
	}

	/// <summary>
	/// Gets the selected ordered-parent count.
	/// </summary>
	public int ParentCount {
		get;
	}

	/// <summary>
	/// Gets the rendered UTF-8 byte count without a byte-order mark.
	/// </summary>
	public int RenderedUtf8ByteCount {
		get;
	}

	/// <summary>
	/// Gets the selected candidate positions in emitted parent order.
	/// </summary>
	public IReadOnlyList<int> SelectedCandidateIndices {
		get;
	}

	/// <summary>
	/// Compares this score with another score using the frozen planning order.
	/// </summary>
	/// <param name="other">The score to compare with this score.</param>
	/// <returns>
	/// A negative value when this score is preferred, zero when the scores are
	/// equal, or a positive value when <paramref name="other"/> is preferred.
	/// Every score sorts after <see langword="null"/>.
	/// </returns>
	public int CompareTo(
		TerminalDescriptionSourcePlanningScore? other
	) {
		if ( other is null ) {
			return 1;
		}

		int comparison =
			LocalDirectiveCount.CompareTo( other.LocalDirectiveCount );
		if ( comparison != 0 ) {
			return comparison;
		}
		comparison =
			CancellationCount.CompareTo( other.CancellationCount );
		if ( comparison != 0 ) {
			return comparison;
		}
		comparison =
			ParentCount.CompareTo( other.ParentCount );
		if ( comparison != 0 ) {
			return comparison;
		}
		comparison =
			RenderedUtf8ByteCount.CompareTo( other.RenderedUtf8ByteCount );
		if ( comparison != 0 ) {
			return comparison;
		}

		for ( int index = 0; index < SelectedCandidateIndices.Count; index++ ) {
			comparison =
				SelectedCandidateIndices[ index ].CompareTo(
					other.SelectedCandidateIndices[ index ]
				);
			if ( comparison != 0 ) {
				return comparison;
			}
		}

		return 0;
	}

	/// <summary>
	/// Determines whether this score has the same components as another score.
	/// </summary>
	/// <param name="other">The other score.</param>
	/// <returns><see langword="true"/> when every score component is equal.</returns>
	public bool Equals(
		TerminalDescriptionSourcePlanningScore? other
	) {
		return ReferenceEquals( this, other )
			|| ( other is not null && CompareTo( other ) == 0 );
	}

	/// <inheritdoc/>
	public override bool Equals(
		object? obj
	) {
		return Equals( obj as TerminalDescriptionSourcePlanningScore );
	}

	/// <inheritdoc/>
	public override int GetHashCode() {
		HashCode hash = new();
		hash.Add( LocalDirectiveCount );
		hash.Add( CancellationCount );
		hash.Add( ParentCount );
		hash.Add( RenderedUtf8ByteCount );
		foreach ( int candidateIndex in SelectedCandidateIndices ) {
			hash.Add( candidateIndex );
		}
		return hash.ToHashCode();
	}
}
