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
/// Represents one immutable protocol-neutral classification of persistent-raster
/// lifecycle support together with the complete canonical evidence snapshot.
/// </summary>
public sealed class PersistentRasterLifecycleProfile {
	internal PersistentRasterLifecycleProfile(
		IReadOnlyList<PersistentRasterLifecycleSupportStatus> statuses,
		IReadOnlyList<PersistentRasterLifecycleEvidence> evidence
	) {
		ArgumentNullException.ThrowIfNull( statuses );
		ArgumentNullException.ThrowIfNull( evidence );

		int expectedStatusCount =
			Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>().Length;
		if ( statuses.Count != expectedStatusCount ) {
			throw new ArgumentException(
				$"A lifecycle profile must contain exactly {expectedStatusCount} support states.",
				nameof( statuses )
			);
		}
		foreach ( PersistentRasterLifecycleSupportStatus status in statuses ) {
			if ( !Enum.IsDefined( status ) ) {
				throw new ArgumentException(
					"A lifecycle profile cannot contain an undefined support state.",
					nameof( statuses )
				);
			}
		}

		RasterDisplay =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.RasterDisplay ];
		PersistentUpload =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.PersistentUpload ];
		AcknowledgedUpload =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload ];
		PlacementCreation =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.PlacementCreation ];
		MultiplePlacements =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.MultiplePlacements ];
		PlacementUpdate =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.PlacementUpdate ];
		PlacementDeletion =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.PlacementDeletion ];
		ResourceDeletion =
			statuses[ (int)PersistentRasterLifecycleEvidenceSubject.ResourceDeletion ];
		Evidence =
			Array.AsReadOnly(
				evidence.ToArray()
			);
	}

	/// <summary>
	/// Gets the classified support state for ephemeral raster display.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus RasterDisplay {
		get;
	}

	/// <summary>
	/// Gets the classified support state for persistent raster upload.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus PersistentUpload {
		get;
	}

	/// <summary>
	/// Gets the classified support state for acknowledged persistent upload.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus AcknowledgedUpload {
		get;
	}

	/// <summary>
	/// Gets the classified support state for placement creation.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus PlacementCreation {
		get;
	}

	/// <summary>
	/// Gets the classified support state for multiple placements of one resource.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus MultiplePlacements {
		get;
	}

	/// <summary>
	/// Gets the classified support state for placement update.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus PlacementUpdate {
		get;
	}

	/// <summary>
	/// Gets the classified support state for placement deletion.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus PlacementDeletion {
		get;
	}

	/// <summary>
	/// Gets the classified support state for persistent resource deletion.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus ResourceDeletion {
		get;
	}

	/// <summary>
	/// Gets the complete canonical immutable evidence snapshot used to classify this
	/// profile, including lower-precedence and contradictory evidence.
	/// </summary>
	public IReadOnlyList<PersistentRasterLifecycleEvidence> Evidence {
		get;
	}

	/// <summary>
	/// Gets the classified support state for one lifecycle evidence subject.
	/// </summary>
	/// <param name="subject">The lifecycle evidence subject to query.</param>
	/// <returns>The classified support state for <paramref name="subject"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="subject"/> is not a defined lifecycle evidence subject.
	/// </exception>
	public PersistentRasterLifecycleSupportStatus GetStatus(
		PersistentRasterLifecycleEvidenceSubject subject
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
			);
		}

		return subject switch {
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay =>
				RasterDisplay,
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload =>
				PersistentUpload,
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload =>
				AcknowledgedUpload,
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation =>
				PlacementCreation,
			PersistentRasterLifecycleEvidenceSubject.MultiplePlacements =>
				MultiplePlacements,
			PersistentRasterLifecycleEvidenceSubject.PlacementUpdate =>
				PlacementUpdate,
			PersistentRasterLifecycleEvidenceSubject.PlacementDeletion =>
				PlacementDeletion,
			PersistentRasterLifecycleEvidenceSubject.ResourceDeletion =>
				ResourceDeletion,
			_ => throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
			),
		};
	}
}
