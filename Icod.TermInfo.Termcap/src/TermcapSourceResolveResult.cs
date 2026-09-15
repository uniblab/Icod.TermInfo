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
/// Contains the result of resolving one termcap source entry and its
/// <c>tc=</c> inheritance chain.
/// </summary>
public sealed class TermcapSourceResolveResult {
	internal TermcapSourceResolveResult(
		TermcapSourceResolvedEntry? entry,
		IEnumerable<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();
		Entry = entry;
		Diagnostics =
			new ReadOnlyCollection<TermcapSourceDiagnostic>(
				diagnosticArray
			);
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets the resolved entry when resolution succeeded completely.
	/// </summary>
	/// <remarks>
	/// Resolution does not expose a partial effective field set when an
	/// inheritance error occurs.
	/// </remarks>
	public TermcapSourceResolvedEntry? Entry { get; }

	/// <summary>
	/// Gets resolver diagnostics in deterministic source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
