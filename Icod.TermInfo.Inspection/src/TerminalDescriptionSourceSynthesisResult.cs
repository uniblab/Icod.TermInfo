namespace Icod.TermInfo.Inspection;

internal sealed class TerminalDescriptionSourceSynthesisResult {
	public TerminalDescriptionSourceSynthesisResult(
		string source,
		int localDirectiveCount,
		int cancellationCount
	) {
		ArgumentNullException.ThrowIfNull( source );
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

		Source = source;
		LocalDirectiveCount = localDirectiveCount;
		CancellationCount = cancellationCount;
	}

	public string Source {
		get;
	}

	public int LocalDirectiveCount {
		get;
	}

	public int CancellationCount {
		get;
	}
}
