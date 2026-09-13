namespace Icod.TermInfo.Inspection;

/// <summary>
/// Derives deterministic protocol-neutral persistent-raster placement evidence
/// from explicit terminal-description semantics and composes that evidence through
/// the frozen database-set precedence model.
/// </summary>
public static class PersistentRasterPlacementInspector {
	/// <summary>
	/// The Icod-owned extended Boolean capability declaring source-rectangle
	/// placement support.
	/// </summary>
	public const string SourceRectangleCapabilityName =
		"IcodPersistentRasterSourceRectangle";

	/// <summary>
	/// The Icod-owned extended Boolean capability declaring signed z-order
	/// placement support.
	/// </summary>
	public const string SignedZOrderCapabilityName =
		"IcodPersistentRasterSignedZOrder";

	private static readonly SemanticCapability[] SemanticCapabilities = [
		new(
			PersistentRasterPlacementSubject.SourceRectangle,
			SourceRectangleCapabilityName,
			0
		),
		new(
			PersistentRasterPlacementSubject.SignedZOrder,
			SignedZOrderCapabilityName,
			1
		),
	];

	/// <summary>
	/// Inspects explicit placement semantics present on one terminal description,
	/// optionally layering caller-supplied evidence after static capability evidence.
	/// </summary>
	/// <param name="description">The immutable terminal description to inspect.</param>
	/// <param name="callerEvidence">
	/// Optional declared or verified evidence supplied by the caller.
	/// </param>
	/// <param name="options">Optional deterministic evidence bounds.</param>
	/// <returns>An immutable classified placement profile.</returns>
	public static PersistentRasterPlacementProfile Inspect(
		TerminalDescription description,
		IEnumerable<PersistentRasterPlacementEvidence>? callerEvidence = null,
		PersistentRasterPlacementEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( description );

		List<PersistentRasterPlacementEvidence> evidence = [];
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
					$"Extended capability '{semanticCapability.CapabilityName}' on terminal '{description.Name}' must be Boolean to declare persistent-raster placement semantics."
				);
			}

			evidence.Add(
				new PersistentRasterPlacementEvidence(
					semanticCapability.Subject,
					value.BooleanValue,
					PersistentRasterPlacementEvidenceKind.CapabilityDerived,
					semanticCapability.CapabilityName,
					semanticCapability.SourceOrdinal
				)
			);
		}

		if ( callerEvidence is not null ) {
			evidence.AddRange( callerEvidence );
		}

		return PersistentRasterPlacementClassifier.Classify(
			evidence,
			options
		);
	}

	/// <summary>
	/// Inspects one canonical terminal identity through the frozen 1.10 ordered
	/// database-set precedence model. Caller evidence is applied only after a
	/// conclusive winner is established.
	/// </summary>
	/// <param name="databaseSet">The immutable ordered database set.</param>
	/// <param name="canonicalName">The exact canonical terminal identity.</param>
	/// <param name="callerEvidence">
	/// Optional caller evidence to layer onto the effective winner only.
	/// </param>
	/// <param name="options">Optional deterministic evidence bounds.</param>
	/// <returns>
	/// Database-set lookup provenance together with every observed static profile
	/// and, only when precedence is conclusive, the effective placement profile.
	/// </returns>
	public static PersistentRasterPlacementDatabaseSetInspection Inspect(
		TermInfoDatabaseSet databaseSet,
		string canonicalName,
		IEnumerable<PersistentRasterPlacementEvidence>? callerEvidence = null,
		PersistentRasterPlacementEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentException.ThrowIfNullOrWhiteSpace( canonicalName );

		TermInfoDatabaseSetLookupResult lookup =
			databaseSet.LookupCanonicalName( canonicalName );
		PersistentRasterPlacementProfile[] occurrenceProfiles =
			new PersistentRasterPlacementProfile[ lookup.Occurrences.Count ];
		for ( int index = 0; index < occurrenceProfiles.Length; index++ ) {
			occurrenceProfiles[ index ] = Inspect(
				lookup.Occurrences[ index ].Entry.Terminal,
				callerEvidence: null,
				options
			);
		}

		PersistentRasterPlacementProfile? effectiveProfile = null;
		if ( lookup.Status == TermInfoDatabaseSetLookupStatus.WinnerKnown ) {
			if ( lookup.Winner is null || occurrenceProfiles.Length == 0 ) {
				throw new InvalidOperationException(
					"A known database-set winner must provide an observed placement profile."
				);
			}

			effectiveProfile = (
				callerEvidence is null
					? occurrenceProfiles[ 0 ]
					: Inspect(
						lookup.Winner.Entry.Terminal,
						callerEvidence,
						options
					)
			)
			;
		}

		return new PersistentRasterPlacementDatabaseSetInspection(
			databaseSet,
			lookup,
			occurrenceProfiles,
			effectiveProfile
		);
	}

	private readonly record struct SemanticCapability(
		PersistentRasterPlacementSubject Subject,
		string CapabilityName,
		int SourceOrdinal
	);
}
