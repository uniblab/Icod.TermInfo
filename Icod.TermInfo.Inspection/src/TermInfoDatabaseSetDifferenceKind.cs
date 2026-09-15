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
/// Identifies one stable database-set comparison difference category.
/// </summary>
public enum TermInfoDatabaseSetDifferenceKind {
	/// <summary>
	/// The ordered constituent root topology differs.
	/// </summary>
	RootTopology = 0,

	/// <summary>
	/// Aggregate or constituent completeness differs.
	/// </summary>
	Completeness = 1,

	/// <summary>
	/// Frozen catalog issue evidence differs.
	/// </summary>
	Issue = 2,

	/// <summary>
	/// A conclusive canonical identity is present only in the left set.
	/// </summary>
	OnlyInLeft = 3,

	/// <summary>
	/// A conclusive canonical identity is present only in the right set.
	/// </summary>
	OnlyInRight = 4,

	/// <summary>
	/// The effective precedence winners are semantically different.
	/// </summary>
	EffectiveSemantic = 5,

	/// <summary>
	/// The effective winners are semantically equal but their physical provenance
	/// differs.
	/// </summary>
	EffectiveProvenance = 6,

	/// <summary>
	/// Effective alias ownership, owner semantics, or owner provenance differs.
	/// </summary>
	AliasOwnership = 7,

	/// <summary>
	/// The observed ordered shadow set differs semantically or structurally.
	/// </summary>
	ShadowSet = 8,

	/// <summary>
	/// Incomplete evidence prevents a complete comparison conclusion.
	/// </summary>
	Indeterminate = 9,
}
