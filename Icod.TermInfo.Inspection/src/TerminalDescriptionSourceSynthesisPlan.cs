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

using System.Collections.ObjectModel;

namespace Icod.TermInfo.Inspection;

internal sealed class TerminalDescriptionSourceSynthesisPlan {
	internal TerminalDescriptionSourceSynthesisPlan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
		TerminalDescriptionSourceSynthesisOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( options );

		Target = target;
		Parents =
			new ReadOnlyCollection<TerminalDescriptionSourceSynthesisParent>(
				parents.ToArray()
			);
		Options = options;
	}

	internal TerminalDescription Target {
		get;
	}

	internal IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Parents {
		get;
	}

	internal TerminalDescriptionSourceSynthesisOptions Options {
		get;
	}
}
