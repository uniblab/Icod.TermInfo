/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
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

namespace Icod.TermInfo.Catalogs;

/// <summary>Identifies the completeness and availability of a catalog observation.</summary>
public enum TerminalCatalogStatus {
	/// <summary>The source was inspected completely without issues.</summary>
	Complete = 0,
	/// <summary>Directory observations are incomplete or ambiguous; consult issues.</summary>
	Partial = 1,
	/// <summary>The requested source was absent at acquisition.</summary>
	Missing = 2,
	/// <summary>The source is not the explicitly selected storage kind.</summary>
	UnsupportedSource = 3,
	/// <summary>Source acquisition failed.</summary>
	Unavailable = 4,
	/// <summary>Hashed content is malformed or unsupported; no entries are returned.</summary>
	InvalidStore = 5,
}
