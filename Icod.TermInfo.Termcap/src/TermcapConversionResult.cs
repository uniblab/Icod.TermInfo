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
/// Contains the result of converting one resolved termcap entry into the Runtime
/// terminal-description model.
/// </summary>
public sealed class TermcapConversionResult {
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
