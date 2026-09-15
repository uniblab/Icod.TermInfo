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
/// Represents one non-supported advanced persistent-raster placement requirement
/// discovered during deterministic planning.
/// </summary>
public sealed class PersistentRasterPlacementPlanIssue {
	internal PersistentRasterPlacementPlanIssue(
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
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Supported ) {
			throw new ArgumentException(
				"A supported placement requirement is not a planning issue.",
				nameof( supportStatus )
			);
		}

		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus == PersistentRasterLifecycleSupportStatus.Unknown;
	}

	/// <summary>Gets the advanced placement semantic affected by this issue.</summary>
	public PersistentRasterPlacementSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state that caused this issue.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether the issue can be strengthened by runtime verification rather
	/// than representing semantic impossibility.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
