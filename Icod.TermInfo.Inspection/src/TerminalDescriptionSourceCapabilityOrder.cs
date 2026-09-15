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
/// Selects deterministic ordering for standard capabilities rendered within
/// each Boolean, numeric, or string group.
/// </summary>
public enum TerminalDescriptionSourceCapabilityOrder {
	/// <summary>
	/// Uses conventional compiled-table order.
	/// </summary>
	Database = 0,

	/// <summary>
	/// Orders by traditional terminfo short name using ordinal comparison.
	/// </summary>
	TermInfoName = 1,

	/// <summary>
	/// Orders by long/variable terminfo name using ordinal comparison.
	/// </summary>
	LongName = 2,

	/// <summary>
	/// Orders by termcap code using ordinal comparison.
	/// </summary>
	TermcapCode = 3,
}
