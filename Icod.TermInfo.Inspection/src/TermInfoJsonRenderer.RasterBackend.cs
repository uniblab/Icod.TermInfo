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

public static partial class TermInfoJsonRenderer {
	/// <summary>
	/// The additive schema identifier used by 1.14 raster-backend profile and
	/// selection-plan automation. Versions 1 through 5 retain their frozen
	/// identifiers.
	/// </summary>
	public const string RasterBackendSchemaIdentifier =
		"urn:icod:terminfo:inspection:json:6";

	/// <summary>
	/// The additive schema version used by 1.14 raster-backend automation.
	/// </summary>
	public const int RasterBackendSchemaVersion = 6;

	private const string RasterBackendProfileDocumentKind =
		"rasterBackendProfile";
	private const string RasterBackendSelectionPlanDocumentKind =
		"rasterBackendSelectionPlan";

	/// <summary>
	/// Renders one raster-backend availability profile using the additive version-6
	/// automation contract.
	/// </summary>
	/// <param name="profile">The immutable classified backend profile.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		RasterBackendProfile profile
	) =>
		Render(
			profile,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one raster-backend availability profile using explicit deterministic
	/// JSON policy.
	/// </summary>
	/// <param name="profile">The immutable classified backend profile.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		RasterBackendProfile profile,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( profile );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderRasterBackendProfileV6(
			profile,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders one deterministic raster-backend selection plan using the additive
	/// version-6 automation contract.
	/// </summary>
	/// <param name="plan">The immutable backend selection plan.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		RasterBackendSelectionPlan plan
	) =>
		Render(
			plan,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one deterministic raster-backend selection plan using explicit
	/// deterministic JSON policy.
	/// </summary>
	/// <param name="plan">The immutable backend selection plan.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		RasterBackendSelectionPlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderRasterBackendSelectionPlanV6(
			plan,
			options,
			cancellationToken
		);
	}

	private static string RenderRasterBackendProfileV6(
		RasterBackendProfile profile,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( profile );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WriteRasterBackendEnvelopePrefix(
				writer,
				RasterBackendProfileDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteString(
				"backend",
				GetRasterBackendKindName( profile.Backend )
			);
			writer.WriteString(
				"status",
				GetRasterBackendSupportStatusName( profile.Status )
			);
			writer.WriteNumber( "evidenceCount", profile.Evidence.Count );
			WriteRasterBackendEvidence(
				writer,
				"evidence",
				profile.Evidence,
				cancellationToken
			);
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException(
				options,
				exception
			);
		}

		return output.GetString();
	}

	private static string RenderRasterBackendSelectionPlanV6(
		RasterBackendSelectionPlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WriteRasterBackendEnvelopePrefix(
				writer,
				RasterBackendSelectionPlanDocumentKind
			);
			writer.WriteStartObject( "data" );
			WriteRasterBackendSelectionRequest(
				writer,
				plan.Request
			);
			WriteRasterBackendPreferenceOrder(
				writer,
				plan.Options.PreferenceOrder,
				cancellationToken
			);
			writer.WriteString(
				"status",
				GetRasterBackendSelectionStatusName( plan.Status )
			);
			if ( plan.SelectedBackend.HasValue ) {
				writer.WriteString(
					"selectedBackend",
					GetRasterBackendKindName( plan.SelectedBackend.Value )
				);
			} else {
				writer.WriteNull( "selectedBackend" );
			}
			string? blocker = GetRasterBackendSelectionBlockerName( plan.Status );
			if ( blocker is null ) {
				writer.WriteNull( "remainingBlocker" );
			} else {
				writer.WriteString( "remainingBlocker", blocker );
			}
			writer.WriteNumber(
				"candidateEvaluationCount",
				plan.CandidateEvaluations.Count
			);
			writer.WriteStartArray( "candidateEvaluations" );
			foreach (
				RasterBackendCandidateEvaluation evaluation
					in plan.CandidateEvaluations
			) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteRasterBackendCandidateEvaluation(
					writer,
					evaluation,
					cancellationToken
				);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException(
				options,
				exception
			);
		}

		return output.GetString();
	}

	private static void WriteRasterBackendEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( documentKind );

		writer.WriteString( "schema", RasterBackendSchemaIdentifier );
		writer.WriteNumber( "schemaVersion", RasterBackendSchemaVersion );
		writer.WriteString( "documentKind", documentKind );
	}

	private static void WriteRasterBackendEvidence(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<RasterBackendEvidence> evidence,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );
		ArgumentNullException.ThrowIfNull( evidence );

		writer.WriteStartArray( propertyName );
		foreach ( RasterBackendEvidence item in evidence ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"backend",
				GetRasterBackendKindName( item.Backend )
			);
			writer.WriteBoolean( "isPositive", item.IsPositive );
			writer.WriteString(
				"kind",
				GetRasterBackendEvidenceKindName( item.Kind )
			);
			writer.WriteString( "sourceLabel", item.SourceLabel );
			writer.WriteNumber( "sourceOrdinal", item.SourceOrdinal );
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WriteRasterBackendSelectionRequest(
		DeterministicJsonWriter writer,
		RasterBackendSelectionRequest request
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( request );

		writer.WriteStartObject( "request" );
		writer.WriteStartObject( "lifecycle" );
		writer.WriteBoolean(
			"displayEphemeral",
			request.LifecycleRequest.DisplayEphemeral
		);
		writer.WriteBoolean(
			"uploadResource",
			request.LifecycleRequest.UploadResource
		);
		writer.WriteNumber(
			"placementCount",
			request.LifecycleRequest.PlacementCount
		);
		writer.WriteBoolean(
			"updatePlacement",
			request.LifecycleRequest.UpdatePlacement
		);
		writer.WriteBoolean(
			"deletePlacement",
			request.LifecycleRequest.DeletePlacement
		);
		writer.WriteBoolean(
			"deleteResource",
			request.LifecycleRequest.DeleteResource
		);
		writer.WriteBoolean(
			"requireAcknowledgedUpload",
			request.LifecycleRequest.RequireAcknowledgedUpload
		);
		writer.WriteEndObject();
		if ( request.PlacementRequest is null ) {
			writer.WriteNull( "placement" );
		} else {
			writer.WriteStartObject( "placement" );
			writer.WriteBoolean(
				"requireSourceRectangle",
				request.PlacementRequest.RequireSourceRectangle
			);
			writer.WriteBoolean(
				"requireSignedZOrder",
				request.PlacementRequest.RequireSignedZOrder
			);
			writer.WriteEndObject();
		}
		writer.WriteEndObject();
	}

	private static void WriteRasterBackendPreferenceOrder(
		DeterministicJsonWriter writer,
		IReadOnlyList<RasterBackendKind> preferenceOrder,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( preferenceOrder );

		writer.WriteStartArray( "preferenceOrder" );
		foreach ( RasterBackendKind backend in preferenceOrder ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStringValue( GetRasterBackendKindName( backend ) );
		}
		writer.WriteEndArray();
	}

	private static void WriteRasterBackendCandidateEvaluation(
		DeterministicJsonWriter writer,
		RasterBackendCandidateEvaluation evaluation,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( evaluation );

		writer.WriteStartObjectValue();
		writer.WriteString(
			"backend",
			GetRasterBackendKindName(
				evaluation.Candidate.BackendProfile.Backend
			)
		);
		writer.WriteString(
			"backendStatus",
			GetRasterBackendSupportStatusName(
				evaluation.Candidate.BackendProfile.Status
			)
		);
		writer.WriteString(
			"status",
			GetRasterBackendCandidateStatusName( evaluation.Status )
		);
		WriteRasterBackendLifecyclePlan(
			writer,
			evaluation.LifecyclePlan,
			cancellationToken
		);
		if ( evaluation.PlacementPlan is null ) {
			writer.WriteNull( "placementPlan" );
		} else {
			WriteRasterBackendPlacementPlan(
				writer,
				evaluation.PlacementPlan,
				cancellationToken
			);
		}
		writer.WriteEndObject();
	}

	private static void WriteRasterBackendLifecyclePlan(
		DeterministicJsonWriter writer,
		PersistentRasterLifecyclePlan plan,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( plan );

		writer.WriteStartObject( "lifecyclePlan" );
		writer.WriteString(
			"status",
			GetPersistentRasterLifecyclePlanStatusName( plan.Status )
		);
		writer.WriteBoolean(
			"requiresRuntimeVerification",
			plan.RequiresRuntimeVerification
		);
		writer.WriteNumber( "stepCount", plan.Steps.Count );
		writer.WriteNumber( "issueCount", plan.Issues.Count );
		writer.WriteStartArray( "steps" );
		foreach ( PersistentRasterLifecyclePlanStep step in plan.Steps ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteNumber( "sequenceIndex", step.SequenceIndex );
			writer.WriteString(
				"operation",
				GetPersistentRasterLifecycleOperationName( step.Operation )
			);
			writer.WriteBoolean(
				"requiresRuntimeVerification",
				step.RequiresRuntimeVerification
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteStartArray( "issues" );
		foreach ( PersistentRasterLifecyclePlanIssue issue in plan.Issues ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"operation",
				GetPersistentRasterLifecycleOperationName( issue.Operation )
			);
			writer.WriteString(
				"subject",
				GetPersistentRasterLifecycleEvidenceSubjectName( issue.Subject )
			);
			writer.WriteString(
				"supportStatus",
				GetPersistentRasterLifecycleSupportStatusName(
					issue.SupportStatus
				)
			);
			writer.WriteBoolean(
				"requiresRuntimeVerification",
				issue.RequiresRuntimeVerification
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	private static void WriteRasterBackendPlacementPlan(
		DeterministicJsonWriter writer,
		PersistentRasterPlacementPlan plan,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( plan );

		writer.WriteStartObject( "placementPlan" );
		writer.WriteString(
			"status",
			GetPersistentRasterPlacementPlanStatusName( plan.Status )
		);
		writer.WriteString(
			"lifecycleStatus",
			GetPersistentRasterLifecyclePlanStatusName( plan.LifecycleStatus )
		);
		writer.WriteBoolean(
			"requiresRuntimeVerification",
			plan.Status
				== PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification
		);
		writer.WriteNumber( "requirementCount", plan.Requirements.Count );
		writer.WriteNumber( "issueCount", plan.Issues.Count );
		writer.WriteStartArray( "requirements" );
		foreach (
			PersistentRasterPlacementPlanRequirement requirement
				in plan.Requirements
		) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterPlacementSubjectName( requirement.Subject )
			);
			writer.WriteString(
				"supportStatus",
				GetPersistentRasterLifecycleSupportStatusName(
					requirement.SupportStatus
				)
			);
			writer.WriteBoolean(
				"requiresRuntimeVerification",
				requirement.RequiresRuntimeVerification
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteStartArray( "issues" );
		foreach ( PersistentRasterPlacementPlanIssue issue in plan.Issues ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterPlacementSubjectName( issue.Subject )
			);
			writer.WriteString(
				"supportStatus",
				GetPersistentRasterLifecycleSupportStatusName(
					issue.SupportStatus
				)
			);
			writer.WriteBoolean(
				"requiresRuntimeVerification",
				issue.RequiresRuntimeVerification
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	private static string GetRasterBackendKindName(
		RasterBackendKind backend
	) =>
		backend switch {
			RasterBackendKind.Sixel => "sixel",
			RasterBackendKind.KittyGraphics => "kittyGraphics",
			_ => throw new ArgumentOutOfRangeException(
				nameof( backend ),
				backend,
				"The raster backend must be a defined value."
			),
		};

	private static string GetRasterBackendEvidenceKindName(
		RasterBackendEvidenceKind kind
	) =>
		kind switch {
			RasterBackendEvidenceKind.CapabilityDerived => "capabilityDerived",
			RasterBackendEvidenceKind.Declared => "declared",
			RasterBackendEvidenceKind.Verified => "verified",
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The raster-backend evidence kind must be a defined value."
			),
		};

	private static string GetRasterBackendSupportStatusName(
		RasterBackendSupportStatus status
	) =>
		status switch {
			RasterBackendSupportStatus.Unknown => "unknown",
			RasterBackendSupportStatus.Supported => "supported",
			RasterBackendSupportStatus.Unsupported => "unsupported",
			RasterBackendSupportStatus.Contradicted => "contradicted",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The raster-backend support status must be a defined value."
			),
		};

	private static string GetRasterBackendCandidateStatusName(
		RasterBackendCandidateStatus status
	) =>
		status switch {
			RasterBackendCandidateStatus.Satisfied => "satisfied",
			RasterBackendCandidateStatus.RequiresRuntimeVerification =>
				"requiresRuntimeVerification",
			RasterBackendCandidateStatus.Impossible => "impossible",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The raster-backend candidate status must be a defined value."
			),
		};

	private static string GetRasterBackendSelectionStatusName(
		RasterBackendSelectionStatus status
	) =>
		status switch {
			RasterBackendSelectionStatus.Selected => "selected",
			RasterBackendSelectionStatus.RequiresRuntimeVerification =>
				"requiresRuntimeVerification",
			RasterBackendSelectionStatus.RequiresPreference => "requiresPreference",
			RasterBackendSelectionStatus.Impossible => "impossible",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The raster-backend selection status must be a defined value."
			),
		};

	private static string? GetRasterBackendSelectionBlockerName(
		RasterBackendSelectionStatus status
	) =>
		status switch {
			RasterBackendSelectionStatus.Selected => null,
			RasterBackendSelectionStatus.RequiresRuntimeVerification =>
				"runtimeVerification",
			RasterBackendSelectionStatus.RequiresPreference => "preference",
			RasterBackendSelectionStatus.Impossible => "impossibility",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The raster-backend selection status must be a defined value."
			),
		};
}
