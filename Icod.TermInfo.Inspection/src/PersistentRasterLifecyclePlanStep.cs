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
/// Represents one immutable ordered semantic operation in a persistent-raster
/// lifecycle plan.
/// </summary>
public sealed class PersistentRasterLifecyclePlanStep {
	internal PersistentRasterLifecyclePlanStep(
		int sequenceIndex,
		PersistentRasterLifecycleOperation operation,
		bool requiresRuntimeVerification
	) {
		if ( sequenceIndex < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sequenceIndex ),
				sequenceIndex,
				"The plan-step sequence index cannot be negative."
			);
		}
		if ( !Enum.IsDefined( operation ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( operation ),
				operation,
				"The lifecycle operation must be a defined value."
			);
		}

		SequenceIndex = sequenceIndex;
		Operation = operation;
		RequiresRuntimeVerification = requiresRuntimeVerification;
	}

	/// <summary>Gets the zero-based deterministic plan-step position.</summary>
	public int SequenceIndex {
		get;
	}

	/// <summary>Gets the protocol-neutral semantic operation.</summary>
	public PersistentRasterLifecycleOperation Operation {
		get;
	}

	/// <summary>
	/// Gets whether this step depends on one or more unknown or contradicted
	/// semantic capabilities that must be verified by the runtime consumer.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
