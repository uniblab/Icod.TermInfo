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
/// Represents one requested advanced persistent-raster placement semantic and its
/// classified support state.
/// </summary>
public sealed class PersistentRasterPlacementPlanRequirement {
	internal PersistentRasterPlacementPlanRequirement(
		PersistentRasterPlacementSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement subject must be a defined value."
			);
		}
		if ( !Enum.IsDefined( supportStatus ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( supportStatus ),
				supportStatus,
				"The lifecycle support status must be a defined value."
			);
		}

		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus == PersistentRasterLifecycleSupportStatus.Unknown;
	}

	/// <summary>Gets the advanced placement semantic being required.</summary>
	public PersistentRasterPlacementSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state for the required semantic.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether this requirement can be strengthened by runtime verification.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
