using Icod.TermInfo;

namespace Icod.TermInfo.Tic;

internal sealed class TicSourceValidationResult {
	internal TicSourceValidationResult(
		string sourceName,
		IEnumerable<TicDiagnostic> diagnostics,
		IEnumerable<TerminalDescription> descriptions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentNullException.ThrowIfNull( descriptions );

		TicDiagnostic[] diagnosticArray = diagnostics.ToArray();
		TerminalDescription[] descriptionArray = descriptions.ToArray();

		SourceName = sourceName;
		Diagnostics = Array.AsReadOnly( diagnosticArray );
		Descriptions = Array.AsReadOnly( descriptionArray );
	}

	internal string SourceName {
		get;
	}

	internal IReadOnlyList<TicDiagnostic> Diagnostics {
		get;
	}

	internal IReadOnlyList<TerminalDescription> Descriptions {
		get;
	}

	internal bool HasErrors =>
		Diagnostics.Any( diagnostic => diagnostic.IsError );
}
