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
/// Identifies the discovery source represented by a terminfo database location.
/// </summary>
public enum TermInfoDatabaseLocationKind {
	/// <summary>
	/// An encoded <c>TERMINFO</c> entry which precedes directory discovery.
	/// </summary>
	EncodedTermInfo = 0,

	/// <summary>
	/// A directory selected by the <c>TERMINFO</c> environment variable.
	/// </summary>
	TermInfoDirectory = 1,

	/// <summary>
	/// The platform user-local terminfo database.
	/// </summary>
	UserDatabase = 2,

	/// <summary>
	/// A directory contributed by <c>TERMINFO_DIRS</c>, including a platform
	/// default inserted by an empty component.
	/// </summary>
	TermInfoDirsDirectory = 3,

	/// <summary>
	/// A platform default directory reached after environment and user sources.
	/// </summary>
	PlatformDefaultDirectory = 4,
}
