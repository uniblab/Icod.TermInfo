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

/// <summary>
/// Describes the deterministic canonical-name lookup state for an ordered
/// terminfo database set.
/// </summary>
public enum TermInfoDatabaseSetLookupStatus {
	/// <summary>
	/// The canonical identity was not observed and every constituent database was
	/// inspected completely, so absence is conclusive.
	/// </summary>
	NotObserved = 0,

	/// <summary>
	/// At least one occurrence was observed and the first applicable occurrence is
	/// known under caller-selected database precedence.
	/// </summary>
	WinnerKnown = 1,

	/// <summary>
	/// Incomplete evidence prevents a conclusive absence or winner determination.
	/// </summary>
	Indeterminate = 2,
}
