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

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the preflight decision for rendering a Runtime terminal description
/// as conventional termcap source.
/// </summary>
public sealed class TermcapRepresentabilityResult {
	internal TermcapRepresentabilityResult(
		IEnumerable<TermcapRenderDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapRenderDiagnostic[] diagnosticArray = diagnostics.ToArray();
		Diagnostics =
			new ReadOnlyCollection<TermcapRenderDiagnostic>(
				diagnosticArray
			);
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapRenderDiagnosticSeverity.Error
			);
	}

	/// <summary>Gets preflight diagnostics in deterministic decision order.</summary>
	public IReadOnlyList<TermcapRenderDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether at least one representability error was found.</summary>
	public bool HasErrors { get; }

	/// <summary>Gets whether the description can be rendered without semantic loss.</summary>
	public bool IsRepresentable => !HasErrors;
}
