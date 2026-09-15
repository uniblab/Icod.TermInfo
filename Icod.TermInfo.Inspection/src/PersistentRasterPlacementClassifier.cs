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
/// Classifies immutable advanced persistent-raster placement evidence into one
/// deterministic protocol-neutral support profile.
/// </summary>
public static class PersistentRasterPlacementClassifier {
	/// <summary>
	/// Classifies raw placement evidence using explicit per-subject provenance
	/// precedence while retaining the complete canonical evidence snapshot.
	/// </summary>
	/// <param name="evidence">The raw positive and negative placement evidence.</param>
	/// <param name="options">Optional deterministic evidence resource bounds.</param>
	/// <returns>An immutable classified placement profile.</returns>
	/// <exception cref="ArgumentException">
	/// The evidence sequence contains an invalid element or exceeds the configured
	/// evidence bound.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="evidence"/> is <see langword="null"/>.
	/// </exception>
	public static PersistentRasterPlacementProfile Classify(
		IEnumerable<PersistentRasterPlacementEvidence> evidence,
		PersistentRasterPlacementEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( evidence );

		IReadOnlyList<PersistentRasterPlacementEvidence> snapshot =
			PersistentRasterPlacementEvidence.Snapshot(
				evidence,
				options
			);
		PersistentRasterPlacementSubject[] subjects =
			Enum.GetValues<PersistentRasterPlacementSubject>();
		PersistentRasterLifecycleSupportStatus[] statuses =
			new PersistentRasterLifecycleSupportStatus[ subjects.Length ];

		foreach ( PersistentRasterPlacementSubject subject in subjects ) {
			statuses[ (int)subject ] = ClassifySubject(
				snapshot,
				subject
			);
		}

		return new PersistentRasterPlacementProfile(
			statuses,
			snapshot
		);
	}

	private static PersistentRasterLifecycleSupportStatus ClassifySubject(
		IReadOnlyList<PersistentRasterPlacementEvidence> evidence,
		PersistentRasterPlacementSubject subject
	) {
		int highestPrecedence = -1;
		bool hasPositive = false;
		bool hasNegative = false;

		foreach ( PersistentRasterPlacementEvidence item in evidence ) {
			if ( item.Subject != subject ) {
				continue;
			}

			int precedence = GetPrecedence( item.Kind );
			if ( precedence > highestPrecedence ) {
				highestPrecedence = precedence;
				hasPositive = item.IsPositive;
				hasNegative = !item.IsPositive;
			} else {
				if ( precedence == highestPrecedence ) {
					if ( item.IsPositive ) {
						hasPositive = true;
					} else {
						hasNegative = true;
					}
				}
			}
		}

		if ( highestPrecedence < 0 ) {
			return PersistentRasterLifecycleSupportStatus.Unknown;
		}
		if ( hasPositive && hasNegative ) {
			return PersistentRasterLifecycleSupportStatus.Contradicted;
		}
		if ( hasPositive ) {
			return PersistentRasterLifecycleSupportStatus.Supported;
		}

		return PersistentRasterLifecycleSupportStatus.Unsupported;
	}

	private static int GetPrecedence(
		PersistentRasterPlacementEvidenceKind kind
	) {
		return kind switch {
			PersistentRasterPlacementEvidenceKind.CapabilityDerived => 0,
			PersistentRasterPlacementEvidenceKind.Declared => 1,
			PersistentRasterPlacementEvidenceKind.Verified => 2,
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The placement evidence kind must be a defined value."
			),
		};
	}
}
