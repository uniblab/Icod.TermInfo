namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	/// <summary>
	/// The additive schema identifier used by 1.13 persistent-raster runtime
	/// evidence automation. Versions 1 through 4 retain their frozen identifiers.
	/// </summary>
	public const string PersistentRasterRuntimeSchemaIdentifier =
		"urn:icod:terminfo:inspection:json:5";

	/// <summary>
	/// The additive schema version used by 1.13 persistent-raster runtime evidence
	/// automation.
	/// </summary>
	public const int PersistentRasterRuntimeSchemaVersion = 5;

	private const string PersistentRasterRuntimeObservationSetDocumentKind =
		"persistentRasterRuntimeObservationSet";
	private const string PersistentRasterRuntimeIntegrationDocumentKind =
		"persistentRasterRuntimeIntegration";

	/// <summary>
	/// Renders one immutable canonical persistent-raster runtime-observation set
	/// using the additive version-5 automation contract.
	/// </summary>
	/// <param name="observations">The immutable canonical observation snapshot.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterRuntimeObservationSet observations
	) =>
		Render(
			observations,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one immutable canonical persistent-raster runtime-observation set
	/// using explicit deterministic JSON policy.
	/// </summary>
	/// <param name="observations">The immutable canonical observation snapshot.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterRuntimeObservationSet observations,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( observations );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterRuntimeObservationSetV5(
			observations,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders one immutable persistent-raster runtime integration audit result
	/// using the additive version-5 automation contract.
	/// </summary>
	/// <param name="integration">The immutable runtime integration audit result.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterRuntimeIntegrationResult integration
	) =>
		Render(
			integration,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one immutable persistent-raster runtime integration audit result
	/// using explicit deterministic JSON policy.
	/// </summary>
	/// <param name="integration">The immutable runtime integration audit result.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterRuntimeIntegrationResult integration,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( integration );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterRuntimeIntegrationV5(
			integration,
			options,
			cancellationToken
		);
	}

	private static string RenderPersistentRasterRuntimeObservationSetV5(
		PersistentRasterRuntimeObservationSet observations,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( observations );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WritePersistentRasterRuntimeEnvelopePrefix(
				writer,
				PersistentRasterRuntimeObservationSetDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteNumber( "observationCount", observations.Count );
			writer.WriteNumber(
				"lifecycleObservationCount",
				observations.LifecycleObservations.Count
			);
			writer.WriteNumber(
				"placementObservationCount",
				observations.PlacementObservations.Count
			);
			WritePersistentRasterRuntimeLifecycleObservations(
				writer,
				"lifecycleObservations",
				observations.LifecycleObservations,
				cancellationToken
			);
			WritePersistentRasterRuntimePlacementObservations(
				writer,
				"placementObservations",
				observations.PlacementObservations,
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

	private static string RenderPersistentRasterRuntimeIntegrationV5(
		PersistentRasterRuntimeIntegrationResult integration,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( integration );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		BoundedJsonOutput output = new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer = new( output, options.WriteIndented );
		try {
			writer.WriteStartObject();
			WritePersistentRasterRuntimeEnvelopePrefix(
				writer,
				PersistentRasterRuntimeIntegrationDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteBoolean( "succeeded", integration.Succeeded );
			writer.WriteNumber(
				"observationCount",
				integration.Observations.Count
			);
			writer.WriteStartObject( "observations" );
			WritePersistentRasterRuntimeLifecycleObservations(
				writer,
				"lifecycle",
				integration.Observations.LifecycleObservations,
				cancellationToken
			);
			WritePersistentRasterRuntimePlacementObservations(
				writer,
				"placement",
				integration.Observations.PlacementObservations,
				cancellationToken
			);
			writer.WriteEndObject();
			WritePersistentRasterRuntimeLifecycleEvidence(
				writer,
				"importedLifecycleEvidence",
				integration.ImportedLifecycleEvidence,
				cancellationToken
			);
			WritePersistentRasterRuntimePlacementEvidence(
				writer,
				"importedPlacementEvidence",
				integration.ImportedPlacementEvidence,
				cancellationToken
			);
			WritePersistentRasterRuntimeLifecycleObservations(
				writer,
				"inconclusiveLifecycleObservations",
				integration.InconclusiveLifecycleObservations,
				cancellationToken
			);
			WritePersistentRasterRuntimePlacementObservations(
				writer,
				"inconclusivePlacementObservations",
				integration.InconclusivePlacementObservations,
				cancellationToken
			);
			WritePersistentRasterRuntimeIntegrationIssues(
				writer,
				integration.Issues,
				cancellationToken
			);
			WritePersistentRasterRuntimeLifecycleStates(
				writer,
				integration.LifecycleProfile
			);
			WritePersistentRasterRuntimePlacementStates(
				writer,
				integration.PlacementProfile
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

	private static void WritePersistentRasterRuntimeEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( documentKind );

		writer.WriteString(
			"schema",
			PersistentRasterRuntimeSchemaIdentifier
		);
		writer.WriteNumber(
			"schemaVersion",
			PersistentRasterRuntimeSchemaVersion
		);
		writer.WriteString( "documentKind", documentKind );
	}

	private static void WritePersistentRasterRuntimeLifecycleObservations(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<PersistentRasterRuntimeLifecycleObservation> observations,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );
		ArgumentNullException.ThrowIfNull( observations );

		writer.WriteStartArray( propertyName );
		foreach ( PersistentRasterRuntimeLifecycleObservation observation in observations ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterLifecycleEvidenceSubjectName(
					observation.Subject
				)
			);
			writer.WriteString(
				"outcome",
				GetPersistentRasterRuntimeObservationOutcomeName(
					observation.Outcome
				)
			);
			writer.WriteString( "sourceLabel", observation.SourceLabel );
			writer.WriteNumber( "sourceOrdinal", observation.SourceOrdinal );
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WritePersistentRasterRuntimePlacementObservations(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<PersistentRasterRuntimePlacementObservation> observations,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );
		ArgumentNullException.ThrowIfNull( observations );

		writer.WriteStartArray( propertyName );
		foreach ( PersistentRasterRuntimePlacementObservation observation in observations ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterPlacementSubjectName(
					observation.Subject
				)
			);
			writer.WriteString(
				"outcome",
				GetPersistentRasterRuntimeObservationOutcomeName(
					observation.Outcome
				)
			);
			writer.WriteString( "sourceLabel", observation.SourceLabel );
			writer.WriteNumber( "sourceOrdinal", observation.SourceOrdinal );
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WritePersistentRasterRuntimeLifecycleEvidence(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<PersistentRasterLifecycleEvidence> evidence,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );
		ArgumentNullException.ThrowIfNull( evidence );

		writer.WriteStartArray( propertyName );
		foreach ( PersistentRasterLifecycleEvidence item in evidence ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterLifecycleEvidenceSubjectName( item.Subject )
			);
			writer.WriteBoolean( "isPositive", item.IsPositive );
			writer.WriteString(
				"kind",
				GetPersistentRasterLifecycleEvidenceKindName( item.Kind )
			);
			writer.WriteString( "sourceLabel", item.SourceLabel );
			writer.WriteNumber( "sourceOrdinal", item.SourceOrdinal );
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WritePersistentRasterRuntimePlacementEvidence(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<PersistentRasterPlacementEvidence> evidence,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );
		ArgumentNullException.ThrowIfNull( evidence );

		writer.WriteStartArray( propertyName );
		foreach ( PersistentRasterPlacementEvidence item in evidence ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"subject",
				GetPersistentRasterPlacementSubjectName( item.Subject )
			);
			writer.WriteBoolean( "isPositive", item.IsPositive );
			writer.WriteString(
				"kind",
				GetPersistentRasterPlacementEvidenceKindName( item.Kind )
			);
			writer.WriteString( "sourceLabel", item.SourceLabel );
			writer.WriteNumber( "sourceOrdinal", item.SourceOrdinal );
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WritePersistentRasterRuntimeIntegrationIssues(
		DeterministicJsonWriter writer,
		IReadOnlyList<PersistentRasterRuntimeIntegrationIssue> issues,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( issues );

		writer.WriteStartArray( "issues" );
		foreach ( PersistentRasterRuntimeIntegrationIssue issue in issues ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStartObjectValue();
			writer.WriteString(
				"kind",
				GetPersistentRasterRuntimeIntegrationIssueKindName( issue.Kind )
			);
			writer.WriteNumber(
				"existingEvidenceCount",
				issue.ExistingEvidenceCount
			);
			writer.WriteNumber(
				"requestedImportCount",
				issue.RequestedImportCount
			);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	private static void WritePersistentRasterRuntimeLifecycleStates(
		DeterministicJsonWriter writer,
		PersistentRasterLifecycleProfile profile
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( profile );

		writer.WriteStartObject( "lifecycleStates" );
		writer.WriteString(
			"rasterDisplay",
			GetPersistentRasterLifecycleSupportStatusName( profile.RasterDisplay )
		);
		writer.WriteString(
			"persistentUpload",
			GetPersistentRasterLifecycleSupportStatusName( profile.PersistentUpload )
		);
		writer.WriteString(
			"acknowledgedUpload",
			GetPersistentRasterLifecycleSupportStatusName( profile.AcknowledgedUpload )
		);
		writer.WriteString(
			"placementCreation",
			GetPersistentRasterLifecycleSupportStatusName( profile.PlacementCreation )
		);
		writer.WriteString(
			"multiplePlacements",
			GetPersistentRasterLifecycleSupportStatusName( profile.MultiplePlacements )
		);
		writer.WriteString(
			"placementUpdate",
			GetPersistentRasterLifecycleSupportStatusName( profile.PlacementUpdate )
		);
		writer.WriteString(
			"placementDeletion",
			GetPersistentRasterLifecycleSupportStatusName( profile.PlacementDeletion )
		);
		writer.WriteString(
			"resourceDeletion",
			GetPersistentRasterLifecycleSupportStatusName( profile.ResourceDeletion )
		);
		writer.WriteEndObject();
	}

	private static void WritePersistentRasterRuntimePlacementStates(
		DeterministicJsonWriter writer,
		PersistentRasterPlacementProfile profile
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( profile );

		writer.WriteStartObject( "placementStates" );
		writer.WriteString(
			"sourceRectangle",
			GetPersistentRasterLifecycleSupportStatusName( profile.SourceRectangle )
		);
		writer.WriteString(
			"signedZOrder",
			GetPersistentRasterLifecycleSupportStatusName( profile.SignedZOrder )
		);
		writer.WriteEndObject();
	}

	private static string GetPersistentRasterRuntimeObservationOutcomeName(
		PersistentRasterRuntimeObservationOutcome outcome
	) =>
		outcome switch {
			PersistentRasterRuntimeObservationOutcome.Supported => "supported",
			PersistentRasterRuntimeObservationOutcome.Unsupported => "unsupported",
			PersistentRasterRuntimeObservationOutcome.Inconclusive => "inconclusive",
			_ => throw new ArgumentOutOfRangeException(
				nameof( outcome ),
				outcome,
				"The runtime-observation outcome must be a defined value."
			),
		};

	private static string GetPersistentRasterRuntimeIntegrationIssueKindName(
		PersistentRasterRuntimeIntegrationIssueKind kind
	) =>
		kind switch {
			PersistentRasterRuntimeIntegrationIssueKind.LifecycleEvidenceCapacityExhausted =>
				"lifecycleEvidenceCapacityExhausted",
			PersistentRasterRuntimeIntegrationIssueKind.PlacementEvidenceCapacityExhausted =>
				"placementEvidenceCapacityExhausted",
			PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted =>
				"lifecycleOrdinalSpaceExhausted",
			PersistentRasterRuntimeIntegrationIssueKind.PlacementOrdinalSpaceExhausted =>
				"placementOrdinalSpaceExhausted",
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The runtime integration issue kind must be a defined value."
			),
		};
}
