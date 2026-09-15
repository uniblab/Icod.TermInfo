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
/// Deterministically integrates caller-owned persistent-raster runtime
/// observations into the frozen lifecycle and placement evidence models.
/// </summary>
public static class PersistentRasterRuntimeEvidenceIntegrator {
	/// <summary>
	/// Integrates conclusive runtime observations as existing <c>Verified</c>
	/// evidence, retains inconclusive observations for audit, and delegates final
	/// support classification to the frozen lifecycle and placement classifiers.
	/// </summary>
	/// <param name="lifecycleProfile">
	/// The existing classified lifecycle profile and complete evidence snapshot.
	/// </param>
	/// <param name="placementProfile">
	/// The existing classified placement profile and complete evidence snapshot.
	/// </param>
	/// <param name="observations">
	/// The immutable canonical caller-owned runtime-observation snapshot.
	/// </param>
	/// <returns>An immutable deterministic integration audit result.</returns>
	public static PersistentRasterRuntimeIntegrationResult Integrate(
		PersistentRasterLifecycleProfile lifecycleProfile,
		PersistentRasterPlacementProfile placementProfile,
		PersistentRasterRuntimeObservationSet observations
	) {
		ArgumentNullException.ThrowIfNull( lifecycleProfile );
		ArgumentNullException.ThrowIfNull( placementProfile );
		ArgumentNullException.ThrowIfNull( observations );

		PersistentRasterRuntimeLifecycleObservation[] conclusiveLifecycle =
			observations.LifecycleObservations
				.Where(
					item => item.Outcome
						!= PersistentRasterRuntimeObservationOutcome.Inconclusive
				)
				.ToArray();
		PersistentRasterRuntimePlacementObservation[] conclusivePlacement =
			observations.PlacementObservations
				.Where(
					item => item.Outcome
						!= PersistentRasterRuntimeObservationOutcome.Inconclusive
				)
				.ToArray();
		PersistentRasterRuntimeLifecycleObservation[] inconclusiveLifecycle =
			observations.LifecycleObservations
				.Where(
					item => item.Outcome
						== PersistentRasterRuntimeObservationOutcome.Inconclusive
				)
				.ToArray();
		PersistentRasterRuntimePlacementObservation[] inconclusivePlacement =
			observations.PlacementObservations
				.Where(
					item => item.Outcome
						== PersistentRasterRuntimeObservationOutcome.Inconclusive
				)
				.ToArray();

		(
			IReadOnlyList<PersistentRasterLifecycleEvidence> importedLifecycle,
			PersistentRasterLifecycleProfile resultingLifecycleProfile,
			PersistentRasterRuntimeIntegrationIssue? lifecycleIssue
		) = IntegrateLifecycleFamily(
			lifecycleProfile,
			conclusiveLifecycle
		);
		(
			IReadOnlyList<PersistentRasterPlacementEvidence> importedPlacement,
			PersistentRasterPlacementProfile resultingPlacementProfile,
			PersistentRasterRuntimeIntegrationIssue? placementIssue
		) = IntegratePlacementFamily(
			placementProfile,
			conclusivePlacement
		);

		List<PersistentRasterRuntimeIntegrationIssue> issues = [];
		if ( lifecycleIssue is not null ) {
			issues.Add( lifecycleIssue );
		}
		if ( placementIssue is not null ) {
			issues.Add( placementIssue );
		}

		return new PersistentRasterRuntimeIntegrationResult(
			observations,
			importedLifecycle,
			importedPlacement,
			inconclusiveLifecycle,
			inconclusivePlacement,
			resultingLifecycleProfile,
			resultingPlacementProfile,
			issues
		);
	}

	private static (
		IReadOnlyList<PersistentRasterLifecycleEvidence> ImportedEvidence,
		PersistentRasterLifecycleProfile Profile,
		PersistentRasterRuntimeIntegrationIssue? Issue
	) IntegrateLifecycleFamily(
		PersistentRasterLifecycleProfile profile,
		IReadOnlyList<PersistentRasterRuntimeLifecycleObservation> observations
	) {
		int existingCount = profile.Evidence.Count;
		int importCount = observations.Count;
		int maximumCount =
			PersistentRasterLifecycleEvidenceOptions.MaximumSupportedEvidenceCount;
		if ( importCount > maximumCount - existingCount ) {
			return (
				Array.Empty<PersistentRasterLifecycleEvidence>(),
				profile,
				new PersistentRasterRuntimeIntegrationIssue(
					PersistentRasterRuntimeIntegrationIssueKind
						.LifecycleEvidenceCapacityExhausted,
					existingCount,
					importCount
				)
			);
		}
		if ( importCount == 0 ) {
			return (
				Array.Empty<PersistentRasterLifecycleEvidence>(),
				profile,
				null
			);
		}

		int firstImportedOrdinal = 0;
		if ( existingCount > 0 ) {
			int maximumExistingOrdinal =
				profile.Evidence.Max( item => item.SourceOrdinal );
			long availableOrdinalCount =
				(long)int.MaxValue - maximumExistingOrdinal;
			if ( importCount > availableOrdinalCount ) {
				return (
					Array.Empty<PersistentRasterLifecycleEvidence>(),
					profile,
					new PersistentRasterRuntimeIntegrationIssue(
						PersistentRasterRuntimeIntegrationIssueKind
							.LifecycleOrdinalSpaceExhausted,
						existingCount,
						importCount
					)
				);
			}
			firstImportedOrdinal = maximumExistingOrdinal + 1;
		}

		PersistentRasterLifecycleEvidence[] imported =
			new PersistentRasterLifecycleEvidence[ importCount ];
		for ( int index = 0; index < observations.Count; index++ ) {
			PersistentRasterRuntimeLifecycleObservation observation =
				observations[ index ];
			imported[ index ] = new PersistentRasterLifecycleEvidence(
				observation.Subject,
				observation.Outcome
					== PersistentRasterRuntimeObservationOutcome.Supported,
				PersistentRasterLifecycleEvidenceKind.Verified,
				observation.SourceLabel,
				checked( firstImportedOrdinal + index )
			);
		}

		PersistentRasterLifecycleProfile resultingProfile =
			PersistentRasterLifecycleClassifier.Classify(
				profile.Evidence.Concat( imported ),
				new PersistentRasterLifecycleEvidenceOptions( maximumCount )
			);
		return (
			Array.AsReadOnly( imported ),
			resultingProfile,
			null
		);
	}

	private static (
		IReadOnlyList<PersistentRasterPlacementEvidence> ImportedEvidence,
		PersistentRasterPlacementProfile Profile,
		PersistentRasterRuntimeIntegrationIssue? Issue
	) IntegratePlacementFamily(
		PersistentRasterPlacementProfile profile,
		IReadOnlyList<PersistentRasterRuntimePlacementObservation> observations
	) {
		int existingCount = profile.Evidence.Count;
		int importCount = observations.Count;
		int maximumCount =
			PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount;
		if ( importCount > maximumCount - existingCount ) {
			return (
				Array.Empty<PersistentRasterPlacementEvidence>(),
				profile,
				new PersistentRasterRuntimeIntegrationIssue(
					PersistentRasterRuntimeIntegrationIssueKind
						.PlacementEvidenceCapacityExhausted,
					existingCount,
					importCount
				)
			);
		}
		if ( importCount == 0 ) {
			return (
				Array.Empty<PersistentRasterPlacementEvidence>(),
				profile,
				null
			);
		}

		int firstImportedOrdinal = 0;
		if ( existingCount > 0 ) {
			int maximumExistingOrdinal =
				profile.Evidence.Max( item => item.SourceOrdinal );
			long availableOrdinalCount =
				(long)int.MaxValue - maximumExistingOrdinal;
			if ( importCount > availableOrdinalCount ) {
				return (
					Array.Empty<PersistentRasterPlacementEvidence>(),
					profile,
					new PersistentRasterRuntimeIntegrationIssue(
						PersistentRasterRuntimeIntegrationIssueKind
							.PlacementOrdinalSpaceExhausted,
						existingCount,
						importCount
					)
				);
			}
			firstImportedOrdinal = maximumExistingOrdinal + 1;
		}

		PersistentRasterPlacementEvidence[] imported =
			new PersistentRasterPlacementEvidence[ importCount ];
		for ( int index = 0; index < observations.Count; index++ ) {
			PersistentRasterRuntimePlacementObservation observation =
				observations[ index ];
			imported[ index ] = new PersistentRasterPlacementEvidence(
				observation.Subject,
				observation.Outcome
					== PersistentRasterRuntimeObservationOutcome.Supported,
				PersistentRasterPlacementEvidenceKind.Verified,
				observation.SourceLabel,
				checked( firstImportedOrdinal + index )
			);
		}

		PersistentRasterPlacementProfile resultingProfile =
			PersistentRasterPlacementClassifier.Classify(
				profile.Evidence.Concat( imported ),
				new PersistentRasterPlacementEvidenceOptions( maximumCount )
			);
		return (
			Array.AsReadOnly( imported ),
			resultingProfile,
			null
		);
	}
}
