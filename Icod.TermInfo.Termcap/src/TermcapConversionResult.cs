using System.Collections.ObjectModel;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the result of converting one resolved termcap entry into the Runtime
/// terminal-description model.
/// </summary>
public sealed class TermcapConversionResult
{
	internal TermcapConversionResult(
		TerminalDescription? description,
		IEnumerable<TermcapConversionDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapConversionDiagnostic[] diagnosticArray =
			diagnostics.ToArray();
		Description = description;
		Diagnostics =
			new ReadOnlyCollection<TermcapConversionDiagnostic>(
				diagnosticArray
			);
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapConversionDiagnosticSeverity.Error
			);
		HasLoss =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Decision == TermcapConversionDecision.Approximation
					|| diagnostic.Decision == TermcapConversionDecision.Unsupported
					|| diagnostic.Decision == TermcapConversionDecision.Unrepresentable
			);
	}

	/// <summary>
	/// Gets the immutable Runtime terminal description when conversion completed.
	/// </summary>
	public TerminalDescription? Description { get; }

	/// <summary>Gets conversion diagnostics in deterministic decision order.</summary>
	public IReadOnlyList<TermcapConversionDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether conversion produced at least one error.</summary>
	public bool HasErrors { get; }

	/// <summary>
	/// Gets whether conversion required an approximation or left understood source
	/// semantics unsupported or unrepresentable.
	/// </summary>
	public bool HasLoss { get; }

	/// <summary>Gets whether the completed conversion is semantically lossless.</summary>
	public bool IsLossless => !HasErrors && !HasLoss;
}
