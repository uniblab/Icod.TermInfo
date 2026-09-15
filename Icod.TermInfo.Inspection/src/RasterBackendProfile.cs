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
/// Represents the immutable classified availability profile of one raster
/// backend.
/// </summary>
public sealed class RasterBackendProfile {
	internal RasterBackendProfile(
		RasterBackendKind backend,
		RasterBackendSupportStatus status,
		IReadOnlyList<RasterBackendEvidence> evidence
	) {
		if ( !Enum.IsDefined( backend ) ) {
			throw new ArgumentOutOfRangeException( nameof( backend ) );
		}
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException( nameof( status ) );
		}
		ArgumentNullException.ThrowIfNull( evidence );

		RasterBackendEvidence[] snapshot = evidence.ToArray();
		if ( snapshot.Length > RasterBackendEvidenceOptions.MaximumSupportedEvidenceCount ) {
			throw new ArgumentException(
				$"Raster-backend profiles cannot retain more than {RasterBackendEvidenceOptions.MaximumSupportedEvidenceCount} evidence entries.",
				nameof( evidence )
			);
		}
		foreach ( RasterBackendEvidence? item in snapshot ) {
			if ( item is null ) {
				throw new ArgumentException(
					"Raster-backend profile evidence cannot contain null elements.",
					nameof( evidence )
				);
			}
			if ( item.Backend != backend ) {
				throw new ArgumentException(
					"Raster-backend profile evidence must describe the profile backend.",
					nameof( evidence )
				);
			}
		}

		Backend = backend;
		Status = status;
		Evidence = Array.AsReadOnly( snapshot );
	}

	/// <summary>Gets the concrete backend represented by this profile.</summary>
	public RasterBackendKind Backend {
		get;
	}

	/// <summary>Gets the classified backend availability status.</summary>
	public RasterBackendSupportStatus Status {
		get;
	}

	/// <summary>Gets the immutable evidence retained by this profile.</summary>
	public IReadOnlyList<RasterBackendEvidence> Evidence {
		get;
	}
}
