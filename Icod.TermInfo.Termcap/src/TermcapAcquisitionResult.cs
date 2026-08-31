using System.Collections.ObjectModel;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the outcome of explicit termcap acquisition, inheritance resolution,
/// and Runtime semantic conversion.
/// </summary>
public sealed class TermcapAcquisitionResult
{
	internal TermcapAcquisitionResult(
		TerminalDescription? description,
		TermcapAcquisitionSource? source,
		IEnumerable<TermcapSourceDiagnostic> sourceDiagnostics,
		IEnumerable<TermcapConversionDiagnostic> conversionDiagnostics
	) {
		ArgumentNullException.ThrowIfNull( sourceDiagnostics );
		ArgumentNullException.ThrowIfNull( conversionDiagnostics );

		TermcapSourceDiagnostic[] sourceDiagnosticArray =
			sourceDiagnostics.ToArray();
		TermcapConversionDiagnostic[] conversionDiagnosticArray =
			conversionDiagnostics.ToArray();

		Description = description;
		Source = source;
		SourceDiagnostics =
			new ReadOnlyCollection<TermcapSourceDiagnostic>(
				sourceDiagnosticArray
			);
		ConversionDiagnostics =
			new ReadOnlyCollection<TermcapConversionDiagnostic>(
				conversionDiagnosticArray
			);
		HasErrors =
			sourceDiagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
			)
			|| conversionDiagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapConversionDiagnosticSeverity.Error
			);
		HasLoss =
			conversionDiagnosticArray.Any(
				diagnostic =>
					diagnostic.Decision == TermcapConversionDecision.Approximation
					|| diagnostic.Decision == TermcapConversionDecision.Unsupported
					|| diagnostic.Decision == TermcapConversionDecision.Unrepresentable
			);
	}

	/// <summary>
	/// Gets the acquired immutable Runtime terminal description when acquisition
	/// and conversion completed.
	/// </summary>
	public TerminalDescription? Description { get; }

	/// <summary>
	/// Gets the configured source which supplied the requested root entry, when
	/// one was located.
	/// </summary>
	public TermcapAcquisitionSource? Source { get; }

	/// <summary>
	/// Gets parser and resolver diagnostics in deterministic acquisition order.
	/// </summary>
	public IReadOnlyList<TermcapSourceDiagnostic> SourceDiagnostics { get; }

	/// <summary>Gets semantic conversion diagnostics.</summary>
	public IReadOnlyList<TermcapConversionDiagnostic> ConversionDiagnostics { get; }

	/// <summary>Gets whether acquisition or conversion produced an error.</summary>
	public bool HasErrors { get; }

	/// <summary>Gets whether semantic conversion reported representational loss.</summary>
	public bool HasLoss { get; }

	/// <summary>Gets whether a source entry matching the requested name was found.</summary>
	public bool Found => Source is not null;

	/// <summary>
	/// Gets whether a complete Runtime description was produced without errors.
	/// </summary>
	public bool IsSuccess => Description is not null && !HasErrors;

	/// <summary>
	/// Gets whether successful acquisition completed without semantic loss.
	/// </summary>
	public bool IsLossless => IsSuccess && !HasLoss;
}
