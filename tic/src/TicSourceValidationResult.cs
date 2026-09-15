/*
	tic
	Compiles and validates terminfo source descriptions.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Icod.TermInfo;

namespace Icod.TermInfo.Tic;

internal sealed class TicSourceValidationResult {
	internal TicSourceValidationResult(
		string sourceName,
		IEnumerable<TicDiagnostic> diagnostics,
		IEnumerable<TerminalDescription> descriptions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentNullException.ThrowIfNull( descriptions );

		TicDiagnostic[] diagnosticArray = diagnostics.ToArray();
		TerminalDescription[] descriptionArray = descriptions.ToArray();

		SourceName = sourceName;
		Diagnostics = Array.AsReadOnly( diagnosticArray );
		Descriptions = Array.AsReadOnly( descriptionArray );
	}

	internal string SourceName {
		get;
	}

	internal IReadOnlyList<TicDiagnostic> Diagnostics {
		get;
	}

	internal IReadOnlyList<TerminalDescription> Descriptions {
		get;
	}

	internal bool HasErrors =>
		Diagnostics.Any( diagnostic => diagnostic.IsError );
}
