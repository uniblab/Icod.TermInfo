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
/// Composes the frozen lifecycle request with optional advanced placement
/// requirements for raster-backend selection.
/// </summary>
public sealed class RasterBackendSelectionRequest {
	/// <summary>Initializes one raster-backend selection request.</summary>
	public RasterBackendSelectionRequest(
		PersistentRasterLifecycleRequest lifecycleRequest,
		PersistentRasterPlacementRequest? placementRequest = null
	) {
		ArgumentNullException.ThrowIfNull( lifecycleRequest );
		LifecycleRequest = lifecycleRequest;
		PlacementRequest = placementRequest;
	}

	/// <summary>Gets the frozen lifecycle semantic request.</summary>
	public PersistentRasterLifecycleRequest LifecycleRequest {
		get;
	}

	/// <summary>
	/// Gets optional advanced placement requirements, or <see langword="null"/> when
	/// no advanced placement semantics are requested.
	/// </summary>
	public PersistentRasterPlacementRequest? PlacementRequest {
		get;
	}
}
