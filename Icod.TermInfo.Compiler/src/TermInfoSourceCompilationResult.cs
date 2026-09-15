/*
	Icod.TermInfo.Compiler
	Compiles terminfo source and terminal descriptions into deterministic terminfo databases.
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

using Icod.TermInfo.Source;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Contains the deterministic result of compiling one terminfo source
/// document.
/// </summary>
public sealed class TermInfoSourceCompilationResult {
	internal TermInfoSourceCompilationResult(
		IEnumerable<CompiledTermInfoSourceEntry> entries,
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( diagnostics );

		CompiledTermInfoSourceEntry[] entryArray =
			entries.ToArray();
		TermInfoSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();

		Entries = entryArray;
		Diagnostics = diagnosticArray;
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity
						== TermInfoSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets successfully compiled entries in source-document order.
	/// </summary>
	public IReadOnlyList<CompiledTermInfoSourceEntry> Entries { get; }

	/// <summary>
	/// Gets source parser and resolver diagnostics with their original source
	/// spans preserved.
	/// </summary>
	public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one source error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
