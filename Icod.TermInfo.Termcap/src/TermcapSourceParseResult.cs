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
/// Contains a parsed termcap source document and its diagnostics.
/// </summary>
public sealed class TermcapSourceParseResult {
	internal TermcapSourceParseResult(
		TermcapSourceDocument document,
		IEnumerable<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( diagnostics );

		Document = document;
		Diagnostics =
			new ReadOnlyCollection<TermcapSourceDiagnostic>(
				diagnostics.ToArray()
			);
	}

	/// <summary>Gets the unresolved parsed document.</summary>
	public TermcapSourceDocument Document { get; }

	/// <summary>Gets diagnostics in deterministic source order.</summary>
	public IReadOnlyList<TermcapSourceDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether any diagnostic has error severity.</summary>
	public bool HasErrors =>
		Diagnostics.Any(
			diagnostic =>
				diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
		);
}
