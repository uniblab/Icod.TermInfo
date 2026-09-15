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
/// Identifies one non-fatal issue encountered while inspecting a conventional
/// terminfo database root.
/// </summary>
public enum TermInfoDatabaseCatalogIssueKind {
	/// <summary>
	/// A candidate file could not be parsed as a supported compiled terminfo
	/// entry, including configured resource-limit failures.
	/// </summary>
	MalformedEntry = 0,

	/// <summary>
	/// A parsed entry is not conventionally placed for the identity represented
	/// by its containing file.
	/// </summary>
	InvalidPlacement = 1,

	/// <summary>
	/// Filesystem access was denied.
	/// </summary>
	PermissionFailure = 2,

	/// <summary>
	/// A filesystem operation failed for another I/O reason.
	/// </summary>
	IoFailure = 3,

	/// <summary>
	/// A symbolic-link, junction, or other reparse-point candidate was skipped
	/// rather than followed.
	/// </summary>
	LinkSkipped = 4,
}
