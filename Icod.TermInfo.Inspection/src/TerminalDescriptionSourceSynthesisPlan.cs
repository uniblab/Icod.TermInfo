using System.Collections.ObjectModel;

namespace Icod.TermInfo.Inspection;

internal sealed class TerminalDescriptionSourceSynthesisPlan {
	internal TerminalDescriptionSourceSynthesisPlan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
		TerminalDescriptionSourceSynthesisOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( options );

		Target = target;
		Parents =
			new ReadOnlyCollection<TerminalDescriptionSourceSynthesisParent>(
				parents.ToArray()
			);
		Options = options;
	}

	internal TerminalDescription Target {
		get;
	}

	internal IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Parents {
		get;
	}

	internal TerminalDescriptionSourceSynthesisOptions Options {
		get;
	}
}
