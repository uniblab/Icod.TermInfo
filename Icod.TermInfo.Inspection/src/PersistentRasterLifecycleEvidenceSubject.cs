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
/// Identifies one protocol-neutral persistent-raster lifecycle dimension about
/// which static or caller-supplied evidence may make a positive or negative
/// assertion.
/// </summary>
public enum PersistentRasterLifecycleEvidenceSubject {
	/// <summary>
	/// Ordinary one-shot raster display capability.
	/// </summary>
	RasterDisplay = 0,

	/// <summary>
	/// Upload of raster content into a reusable terminal-resident resource.
	/// </summary>
	PersistentUpload = 1,

	/// <summary>
	/// Positive acknowledgement or equivalent confirmation of an upload.
	/// </summary>
	AcknowledgedUpload = 2,

	/// <summary>
	/// Creation of a visible placement referring to a persistent resource.
	/// </summary>
	PlacementCreation = 3,

	/// <summary>
	/// Simultaneous multiple placements referring to one persistent resource.
	/// </summary>
	MultiplePlacements = 4,

	/// <summary>
	/// Update or replacement of an existing placement while retaining its
	/// semantic identity.
	/// </summary>
	PlacementUpdate = 5,

	/// <summary>
	/// Targeted deletion of a placement without necessarily deleting its
	/// underlying raster resource.
	/// </summary>
	PlacementDeletion = 6,

	/// <summary>
	/// Targeted deletion of terminal-resident raster resource data.
	/// </summary>
	ResourceDeletion = 7,
}
