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
/// Contains the result of resolving one terminfo source entry and its
/// inheritance graph.
/// </summary>
public sealed class TermInfoSourceResolveResult {
	internal TermInfoSourceResolveResult(
		TermInfoSourceResolvedEntry? entry,
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermInfoSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();

		Entry = entry;
		Diagnostics = diagnosticArray;
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity
						== TermInfoSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets the resolved entry when resolution succeeded completely.
	/// </summary>
	/// <remarks>
	/// Resolution does not expose a partial semantic result when an inheritance
	/// error occurs.
	/// </remarks>
	public TermInfoSourceResolvedEntry? Entry { get; }

	/// <summary>
	/// Gets resolver diagnostics in deterministic source order.
	/// </summary>
	public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
