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
/// Composes one concrete backend availability profile with the lifecycle and
/// placement semantic profiles that apply to that backend context.
/// </summary>
public sealed class RasterBackendCandidate {
	/// <summary>Initializes one immutable raster-backend candidate.</summary>
	public RasterBackendCandidate(
		RasterBackendProfile backendProfile,
		PersistentRasterLifecycleProfile lifecycleProfile,
		PersistentRasterPlacementProfile placementProfile
	) {
		ArgumentNullException.ThrowIfNull( backendProfile );
		ArgumentNullException.ThrowIfNull( lifecycleProfile );
		ArgumentNullException.ThrowIfNull( placementProfile );

		BackendProfile = backendProfile;
		LifecycleProfile = lifecycleProfile;
		PlacementProfile = placementProfile;
	}

	/// <summary>
	/// Initializes one immutable raster-backend candidate from the exact lifecycle
	/// and placement profiles retained by a frozen 1.13 runtime-integration result.
	/// </summary>
	/// <param name="backendProfile">The classified backend availability profile.</param>
	/// <param name="integration">
	/// The backend-scoped runtime-integration result whose resulting lifecycle and
	/// placement profiles are composed into the candidate.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="backendProfile"/> or <paramref name="integration"/> is
	/// <see langword="null"/>.
	/// </exception>
	public RasterBackendCandidate(
		RasterBackendProfile backendProfile,
		PersistentRasterRuntimeIntegrationResult integration
	) {
		ArgumentNullException.ThrowIfNull( backendProfile );
		ArgumentNullException.ThrowIfNull( integration );

		BackendProfile = backendProfile;
		LifecycleProfile = integration.LifecycleProfile;
		PlacementProfile = integration.PlacementProfile;
	}

	/// <summary>Gets the concrete backend availability profile.</summary>
	public RasterBackendProfile BackendProfile {
		get;
	}

	/// <summary>Gets the lifecycle semantic profile for this backend context.</summary>
	public PersistentRasterLifecycleProfile LifecycleProfile {
		get;
	}

	/// <summary>Gets the placement semantic profile for this backend context.</summary>
	public PersistentRasterPlacementProfile PlacementProfile {
		get;
	}
}
