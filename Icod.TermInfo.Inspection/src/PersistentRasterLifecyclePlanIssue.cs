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
/// Represents one structured non-supported requirement discovered during
/// persistent-raster lifecycle planning.
/// </summary>
public sealed class PersistentRasterLifecyclePlanIssue {
	internal PersistentRasterLifecyclePlanIssue(
		PersistentRasterLifecycleOperation operation,
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		if ( !Enum.IsDefined( operation ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( operation ),
				operation,
				"The lifecycle operation must be a defined value."
			);
		}
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
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
				"A supported requirement is not a lifecycle planning issue.",
				nameof( supportStatus )
			);
		}

		Operation = operation;
		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus
				is PersistentRasterLifecycleSupportStatus.Unknown
				or PersistentRasterLifecycleSupportStatus.Contradicted;
	}

	/// <summary>Gets the semantic operation affected by this issue.</summary>
	public PersistentRasterLifecycleOperation Operation {
		get;
	}

	/// <summary>Gets the semantic capability that caused this issue.</summary>
	public PersistentRasterLifecycleEvidenceSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state that caused this issue.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether the issue can be strengthened by runtime verification rather
	/// than representing conclusive semantic impossibility.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
