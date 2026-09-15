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
/// Contains tokens and diagnostics produced from one terminfo source document.
/// </summary>
public sealed class TermInfoSourceLexResult {
	internal TermInfoSourceLexResult(
		IEnumerable<TermInfoSourceToken> tokens,
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( tokens );
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermInfoSourceToken[] tokenArray =
			tokens.ToArray();
		TermInfoSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();

		Tokens = tokenArray;
		Diagnostics = diagnosticArray;
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity
						== TermInfoSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets the semantic lexical units in source order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceToken> Tokens { get; }

	/// <summary>
	/// Gets diagnostics in deterministic source order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
