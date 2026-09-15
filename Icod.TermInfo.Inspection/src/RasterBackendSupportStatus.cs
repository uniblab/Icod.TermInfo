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
/// Identifies the deterministic availability classification of one raster
/// backend.
/// </summary>
public enum RasterBackendSupportStatus {
	/// <summary>No conclusive backend availability evidence is available.</summary>
	Unknown = 0,

	/// <summary>The backend is conclusively supported.</summary>
	Supported = 1,

	/// <summary>The backend is conclusively unsupported.</summary>
	Unsupported = 2,

	/// <summary>Equally authoritative evidence contradicts backend availability.</summary>
	Contradicted = 3,
}
