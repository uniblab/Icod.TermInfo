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
/// Classifies deterministic semantic evidence for repeated database-set
/// identities and alias collisions.
/// </summary>
public enum TermInfoDatabaseSetSemanticRelationship {
	/// <summary>
	/// The relevant effective terminal descriptions compare equal.
	/// </summary>
	SemanticallyEqual = 0,

	/// <summary>
	/// At least one relevant effective terminal description or canonical owner
	/// conflicts with the selected precedence evidence.
	/// </summary>
	SemanticallyDifferent = 1,

	/// <summary>
	/// Incomplete input prevents a conclusive semantic classification.
	/// </summary>
	Indeterminate = 2,
}
