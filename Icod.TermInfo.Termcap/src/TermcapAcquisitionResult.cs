/*
	Icod.TermInfo.Termcap
	Provides managed termcap parsing, conversion, rendering, and interoperability.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the outcome of explicit termcap acquisition, inheritance resolution,
/// and Runtime semantic conversion.
/// </summary>
public sealed class TermcapAcquisitionResult {
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
