/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

namespace Icod.TermInfo.Source;

/// <summary>
/// Contains the result of interpreting one numeric terminfo source value.
/// </summary>
public sealed class TermInfoSourceNumericValueResult {
	internal TermInfoSourceNumericValueResult(
		int? value,
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermInfoSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();

		Value = value;
		Diagnostics = diagnosticArray;
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity
						== TermInfoSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets the decoded numeric value, or <see langword="null"/> when the source
	/// value is invalid.
	/// </summary>
	public int? Value { get; }

	/// <summary>
	/// Gets value-semantics diagnostics in deterministic source order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
