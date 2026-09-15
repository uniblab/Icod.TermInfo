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
/// Represents one bounded protocol-neutral advanced persistent-raster placement
/// requirement request.
/// </summary>
public sealed class PersistentRasterPlacementRequest {
	/// <summary>
	/// Initializes one advanced persistent-raster placement requirement request.
	/// </summary>
	/// <param name="requireSourceRectangle">
	/// Whether pixel-space source-rectangle semantics are required.
	/// </param>
	/// <param name="requireSignedZOrder">
	/// Whether signed stacking-order semantics are required.
	/// </param>
	/// <exception cref="ArgumentException">
	/// Neither advanced placement semantic is required.
	/// </exception>
	public PersistentRasterPlacementRequest(
		bool requireSourceRectangle = false,
		bool requireSignedZOrder = false
	) {
		if ( !requireSourceRectangle && !requireSignedZOrder ) {
			throw new ArgumentException(
				"A placement request must require at least one advanced placement semantic."
			);
		}

		RequireSourceRectangle = requireSourceRectangle;
		RequireSignedZOrder = requireSignedZOrder;
	}

	/// <summary>
	/// Gets whether pixel-space source-rectangle semantics are required.
	/// </summary>
	public bool RequireSourceRectangle {
		get;
	}

	/// <summary>
	/// Gets whether signed stacking-order semantics are required.
	/// </summary>
	public bool RequireSignedZOrder {
		get;
	}
}
