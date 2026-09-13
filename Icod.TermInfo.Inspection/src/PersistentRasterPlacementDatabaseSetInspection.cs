namespace Icod.TermInfo.Inspection;

/// <summary>
/// Preserves frozen database-set lookup provenance together with deterministic
/// persistent-raster placement profiles for every observed occurrence and the
/// effective profile when precedence is conclusive.
/// </summary>
public sealed class PersistentRasterPlacementDatabaseSetInspection {
	internal PersistentRasterPlacementDatabaseSetInspection(
		TermInfoDatabaseSet databaseSet,
		TermInfoDatabaseSetLookupResult lookup,
		IEnumerable<PersistentRasterPlacementProfile> occurrenceProfiles,
		PersistentRasterPlacementProfile? effectiveProfile
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( lookup );
		ArgumentNullException.ThrowIfNull( occurrenceProfiles );

		PersistentRasterPlacementProfile[] profileArray =
			occurrenceProfiles.ToArray();
		if ( profileArray.Any( profile => profile is null ) ) {
			throw new ArgumentException(
				"Database-set placement occurrence profiles cannot contain null.",
				nameof( occurrenceProfiles )
			);
		}
		if ( profileArray.Length != lookup.Occurrences.Count ) {
			throw new ArgumentException(
				"Database-set placement occurrence profiles must align one-to-one with lookup occurrences.",
				nameof( occurrenceProfiles )
			);
		}

		switch ( lookup.Status ) {
			case TermInfoDatabaseSetLookupStatus.WinnerKnown:
				if ( effectiveProfile is null || profileArray.Length == 0 ) {
					throw new ArgumentException(
						"A conclusive database-set winner must provide an effective placement profile.",
						nameof( effectiveProfile )
					);
				}
				break;
			case TermInfoDatabaseSetLookupStatus.NotObserved:
			case TermInfoDatabaseSetLookupStatus.Indeterminate:
				if ( effectiveProfile is not null ) {
					throw new ArgumentException(
						"A database-set lookup without a conclusive winner cannot expose an effective placement profile.",
						nameof( effectiveProfile )
					);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException(
					nameof( lookup ),
					lookup.Status,
					"The database-set lookup status must be a defined value."
				);
		}

		DatabaseSet = databaseSet;
		Lookup = lookup;
		OccurrenceProfiles = Array.AsReadOnly( profileArray );
		EffectiveProfile = effectiveProfile;
	}

	/// <summary>
	/// Gets the exact immutable ordered database set inspected by this result.
	/// </summary>
	public TermInfoDatabaseSet DatabaseSet {
		get;
	}

	/// <summary>
	/// Gets the unchanged frozen 1.10 canonical-name lookup and incompleteness
	/// evidence.
	/// </summary>
	public TermInfoDatabaseSetLookupResult Lookup {
		get;
	}

	/// <summary>
	/// Gets placement profiles in exact lookup occurrence order. These profiles
	/// contain description-derived evidence only; caller evidence is never applied
	/// to shadows or otherwise non-effective occurrences.
	/// </summary>
	public IReadOnlyList<PersistentRasterPlacementProfile> OccurrenceProfiles {
		get;
	}

	/// <summary>
	/// Gets the effective placement profile only when the frozen database-set
	/// lookup has a known winner. Caller evidence, when supplied, is layered here
	/// only after that winner boundary has been established.
	/// </summary>
	public PersistentRasterPlacementProfile? EffectiveProfile {
		get;
	}
}
