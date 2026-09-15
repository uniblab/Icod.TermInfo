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
/// Identifies the observed storage state for one explicitly inspected terminfo
/// database root.
/// </summary>
public enum TermInfoDatabaseCatalogKind {
	/// <summary>
	/// The requested root does not currently exist.
	/// </summary>
	Missing = 0,

	/// <summary>
	/// The requested root is a conventional terminfo directory.
	/// </summary>
	ConventionalDirectory = 1,

	/// <summary>
	/// The requested root identifies a non-directory store which this release
	/// does not support.
	/// </summary>
	UnsupportedStore = 2,

	/// <summary>
	/// The requested root exists or may exist, but could not be inspected
	/// sufficiently to determine or enumerate its conventional contents.
	/// </summary>
	Unavailable = 3,
}
