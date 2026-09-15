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
/// Identifies a protocol-neutral semantic operation in the raster lifecycle.
/// </summary>
public enum PersistentRasterLifecycleOperation {
	/// <summary>
	/// Displays raster content ephemerally without establishing a reusable
	/// terminal-resident resource.
	/// </summary>
	DisplayEphemeral = 0,

	/// <summary>
	/// Uploads raster data as a reusable terminal-resident resource without
	/// requiring immediate display.
	/// </summary>
	UploadResource = 1,

	/// <summary>
	/// Creates a visible placement that refers to an existing persistent raster
	/// resource.
	/// </summary>
	CreatePlacement = 2,

	/// <summary>
	/// Replaces or updates an existing placement while retaining its semantic
	/// placement identity.
	/// </summary>
	UpdatePlacement = 3,

	/// <summary>
	/// Removes one existing placement without necessarily deleting its underlying
	/// raster resource.
	/// </summary>
	DeletePlacement = 4,

	/// <summary>
	/// Removes terminal-resident raster resource data.
	/// </summary>
	DeleteResource = 5,
}
