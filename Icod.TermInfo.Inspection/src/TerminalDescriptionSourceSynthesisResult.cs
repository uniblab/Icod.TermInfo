/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

internal sealed class TerminalDescriptionSourceSynthesisResult {
	public TerminalDescriptionSourceSynthesisResult(
		string source,
		int localDirectiveCount,
		int cancellationCount
	) {
		ArgumentNullException.ThrowIfNull( source );
		if ( localDirectiveCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( localDirectiveCount ),
				localDirectiveCount,
				"The local directive count cannot be negative."
			);
		}
		if ( cancellationCount < 0
			|| cancellationCount > localDirectiveCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( cancellationCount ),
				cancellationCount,
				"The cancellation count must be nonnegative and cannot exceed the local directive count."
			);
		}

		Source = source;
		LocalDirectiveCount = localDirectiveCount;
		CancellationCount = cancellationCount;
	}

	public string Source {
		get;
	}

	public int LocalDirectiveCount {
		get;
	}

	public int CancellationCount {
		get;
	}
}
