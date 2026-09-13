namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	/// <summary>
	/// The additive schema identifier used by 1.12 advanced persistent-raster
	/// placement automation. Versions 1 through 3 retain their frozen identifiers.
	/// </summary>
	public const string PersistentRasterPlacementSchemaIdentifier =
		"urn:icod:terminfo:inspection:json:4";

	/// <summary>
	/// The additive schema version used by 1.12 advanced persistent-raster placement
	/// automation.
	/// </summary>
	public const int PersistentRasterPlacementSchemaVersion = 4;

	private const string PersistentRasterPlacementProfileDocumentKind =
		"persistentRasterPlacementProfile";
	private const string PersistentRasterPlacementPlanDocumentKind =
		"persistentRasterPlacementPlan";

	/// <summary>
	/// Renders one advanced persistent-raster placement profile using the additive
	/// version-4 automation contract.
	/// </summary>
	/// <param name="profile">The immutable classified placement profile.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterPlacementProfile profile
	) =>
		Render(
			profile,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one advanced persistent-raster placement profile using explicit
	/// deterministic JSON policy.
	/// </summary>
	/// <param name="profile">The immutable classified placement profile.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterPlacementProfile profile,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( profile );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterPlacementProfileV4(
			profile,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders one advanced persistent-raster placement plan using the additive
	/// version-4 automation contract.
	/// </summary>
	/// <param name="plan">The immutable placement plan.</param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterPlacementPlan plan
	) =>
		Render(
			plan,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders one advanced persistent-raster placement plan using explicit
	/// deterministic JSON policy.
	/// </summary>
	/// <param name="plan">The immutable placement plan.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	public static string Render(
		PersistentRasterPlacementPlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderPersistentRasterPlacementPlanV4(
			plan,
			options,
			cancellationToken
		);
	}

	private static string RenderPersistentRasterPlacementProfileV4(
		PersistentRasterPlacementProfile profile,
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
			WritePersistentRasterPlacementEnvelopePrefix(
				writer,
				PersistentRasterPlacementProfileDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteStartObject( "states" );
			writer.WriteString(
				"sourceRectangle",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.SourceRectangle
				)
			);
			writer.WriteString(
				"signedZOrder",
				GetPersistentRasterLifecycleSupportStatusName(
					profile.SignedZOrder
				)
			);
			writer.WriteEndObject();
			writer.WriteNumber( "evidenceCount", profile.Evidence.Count );
			writer.WriteStartArray( "evidence" );
			foreach ( PersistentRasterPlacementEvidence evidence in profile.Evidence ) {
				cancellationToken.ThrowIfCancellationRequested();
				writer.WriteStartObjectValue();
				writer.WriteString(
					"subject",
					GetPersistentRasterPlacementSubjectName(
						evidence.Subject
					)
				);
				writer.WriteBoolean( "isPositive", evidence.IsPositive );
				writer.WriteString(
					"kind",
					GetPersistentRasterPlacementEvidenceKindName(
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

	private static string RenderPersistentRasterPlacementPlanV4(
		PersistentRasterPlacementPlan plan,
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
			WritePersistentRasterPlacementEnvelopePrefix(
				writer,
				PersistentRasterPlacementPlanDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteString(
				"status",
				GetPersistentRasterPlacementPlanStatusName(
					plan.Status
				)
			);
			writer.WriteString(
				"lifecycleStatus",
				GetPersistentRasterLifecyclePlanStatusName(
					plan.LifecycleStatus
				)
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
					GetPersistentRasterPlacementSubjectName(
						requirement.Subject
					)
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
					GetPersistentRasterPlacementSubjectName(
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

	private static void WritePersistentRasterPlacementEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentException.ThrowIfNullOrWhiteSpace( documentKind );

		writer.WriteString(
			"schema",
			PersistentRasterPlacementSchemaIdentifier
		);
		writer.WriteNumber(
			"schemaVersion",
			PersistentRasterPlacementSchemaVersion
		);
		writer.WriteString( "documentKind", documentKind );
	}

	private static string GetPersistentRasterPlacementSubjectName(
		PersistentRasterPlacementSubject subject
	) =>
		subject switch {
			PersistentRasterPlacementSubject.SourceRectangle => "sourceRectangle",
			PersistentRasterPlacementSubject.SignedZOrder => "signedZOrder",
			_ => throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement subject must be a defined value."
			),
		};

	private static string GetPersistentRasterPlacementEvidenceKindName(
		PersistentRasterPlacementEvidenceKind kind
	) =>
		kind switch {
			PersistentRasterPlacementEvidenceKind.CapabilityDerived =>
				"capabilityDerived",
			PersistentRasterPlacementEvidenceKind.Declared => "declared",
			PersistentRasterPlacementEvidenceKind.Verified => "verified",
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The placement evidence kind must be a defined value."
			),
		};

	private static string GetPersistentRasterPlacementPlanStatusName(
		PersistentRasterPlacementPlanStatus status
	) =>
		status switch {
			PersistentRasterPlacementPlanStatus.Satisfied => "satisfied",
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification =>
				"requiresRuntimeVerification",
			PersistentRasterPlacementPlanStatus.Indeterminate => "indeterminate",
			PersistentRasterPlacementPlanStatus.Impossible => "impossible",
			_ => throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The placement plan status must be a defined value."
			),
		};
}
