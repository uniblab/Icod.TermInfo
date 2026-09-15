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
/// Derives deterministic protocol-neutral persistent-raster lifecycle evidence
/// from explicit terminal-description semantics and frozen database-set
/// precedence.
/// </summary>
public static class PersistentRasterLifecycleInspector {
	/// <summary>
	/// The Icod-owned extended Boolean capability declaring ordinary raster display.
	/// </summary>
	public const string RasterDisplayCapabilityName = "IcodRasterDisplay";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring persistent raster upload.
	/// </summary>
	public const string PersistentUploadCapabilityName =
		"IcodPersistentRasterUpload";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring acknowledged persistent
	/// raster upload.
	/// </summary>
	public const string AcknowledgedUploadCapabilityName =
		"IcodPersistentRasterAcknowledgedUpload";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring placement creation.
	/// </summary>
	public const string PlacementCreationCapabilityName =
		"IcodPersistentRasterPlacement";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring multiple placements of
	/// one persistent resource.
	/// </summary>
	public const string MultiplePlacementsCapabilityName =
		"IcodPersistentRasterMultiplePlacements";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring placement update.
	/// </summary>
	public const string PlacementUpdateCapabilityName =
		"IcodPersistentRasterPlacementUpdate";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring placement deletion.
	/// </summary>
	public const string PlacementDeletionCapabilityName =
		"IcodPersistentRasterPlacementDeletion";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring persistent resource
	/// deletion.
	/// </summary>
	public const string ResourceDeletionCapabilityName =
		"IcodPersistentRasterResourceDeletion";

	private static readonly SemanticCapability[] SemanticCapabilities = [
		new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			RasterDisplayCapabilityName,
			0
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			PersistentUploadCapabilityName,
			1
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			AcknowledgedUploadCapabilityName,
			2
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
			PlacementCreationCapabilityName,
			3
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
			MultiplePlacementsCapabilityName,
			4
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
			PlacementUpdateCapabilityName,
			5
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
			PlacementDeletionCapabilityName,
			6
		),
		new(
			PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
			ResourceDeletionCapabilityName,
			7
		),
	];

	/// <summary>
	/// Inspects explicit lifecycle semantics present on one terminal description.
	/// </summary>
	/// <param name="description">The immutable terminal description to inspect.</param>
	/// <param name="options">Optional deterministic evidence bounds.</param>
	/// <returns>An immutable classified lifecycle profile.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// A reserved lifecycle semantic capability exists with a non-Boolean value.
	/// </exception>
	public static PersistentRasterLifecycleProfile Inspect(
		TerminalDescription description,
		PersistentRasterLifecycleEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( description );

		List<PersistentRasterLifecycleEvidence> evidence = [];
		foreach ( SemanticCapability semanticCapability in SemanticCapabilities ) {
			if (
				!description.ExtendedCapabilities.TryGetValue(
					semanticCapability.CapabilityName,
					out TermInfoCapabilityValue value
				)
			) {
				continue;
			}
			if ( !value.IsBoolean ) {
				throw new InvalidOperationException(
					$"Extended capability '{semanticCapability.CapabilityName}' on terminal '{description.Name}' must be Boolean to declare persistent-raster lifecycle semantics."
				);
			}

			evidence.Add(
				new PersistentRasterLifecycleEvidence(
					semanticCapability.Subject,
					value.BooleanValue,
					PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
					semanticCapability.CapabilityName,
					semanticCapability.SourceOrdinal
				)
			);
		}

		return PersistentRasterLifecycleClassifier.Classify(
			evidence,
			options
		);
	}

	/// <summary>
	/// Inspects one canonical terminal identity through the frozen 1.10 ordered
	/// database-set precedence model.
	/// </summary>
	/// <param name="databaseSet">The immutable ordered database set.</param>
	/// <param name="canonicalName">The exact canonical terminal identity.</param>
	/// <param name="options">Optional deterministic evidence bounds.</param>
	/// <returns>
	/// Database-set lookup provenance together with every observed occurrence
	/// profile and, only when precedence is conclusive, the effective profile.
	/// </returns>
	public static PersistentRasterLifecycleDatabaseSetInspection Inspect(
		TermInfoDatabaseSet databaseSet,
		string canonicalName,
		PersistentRasterLifecycleEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentException.ThrowIfNullOrWhiteSpace( canonicalName );

		TermInfoDatabaseSetLookupResult lookup =
			databaseSet.LookupCanonicalName( canonicalName );
		PersistentRasterLifecycleProfile[] occurrenceProfiles =
			new PersistentRasterLifecycleProfile[ lookup.Occurrences.Count ];
		for ( int index = 0; index < occurrenceProfiles.Length; index++ ) {
			occurrenceProfiles[ index ] = Inspect(
				lookup.Occurrences[ index ].Entry.Terminal,
				options
			);
		}

		PersistentRasterLifecycleProfile? effectiveProfile = null;
		if ( lookup.Status == TermInfoDatabaseSetLookupStatus.WinnerKnown ) {
			if ( lookup.Winner is null || occurrenceProfiles.Length == 0 ) {
				throw new InvalidOperationException(
					"A known database-set winner must provide an observed lifecycle profile."
				);
			}
			effectiveProfile = occurrenceProfiles[ 0 ];
		}

		return new PersistentRasterLifecycleDatabaseSetInspection(
			databaseSet,
			lookup,
			occurrenceProfiles,
			effectiveProfile
		);
	}

	private readonly record struct SemanticCapability(
		PersistentRasterLifecycleEvidenceSubject Subject,
		string CapabilityName,
		int SourceOrdinal
	);
}
