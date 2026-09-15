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
/// Preserves frozen database-set lookup provenance together with deterministic
/// lifecycle profiles for every observed occurrence and the conclusive effective
/// profile when one exists.
/// </summary>
public sealed class PersistentRasterLifecycleDatabaseSetInspection {
	internal PersistentRasterLifecycleDatabaseSetInspection(
		TermInfoDatabaseSet databaseSet,
		TermInfoDatabaseSetLookupResult lookup,
		IEnumerable<PersistentRasterLifecycleProfile> occurrenceProfiles,
		PersistentRasterLifecycleProfile? effectiveProfile
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( lookup );
		ArgumentNullException.ThrowIfNull( occurrenceProfiles );

		PersistentRasterLifecycleProfile[] profileArray =
			occurrenceProfiles.ToArray();
		if ( profileArray.Any( profile => profile is null ) ) {
			throw new ArgumentException(
				"Database-set lifecycle occurrence profiles cannot contain null.",
				nameof( occurrenceProfiles )
			);
		}
		if ( profileArray.Length != lookup.Occurrences.Count ) {
			throw new ArgumentException(
				"Database-set lifecycle occurrence profiles must align one-to-one with lookup occurrences.",
				nameof( occurrenceProfiles )
			);
		}

		switch ( lookup.Status ) {
			case TermInfoDatabaseSetLookupStatus.WinnerKnown:
				if (
					effectiveProfile is null
					|| profileArray.Length == 0
					|| !ReferenceEquals(
						effectiveProfile,
						profileArray[ 0 ]
					)
				) {
					throw new ArgumentException(
						"A conclusive database-set winner must use the first observed occurrence profile as the effective lifecycle profile.",
						nameof( effectiveProfile )
					);
				}
				break;
			case TermInfoDatabaseSetLookupStatus.NotObserved:
			case TermInfoDatabaseSetLookupStatus.Indeterminate:
				if ( effectiveProfile is not null ) {
					throw new ArgumentException(
						"A database-set lookup without a conclusive winner cannot expose an effective lifecycle profile.",
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
	/// Gets the unchanged frozen 1.10 lookup and incompleteness evidence.
	/// </summary>
	public TermInfoDatabaseSetLookupResult Lookup {
		get;
	}

	/// <summary>
	/// Gets lifecycle profiles in exact <see cref="TermInfoDatabaseSetLookupResult.Occurrences"/>
	/// order, including later shadows and observations that cannot establish an
	/// effective winner because an earlier incomplete database blocks precedence.
	/// </summary>
	public IReadOnlyList<PersistentRasterLifecycleProfile> OccurrenceProfiles {
		get;
	}

	/// <summary>
	/// Gets the effective lifecycle profile only when the frozen lookup status is
	/// <see cref="TermInfoDatabaseSetLookupStatus.WinnerKnown"/>; otherwise
	/// <see langword="null"/>.
	/// </summary>
	public PersistentRasterLifecycleProfile? EffectiveProfile {
		get;
	}
}
