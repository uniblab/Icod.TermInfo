namespace Icod.TermInfo.Inspection;

internal sealed class TerminalDescriptionSourcePlanningRequest {
	internal TerminalDescriptionSourcePlanningRequest(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		Target = target;
		Candidates = Array.AsReadOnly( candidates.ToArray() );
		Options = options;
	}

	internal TerminalDescription Target {
		get;
	}

	internal IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Candidates {
		get;
	}

	internal TerminalDescriptionSourcePlanningOptions Options {
		get;
	}
}
