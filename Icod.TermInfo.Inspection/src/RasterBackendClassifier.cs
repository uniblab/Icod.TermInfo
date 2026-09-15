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
/// Classifies immutable raster-backend availability evidence into one
/// deterministic backend profile.
/// </summary>
public static class RasterBackendClassifier {
	/// <summary>
	/// Classifies availability evidence for one concrete raster backend using
	/// explicit provenance precedence while retaining the complete canonical
	/// evidence snapshot.
	/// </summary>
	/// <param name="backend">The concrete backend being classified.</param>
	/// <param name="evidence">The raw positive and negative availability evidence.</param>
	/// <param name="options">Optional deterministic evidence resource bounds.</param>
	/// <returns>An immutable classified raster-backend profile.</returns>
	/// <exception cref="ArgumentException">
	/// The evidence sequence contains evidence for another backend or exceeds the
	/// configured evidence bound.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="evidence"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="backend"/> is not a defined backend value.
	/// </exception>
	public static RasterBackendProfile Classify(
		RasterBackendKind backend,
		IEnumerable<RasterBackendEvidence> evidence,
		RasterBackendEvidenceOptions? options = null
	) {
		if ( !Enum.IsDefined( backend ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( backend ),
				backend,
				"The raster backend must be a defined value."
			);
		}
		ArgumentNullException.ThrowIfNull( evidence );

		IReadOnlyList<RasterBackendEvidence> snapshot =
			RasterBackendEvidence.Snapshot(
				evidence,
				options
			);
		int highestPrecedence = -1;
		bool hasPositive = false;
		bool hasNegative = false;

		foreach ( RasterBackendEvidence item in snapshot ) {
			if ( item.Backend != backend ) {
				throw new ArgumentException(
					"Raster-backend classification evidence must describe only the requested backend.",
					nameof( evidence )
				);
			}

			int precedence = GetPrecedence( item.Kind );
			if ( precedence > highestPrecedence ) {
				highestPrecedence = precedence;
				hasPositive = item.IsPositive;
				hasNegative = !item.IsPositive;
			} else if ( precedence == highestPrecedence ) {
				if ( item.IsPositive ) {
					hasPositive = true;
				} else {
					hasNegative = true;
				}
			}
		}

		RasterBackendSupportStatus status;
		if ( highestPrecedence < 0 ) {
			status = RasterBackendSupportStatus.Unknown;
		} else if ( hasPositive && hasNegative ) {
			status = RasterBackendSupportStatus.Contradicted;
		} else if ( hasPositive ) {
			status = RasterBackendSupportStatus.Supported;
		} else {
			status = RasterBackendSupportStatus.Unsupported;
		}

		return new RasterBackendProfile(
			backend,
			status,
			snapshot
		);
	}

	private static int GetPrecedence(
		RasterBackendEvidenceKind kind
	) {
		return kind switch {
			RasterBackendEvidenceKind.CapabilityDerived => 0,
			RasterBackendEvidenceKind.Declared => 1,
			RasterBackendEvidenceKind.Verified => 2,
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The raster-backend evidence kind must be a defined value."
			),
		};
	}
}
