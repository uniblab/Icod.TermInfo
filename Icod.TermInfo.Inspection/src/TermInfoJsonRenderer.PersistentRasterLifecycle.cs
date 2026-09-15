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
	/// The additive schema identifier used by 1.11 persistent-raster lifecycle
	/// automation. Version-1 and version-2 documents retain their frozen schema
	/// identifiers.
	/// </summary>
	public const string PersistentRasterLifecycleSchemaIdentifier =
		"urn:icod:terminfo:inspection:json:3";

	/// <summary>
	/// The additive schema version used by 1.11 persistent-raster lifecycle
	/// automation.
	/// </summary>
	public const int PersistentRasterLifecycleSchemaVersion = 3;

	private const string PersistentRasterLifecycleProfileDocumentKind =
		"persistentRasterLifecycleProfile";
	private const string PersistentRasterLifecyclePlanDocumentKind =
		"persistentRasterLifecyclePlan";

	/// <summary>
	/// Renders one persistent-raster lifecycle profile using the additive version-3
	/// automation contract.
	/// </summary>
	/// <param name="profile">The immutable classified lifecycle profile.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterLifecycleProfile profile
	) =>
		Render(
			profile,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one persistent-raster lifecycle profile using explicit deterministic
	/// JSON policy.
	/// </summary>
	/// <param name="profile">The immutable classified lifecycle profile.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterLifecycleProfile profile,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( profile );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterLifecycleProfileV3(
			profile,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders one persistent-raster lifecycle plan using the additive version-3
	/// automation contract.
	/// </summary>
	/// <param name="plan">The immutable semantic lifecycle plan.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterLifecyclePlan plan
	) =>
		Render(
			plan,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one persistent-raster lifecycle plan using explicit deterministic
	/// JSON policy.
	/// </summary>
	/// <param name="plan">The immutable semantic lifecycle plan.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterLifecyclePlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterLifecyclePlanV3(
			plan,
			options,
			cancellationToken
		);
	}

	private static string RenderPersistentRasterLifecycleProfileV3(
		PersistentRasterLifecycleProfile profile,
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
			WritePersistentRasterLifecycleEnvelopePrefix(
				writer,
				PersistentRasterLifecycleProfileDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteStartObject( "states" );
			writer.WriteString(
				"rasterDisplay",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.RasterDisplay
				)
			);
			writer.WriteString(
				"persistentUpload",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.PersistentUpload
				)
			);
			writer.WriteString(
				"acknowledgedUpload",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.AcknowledgedUpload
				)
			);
			writer.WriteString(
				"placementCreation",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.PlacementCreation
				)
			);
			writer.WriteString(
				"multiplePlacements",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.MultiplePlacements
				)
			);
			writer.WriteString(
				"placementUpdate",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.PlacementUpdate
				)
			);
			writer.WriteString(
				"placementDeletion",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.PlacementDeletion
				)
			);
			writer.WriteString(
				"resourceDeletion",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.ResourceDeletion
				)
			);
			writer.WriteEndObject();
			writer.WriteNumber( "evidenceCount", profile.Evidence.Count );
			writer.WriteStartArray( "evidence" );
			foreach ( PersistentRasterLifecycleEvidence evidence in profile.Evidence ) {
				cancellationToken.ThrowIfCancellationRequested();
				writer.WriteStartObjectValue();
				writer.WriteString(
					"subject",
					GetPersistentRasterLifecycleEvidenceSubjectName(
						evidence.Subject
					)
				);
				writer.WriteBoolean( "isPositive", evidence.IsPositive );
				writer.WriteString(
					"kind",
					GetPersistentRasterLifecycleEvidenceKindName(
						evidence.Kind
					)
				);
				writer.WriteString( "sourceLabel", evidence.SourceLabel );
				writer.WriteNumber( "sourceOrdinal", evidence.SourceOrdinal );
				writer.WriteEndObject();
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

	private static string RenderPersistentRasterLifecyclePlanV3(
		PersistentRasterLifecyclePlan plan,
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
			WritePersistentRasterLifecycleEnvelopePrefix(
				writer,
				PersistentRasterLifecyclePlanDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteString(
				"status",
				GetPersistentRasterLifecyclePlanStatusName(
					plan.Status
				)
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
					GetPersistentRasterLifecycleOperationName(
						step.Operation
					)
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
					GetPersistentRasterLifecycleOperationName(
						issue.Operation
					)
				);
				writer.WriteString(
					"subject",
					GetPersistentRasterLifecycleEvidenceSubjectName(
						issue.Subject
					)
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

	private static void WritePersistentRasterLifecycleEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( documentKind );

		writer.WriteString(
			"schema",
			PersistentRasterLifecycleSchemaIdentifier
		);
		writer.WriteNumber(
			"schemaVersion",
			PersistentRasterLifecycleSchemaVersion
		);
		writer.WriteString( "documentKind", documentKind );
	}

	private static string GetPersistentRasterLifecycleSupportStatusName(
		PersistentRasterLifecycleSupportStatus status
	) =>
		status switch {
			PersistentRasterLifecycleSupportStatus.Unknown => "unknown",
			PersistentRasterLifecycleSupportStatus.Supported => "supported",
			PersistentRasterLifecycleSupportStatus.Unsupported => "unsupported",
			PersistentRasterLifecycleSupportStatus.Contradicted => "contradicted",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The lifecycle support status must be a defined value."
			),
		};

	private static string GetPersistentRasterLifecycleEvidenceSubjectName(
		PersistentRasterLifecycleEvidenceSubject subject
	) =>
		subject switch {
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay =>
				"rasterDisplay",
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload =>
				"persistentUpload",
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload =>
				"acknowledgedUpload",
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation =>
				"placementCreation",
			PersistentRasterLifecycleEvidenceSubject.MultiplePlacements =>
				"multiplePlacements",
			PersistentRasterLifecycleEvidenceSubject.PlacementUpdate =>
				"placementUpdate",
			PersistentRasterLifecycleEvidenceSubject.PlacementDeletion =>
				"placementDeletion",
			PersistentRasterLifecycleEvidenceSubject.ResourceDeletion =>
				"resourceDeletion",
			_ => throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
			),
		};

	private static string GetPersistentRasterLifecycleEvidenceKindName(
		PersistentRasterLifecycleEvidenceKind kind
	) =>
		kind switch {
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived =>
				"capabilityDerived",
			PersistentRasterLifecycleEvidenceKind.Declared => "declared",
			PersistentRasterLifecycleEvidenceKind.Verified => "verified",
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The lifecycle evidence kind must be a defined value."
			),
		};

	private static string GetPersistentRasterLifecycleOperationName(
		PersistentRasterLifecycleOperation operation
	) =>
		operation switch {
			PersistentRasterLifecycleOperation.DisplayEphemeral =>
				"displayEphemeral",
			PersistentRasterLifecycleOperation.UploadResource => "uploadResource",
			PersistentRasterLifecycleOperation.CreatePlacement => "createPlacement",
			PersistentRasterLifecycleOperation.UpdatePlacement => "updatePlacement",
			PersistentRasterLifecycleOperation.DeletePlacement => "deletePlacement",
			PersistentRasterLifecycleOperation.DeleteResource => "deleteResource",
			_ => throw new ArgumentOutOfRangeException(
				nameof( operation ),
				operation,
				"The lifecycle operation must be a defined value."
			),
		};

	private static string GetPersistentRasterLifecyclePlanStatusName(
		PersistentRasterLifecyclePlanStatus status
	) =>
		status switch {
			PersistentRasterLifecyclePlanStatus.Success => "success",
			PersistentRasterLifecyclePlanStatus.Indeterminate => "indeterminate",
			PersistentRasterLifecyclePlanStatus.Impossible => "impossible",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The lifecycle plan status must be a defined value."
			),
		};
}
