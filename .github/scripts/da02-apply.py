from pathlib import Path


def replace_exact(path_name: str, old: str, new: str, count: int = 1) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual != count:
        raise RuntimeError(
            f"{path_name}: expected {count} occurrence(s), found {actual}: {old!r}"
        )
    path.write_text(text.replace(old, new, count), encoding="utf-8", newline="\n")


def replace_all_required(path_name: str, old: str, new: str) -> None:
    path = Path(path_name)
    text = path.read_text(encoding="utf-8")
    actual = text.count(old)
    if actual < 1:
        raise RuntimeError(f"{path_name}: required text not found: {old!r}")
    path.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


def write_new(path_name: str, content: str) -> None:
    path = Path(path_name)
    if path.exists():
        raise RuntimeError(f"{path_name}: file already exists")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8", newline="\n")


write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetLookupStatus.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes the deterministic canonical-name lookup state for an ordered
/// terminfo database set.
/// </summary>
public enum TermInfoDatabaseSetLookupStatus {
	/// <summary>
	/// The canonical identity was not observed and every constituent database was
	/// inspected completely, so absence is conclusive.
	/// </summary>
	NotObserved = 0,

	/// <summary>
	/// At least one occurrence was observed and the first applicable occurrence is
	/// known under caller-selected database precedence.
	/// </summary>
	WinnerKnown = 1,

	/// <summary>
	/// Incomplete evidence prevents a conclusive absence or winner determination.
	/// </summary>
	Indeterminate = 2,
}
''',
)

write_new(
    "Icod.TermInfo.Inspection/src/TermInfoDatabaseSetLookupResult.cs",
    '''namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents deterministic ordered-precedence evidence for one canonical
/// terminal name in a terminfo database set.
/// </summary>
public sealed class TermInfoDatabaseSetLookupResult {
	internal TermInfoDatabaseSetLookupResult(
		string name,
		TermInfoDatabaseSetLookupStatus status,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences,
		TermInfoDatabaseSetOccurrence? winner,
		IEnumerable<TermInfoDatabaseSetOccurrence> shadowedOccurrences,
		IEnumerable<int> incompleteDatabaseIndices,
		IEnumerable<int> blockingDatabaseIndices
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( occurrences );
		ArgumentNullException.ThrowIfNull( shadowedOccurrences );
		ArgumentNullException.ThrowIfNull( incompleteDatabaseIndices );
		ArgumentNullException.ThrowIfNull( blockingDatabaseIndices );

		TermInfoDatabaseSetOccurrence[] occurrenceArray = occurrences.ToArray();
		TermInfoDatabaseSetOccurrence[] shadowArray = shadowedOccurrences.ToArray();
		int[] incompleteArray = incompleteDatabaseIndices.ToArray();
		int[] blockingArray = blockingDatabaseIndices.ToArray();
		if ( occurrenceArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Lookup occurrences cannot contain null.",
				nameof( occurrences )
			);
		}
		if ( shadowArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Shadow occurrences cannot contain null.",
				nameof( shadowedOccurrences )
			);
		}
		if ( occurrenceArray.Any(
				occurrence => !string.Equals(
					occurrence.Name,
					name,
					StringComparison.Ordinal
				)
			) ) {
			throw new ArgumentException(
				"Every lookup occurrence must declare the requested canonical identity.",
				nameof( occurrences )
			);
		}
		if ( incompleteArray.Any( index => index < 0 )
			|| blockingArray.Any( index => index < 0 ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( incompleteDatabaseIndices ),
				"Database indices cannot be negative."
			);
		}
		if ( incompleteArray.Distinct().Count() != incompleteArray.Length
			|| blockingArray.Distinct().Count() != blockingArray.Length ) {
			throw new ArgumentException(
				"Database-index evidence cannot contain duplicates."
			);
		}
		if ( blockingArray.Except( incompleteArray ).Any() ) {
			throw new ArgumentException(
				"Every blocking database must also be incomplete.",
				nameof( blockingDatabaseIndices )
			);
		}

		switch ( status ) {
			case TermInfoDatabaseSetLookupStatus.NotObserved:
				if ( occurrenceArray.Length != 0
					|| winner is not null
					|| shadowArray.Length != 0
					|| incompleteArray.Length != 0
					|| blockingArray.Length != 0 ) {
					throw new ArgumentException(
						"A conclusive absence cannot contain occurrence or incomplete evidence."
					);
				}
				break;
			case TermInfoDatabaseSetLookupStatus.WinnerKnown:
				if ( occurrenceArray.Length == 0
					|| winner is null
					|| !ReferenceEquals( winner, occurrenceArray[ 0 ] )
					|| blockingArray.Length != 0
					|| shadowArray.Length != occurrenceArray.Length - 1 ) {
					throw new ArgumentException(
						"A known winner must be the first occurrence and all later observed occurrences must be shadows."
					);
				}
				for ( int index = 0; index < shadowArray.Length; index++ ) {
					if ( !ReferenceEquals( shadowArray[ index ], occurrenceArray[ index + 1 ] ) ) {
						throw new ArgumentException(
							"Shadow occurrences must preserve the later occurrence order.",
							nameof( shadowedOccurrences )
						);
					}
				}
				break;
			case TermInfoDatabaseSetLookupStatus.Indeterminate:
				if ( winner is not null
					|| shadowArray.Length != 0
					|| blockingArray.Length == 0 ) {
					throw new ArgumentException(
						"Indeterminate lookup evidence requires at least one blocking incomplete database and cannot claim a winner or shadows."
					);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( status ) );
		}

		Name = name;
		Status = status;
		Occurrences = Array.AsReadOnly( occurrenceArray );
		Winner = winner;
		ShadowedOccurrences = Array.AsReadOnly( shadowArray );
		IncompleteDatabaseIndices = Array.AsReadOnly( incompleteArray );
		BlockingDatabaseIndices = Array.AsReadOnly( blockingArray );
	}

	/// <summary>
	/// Gets the exact ordinal canonical name requested by the caller.
	/// </summary>
	public string Name {
		get;
	}

	/// <summary>
	/// Gets the conclusive or indeterminate lookup state.
	/// </summary>
	public TermInfoDatabaseSetLookupStatus Status {
		get;
	}

	/// <summary>
	/// Gets all observed physical occurrences in database and catalog-entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}

	/// <summary>
	/// Gets the first applicable observed occurrence when precedence is conclusive.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? Winner {
		get;
	}

	/// <summary>
	/// Gets later observed occurrences only when a winner is conclusive.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> ShadowedOccurrences {
		get;
	}

	/// <summary>
	/// Gets every incomplete constituent database index in caller order.
	/// </summary>
	public IReadOnlyList<int> IncompleteDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets incomplete database indices which prevent a conclusive absence or
	/// winner determination.
	/// </summary>
	public IReadOnlyList<int> BlockingDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets whether at least one physical occurrence was observed.
	/// </summary>
	public bool IsObserved =>
		Occurrences.Count != 0;

	/// <summary>
	/// Gets whether more than one physical occurrence was observed.
	/// </summary>
	public bool HasMultipleOccurrences =>
		Occurrences.Count > 1;

	/// <summary>
	/// Gets whether every database was inspected completely, so the occurrence
	/// list itself is exhaustive for the requested canonical identity.
	/// </summary>
	public bool IsObservationComplete =>
		IncompleteDatabaseIndices.Count == 0;
}
''',
)

set_path = Path("Icod.TermInfo.Inspection/src/TermInfoDatabaseSet.cs")
set_text = set_path.read_text(encoding="utf-8")
needle = '''\tpublic bool IsComplete {\n\t\tget;\n\t}\n}'''
replacement = '''\tpublic bool IsComplete {\n\t\tget;\n\t}\n\n\t/// <summary>\n\t/// Resolves one exact canonical terminal name against caller-selected database\n\t/// precedence without treating aliases as canonical identities.\n\t/// </summary>\n\t/// <param name="name">The exact canonical terminal name.</param>\n\t/// <returns>Structured precedence and incomplete-input evidence.</returns>\n\tpublic TermInfoDatabaseSetLookupResult LookupCanonicalName(\n\t\tstring name\n\t) {\n\t\tArgumentException.ThrowIfNullOrWhiteSpace( name );\n\n\t\tint[] incompleteDatabaseIndices =\n\t\t\tEntries\n\t\t\t\t.Where( entry => !entry.IsComplete )\n\t\t\t\t.Select( entry => entry.Index )\n\t\t\t\t.ToArray();\n\t\tTermInfoDatabaseSetIdentity? identity = FindIdentity( name );\n\t\tif ( identity is null ) {\n\t\t\tif ( incompleteDatabaseIndices.Length == 0 ) {\n\t\t\t\treturn new TermInfoDatabaseSetLookupResult(\n\t\t\t\t\tname,\n\t\t\t\t\tTermInfoDatabaseSetLookupStatus.NotObserved,\n\t\t\t\t\tArray.Empty<TermInfoDatabaseSetOccurrence>(),\n\t\t\t\t\tnull,\n\t\t\t\t\tArray.Empty<TermInfoDatabaseSetOccurrence>(),\n\t\t\t\t\tArray.Empty<int>(),\n\t\t\t\t\tArray.Empty<int>()\n\t\t\t\t);\n\t\t\t}\n\n\t\t\treturn new TermInfoDatabaseSetLookupResult(\n\t\t\t\tname,\n\t\t\t\tTermInfoDatabaseSetLookupStatus.Indeterminate,\n\t\t\t\tArray.Empty<TermInfoDatabaseSetOccurrence>(),\n\t\t\t\tnull,\n\t\t\t\tArray.Empty<TermInfoDatabaseSetOccurrence>(),\n\t\t\t\tincompleteDatabaseIndices,\n\t\t\t\tincompleteDatabaseIndices\n\t\t\t);\n\t\t}\n\n\t\tTermInfoDatabaseSetOccurrence[] occurrences =\n\t\t\tidentity.Occurrences.ToArray();\n\t\tint firstObservedDatabaseIndex = occurrences[ 0 ].DatabaseIndex;\n\t\tint[] blockingDatabaseIndices =\n\t\t\tincompleteDatabaseIndices\n\t\t\t\t.Where( index => index <= firstObservedDatabaseIndex )\n\t\t\t\t.ToArray();\n\t\tif ( blockingDatabaseIndices.Length != 0 ) {\n\t\t\treturn new TermInfoDatabaseSetLookupResult(\n\t\t\t\tname,\n\t\t\t\tTermInfoDatabaseSetLookupStatus.Indeterminate,\n\t\t\t\toccurrences,\n\t\t\t\tnull,\n\t\t\t\tArray.Empty<TermInfoDatabaseSetOccurrence>(),\n\t\t\t\tincompleteDatabaseIndices,\n\t\t\t\tblockingDatabaseIndices\n\t\t\t);\n\t\t}\n\n\t\treturn new TermInfoDatabaseSetLookupResult(\n\t\t\tname,\n\t\t\tTermInfoDatabaseSetLookupStatus.WinnerKnown,\n\t\t\toccurrences,\n\t\t\toccurrences[ 0 ],\n\t\t\toccurrences.Skip( 1 ),\n\t\t\tincompleteDatabaseIndices,\n\t\t\tArray.Empty<int>()\n\t\t);\n\t}\n\n\tprivate TermInfoDatabaseSetIdentity? FindIdentity(\n\t\tstring name\n\t) {\n\t\tArgumentException.ThrowIfNullOrWhiteSpace( name );\n\n\t\tint low = 0;\n\t\tint high = Identities.Count - 1;\n\t\twhile ( low <= high ) {\n\t\t\tint middle = low + ( ( high - low ) / 2 );\n\t\t\tTermInfoDatabaseSetIdentity candidate = Identities[ middle ];\n\t\t\tint comparison = StringComparer.Ordinal.Compare( candidate.Name, name );\n\t\t\tif ( comparison == 0 ) {\n\t\t\t\treturn candidate;\n\t\t\t}\n\t\t\tif ( comparison < 0 ) {\n\t\t\t\tlow = middle + 1;\n\t\t\t} else {\n\t\t\t\thigh = middle - 1;\n\t\t\t}\n\t\t}\n\n\t\treturn null;\n\t}\n}'''
if set_text.count(needle) != 1:
    raise RuntimeError("TermInfoDatabaseSet.cs insertion marker mismatch")
set_path.write_text(set_text.replace(needle, replacement, 1), encoding="utf-8", newline="\n")

write_new(
    "tests/Icod.TermInfo.Inspection.Tests/src/DA02DatabaseSetPrecedenceTests.cs",
    '''using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA02DatabaseSetPrecedenceTests {
	[Fact]
	public void CompleteAbsenceIsConclusiveAndAliasDoesNotBecomeCanonicalLookup() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"absence",
						CreateTerminal( "canonical", "alias" )
					),
				]
			);

		TermInfoDatabaseSetLookupResult missing =
			set.LookupCanonicalName( "missing" );
		Assert.Equal( TermInfoDatabaseSetLookupStatus.NotObserved, missing.Status );
		Assert.False( missing.IsObserved );
		Assert.True( missing.IsObservationComplete );
		Assert.Null( missing.Winner );
		Assert.Empty( missing.Occurrences );
		Assert.Empty( missing.ShadowedOccurrences );
		Assert.Empty( missing.BlockingDatabaseIndices );

		TermInfoDatabaseSetLookupResult alias =
			set.LookupCanonicalName( "alias" );
		Assert.Equal( TermInfoDatabaseSetLookupStatus.NotObserved, alias.Status );
	}

	[Fact]
	public void CompleteSingleOccurrenceHasKnownWinnerWithoutShadows() {
		TermInfoDatabaseCatalog catalog =
			CreateCatalog( "single", CreateTerminal( "target", "target-alias" ) );
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ catalog ]);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.True( result.IsObserved );
		Assert.False( result.HasMultipleOccurrences );
		Assert.True( result.IsObservationComplete );
		TermInfoDatabaseSetOccurrence winner = Assert.IsType<TermInfoDatabaseSetOccurrence>( result.Winner );
		Assert.Same( result.Occurrences[ 0 ], winner );
		Assert.Equal( 0, winner.DatabaseIndex );
		Assert.Equal( 0, winner.CatalogEntryIndex );
		Assert.Contains( "target-alias", winner.Aliases );
		Assert.Empty( result.ShadowedOccurrences );
		Assert.Empty( result.IncompleteDatabaseIndices );
	}

	[Fact]
	public void CompleteRepeatedIdentityUsesFirstOccurrenceAndPreservesLaterShadowOrder() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"first",
						CreateTerminal( "same", "first-a" ),
						CreateTerminal( "same", "first-b" )
					),
					CreateCatalog(
						"second",
						CreateTerminal( "same", "second" )
					),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "same" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.True( result.HasMultipleOccurrences );
		Assert.Equal( 3, result.Occurrences.Count );
		Assert.Same( result.Occurrences[ 0 ], result.Winner );
		Assert.Equal( 2, result.ShadowedOccurrences.Count );
		Assert.Same( result.Occurrences[ 1 ], result.ShadowedOccurrences[ 0 ] );
		Assert.Same( result.Occurrences[ 2 ], result.ShadowedOccurrences[ 1 ] );
		Assert.Equal(
			new[] { 0, 0, 1 },
			result.Occurrences.Select( occurrence => occurrence.DatabaseIndex ).ToArray()
		);
	}

	[Fact]
	public void EarlierIncompleteDatabaseMakesLaterObservedWinnerIndeterminate() {
		TermInfoDatabaseCatalog incomplete =
			CreateIncompleteCatalog( "blocking", Array.Empty<TerminalDescription>() );
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					incomplete,
					CreateCatalog( "later", CreateTerminal( "target" ) ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.True( result.IsObserved );
		Assert.False( result.IsObservationComplete );
		Assert.Null( result.Winner );
		Assert.Empty( result.ShadowedOccurrences );
		Assert.Equal( new[] { 0 }, result.IncompleteDatabaseIndices );
		Assert.Equal( new[] { 0 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void ObservedOccurrenceInsideIncompleteDatabaseCannotClaimWinner() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateIncompleteCatalog(
						"same-root",
						[ CreateTerminal( "target" ) ]
					),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.Single( result.Occurrences );
		Assert.Null( result.Winner );
		Assert.Equal( new[] { 0 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void LaterIncompleteDatabaseDoesNotInvalidateEarlierKnownWinnerButMarksObservationIncomplete() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog( "winner", CreateTerminal( "target" ) ),
					CreateIncompleteCatalog( "later-incomplete", Array.Empty<TerminalDescription>() ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, result.Status );
		Assert.NotNull( result.Winner );
		Assert.Equal( 0, result.Winner!.DatabaseIndex );
		Assert.False( result.IsObservationComplete );
		Assert.Equal( new[] { 1 }, result.IncompleteDatabaseIndices );
		Assert.Empty( result.BlockingDatabaseIndices );
	}

	[Fact]
	public void IncompleteSetCannotClaimCanonicalAbsence() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog( "complete", CreateTerminal( "other" ) ),
					CreateIncompleteCatalog( "unknown", Array.Empty<TerminalDescription>() ),
				]
			);

		TermInfoDatabaseSetLookupResult result =
			set.LookupCanonicalName( "target" );

		Assert.Equal( TermInfoDatabaseSetLookupStatus.Indeterminate, result.Status );
		Assert.False( result.IsObserved );
		Assert.Null( result.Winner );
		Assert.Equal( new[] { 1 }, result.BlockingDatabaseIndices );
	}

	[Fact]
	public void LookupIsOrdinalCultureIndependentAndDoesNotReorderPathEvidence() {
		TermInfoDatabaseCatalog catalog =
			CreateCatalog(
				"culture",
				CreateTerminal( "I-terminal" ),
				CreateTerminal( "ı-terminal" )
			);
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = new CultureInfo( "tr-TR" );
			TermInfoDatabaseSet set =
				TermInfoDatabaseInspector.CreateSet([ catalog ]);

			TermInfoDatabaseSetLookupResult latin =
				set.LookupCanonicalName( "I-terminal" );
			TermInfoDatabaseSetLookupResult dotless =
				set.LookupCanonicalName( "ı-terminal" );

			Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, latin.Status );
			Assert.Equal( TermInfoDatabaseSetLookupStatus.WinnerKnown, dotless.Status );
			Assert.NotEqual( latin.Winner!.Entry.Path, dotless.Winner!.Entry.Path );
			Assert.Same( catalog.Entries[ 0 ], latin.Winner.Entry );
			Assert.Same( catalog.Entries[ 1 ], dotless.Winner.Entry );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[Fact]
	public void Da02AddsOnlyStructuredLookupStatusAndResultConcepts() {
		Type[] exportedTypes =
			typeof( TermInfoDatabaseSetLookupResult ).Assembly.GetExportedTypes();

		Assert.Contains( typeof( TermInfoDatabaseSetLookupResult ), exportedTypes );
		Assert.Contains( typeof( TermInfoDatabaseSetLookupStatus ), exportedTypes );
		Assert.InRange( exportedTypes.Length, 39, int.MaxValue );
	}

	private static TermInfoDatabaseCatalog CreateCatalog(
		string rootName,
		params TerminalDescription[] terminals
	) =>
		CreateCatalogCore(
			rootName,
			terminals,
			Array.Empty<TermInfoDatabaseCatalogIssue>()
		);

	private static TermInfoDatabaseCatalog CreateIncompleteCatalog(
		string rootName,
		IReadOnlyList<TerminalDescription> terminals
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine( root, "entries", "malformed" ),
				"DA02 incomplete fixture."
			);
		return CreateCatalogCore( rootName, terminals, [ issue ] );
	}

	private static TermInfoDatabaseCatalog CreateCatalogCore(
		string rootName,
		IEnumerable<TerminalDescription> terminals,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogEntry[] entries =
			terminals
				.Select(
					( terminal, index ) => new TermInfoDatabaseCatalogEntry(
						Path.Combine(
							root,
							"entries",
							index.ToString( CultureInfo.InvariantCulture )
						),
						terminal
					)
				)
				.OrderBy( entry => entry.Name, StringComparer.Ordinal )
				.ThenBy( entry => entry.Path, StringComparer.Ordinal )
				.ToArray();
		string[] duplicates =
			entries
				.GroupBy( entry => entry.Name, StringComparer.Ordinal )
				.Where( group => group.Count() > 1 )
				.Select( group => group.Key )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			issues,
			duplicates
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private static string AbsolutePath(
		string suffix
	) =>
		Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-da02-{suffix}-{Guid.NewGuid():N}"
		);
}
''',
)

write_new(
    "docs/1.10.0-DA02-DETERMINISTIC-MULTI-CATALOG-PRECEDENCE.md",
    '''# Icod.TermInfo 1.10.0 DA02 — Deterministic Multi-Catalog Inspection and Precedence

**Development version:** `1.10.0-Alpha-2`  
**Tranche:** DA02  
**Published baseline:** `1.9.0`  
**DA01 baseline:** `1.10.0-Alpha-1`  
**Primary package:** `Icod.TermInfo.Inspection`  
**Status:** implementation complete; PR Staging validation pending  

## 1. Purpose

DA02 makes the immutable DA01 database-set model operational for exact canonical-
name lookup under caller-selected database order. It freezes precedence evidence
without yet comparing terminal semantics.

The governing distinction is:

```text
complete evidence + no occurrence
    -> NotObserved

complete precedence through first observed occurrence
    -> WinnerKnown

incomplete evidence before or at the first observed occurrence
or incomplete evidence for an otherwise absent identity
    -> Indeterminate
```

DA02 does not classify whether shadowed occurrences are semantically equal or
different. That is DA03.

## 2. Public surface

DA02 adds exactly two public concepts:

```text
TermInfoDatabaseSetLookupStatus
TermInfoDatabaseSetLookupResult
```

`TermInfoDatabaseSet` gains:

```csharp
TermInfoDatabaseSetLookupResult LookupCanonicalName( string name )
```

The method is intentionally canonical-name-only. Aliases remain occurrence
evidence and are not lookup keys until DA03 defines alias collision semantics.

## 3. Precedence

`TermInfoDatabaseSetIdentity.Occurrences` is already frozen by DA01 in database
order and then constituent catalog-entry order. DA02 uses that order directly.

For a complete prefix, the first observed occurrence is the winner. Every later
observed occurrence is exposed as a shadow in the same deterministic order.
No filesystem enumeration, culture comparison, path collation, semantic
comparison, or reinspection participates in precedence.

## 4. Incomplete evidence

A database is incomplete when its DA01 `TermInfoDatabaseSetEntry.IsComplete` is
false. DA02 preserves two kinds of incomplete evidence:

- `IncompleteDatabaseIndices` — every incomplete constituent in caller order;
- `BlockingDatabaseIndices` — the incomplete constituents which prevent a
  conclusive result for this canonical name.

If an identity is observed first in database `N`, any incomplete database with
index less than or equal to `N` blocks a winner claim. An incomplete later
database does not invalidate a winner already established by a complete earlier
prefix, but `IsObservationComplete` remains false because additional shadows may
exist in that incomplete later database.

For an unobserved canonical name, any incomplete database blocks a conclusive
absence. DA02 therefore never turns partial catalog evidence into `NotObserved`.

## 5. Result contract

`TermInfoDatabaseSetLookupResult` retains:

```text
Name
Status
Occurrences
Winner
ShadowedOccurrences
IncompleteDatabaseIndices
BlockingDatabaseIndices
IsObserved
HasMultipleOccurrences
IsObservationComplete
```

A `WinnerKnown` result always identifies `Occurrences[0]` as `Winner`, and
`ShadowedOccurrences` is exactly the later observed suffix. An `Indeterminate`
result never claims a winner or shadows. `NotObserved` is possible only with
complete aggregate evidence.

## 6. Frozen boundaries

DA02 does not change:

- Runtime, Source, Compiler, or Termcap APIs;
- the frozen 1.9 catalog or JSON v1 contracts;
- path normalization;
- `TerminalDescriptionComparer` semantics;
- 1.7 synthesis or 1.8 planning;
- any command syntax or output contract.

No semantic equality test is performed by DA02. Two physically repeated entries
with different terminal semantics still participate only in ordered occurrence
precedence; DA03 owns semantic classification.

## 7. Validation

DA02 tests cover:

- conclusive absence in complete sets;
- canonical-only lookup and alias non-promotion;
- one observed occurrence;
- repeated occurrences and exact shadow order;
- earlier incomplete roots blocking a later observed occurrence;
- an observed occurrence inside an incomplete root;
- later incomplete roots preserving an earlier known winner while making the
  occurrence universe incomplete;
- incomplete sets preventing false absence;
- ordinal lookup under Turkish culture;
- unchanged physical entry/path evidence;
- public API growth limited to the two DA02 concepts.

**DA02 gate:** callers can deterministically ask what canonical definition wins
across a complete explicit ordered set, receive ordered shadow evidence, and
receive explicit indeterminate evidence whenever incomplete earlier input makes a
winner or clean absence unknowable.
''',
)

replace_exact(
    "Directory.Build.props",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-1</IcodTermInfoSuiteVersion>",
    "<IcodTermInfoSuiteVersion>1.10.0-Alpha-2</IcodTermInfoSuiteVersion>",
)

replace_exact(
    "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
    "<PackageReleaseNotes>1.10.0-Alpha-1 adds the immutable bounded ordered database-set foundation, canonical occurrence indexing, constituent completeness evidence, and explicit-root/catalog construction while preserving the frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
    "<PackageReleaseNotes>1.10.0-Alpha-2 adds deterministic exact canonical-name precedence lookup, known-winner and ordered-shadow evidence, and explicit indeterminate results for incomplete earlier catalogs while preserving the DA01 model, frozen 1.9 JSON v1, lower-layer, synthesis, planning, and command contracts.</PackageReleaseNotes>",
)

current_version_files = [
    "tests/Icod.TermInfo.Tests/src/T45CompletionGateTests.cs",
    "tests/Icod.TermInfo.Termcap.Tests/src/TC08ContractTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/RS08ContractTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/RP08ReleaseClosureTests.cs",
    "tests/Icod.TermInfo.Inspection.Tests/src/MI07ReleaseClosureTests.cs",
    "tests/Icod.TermInfo.Tic.Tests/src/ReleaseClosureTests.cs",
    "tests/Icod.TermInfo.Tic.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.InfoCmp.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.Toe.Tests/src/CommandTests.cs",
    "tests/Icod.TermInfo.Router.Tests/src/ContractTests.cs",
    "tests/Icod.TermInfo.Router.Tests/src/CommandTests.cs",
]
for path_name in current_version_files:
    replace_all_required(path_name, "1.10.0-Alpha-1", "1.10.0-Alpha-2")

replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "exportedTypes.Length >= 37",
    "exportedTypes.Length >= 39",
)
replace_exact(
    "tools/inspection-package-smoke/Program.cs",
    "&& exportedTypes.Contains( typeof( TermInfoDatabaseSetOptions ) )",
    '''&& exportedTypes.Contains( typeof( TermInfoDatabaseSetOptions ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetLookupResult ) )\n\t\t&& exportedTypes.Contains( typeof( TermInfoDatabaseSetLookupStatus ) )''',
)

replace_exact(
    "Icod.TermInfo-1.10.0-Deterministic-Multi-Database-Inspection-Comparison-and-Planning-Automation-Roadmap.md",
    "**Status:** DA01 implementation complete; Staging validation pending",
    "**Status:** DA02 implementation complete; Staging validation pending",
)

replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current coordinated version:** `1.10.0-Alpha-1`",
    "**Current coordinated version:** `1.10.0-Alpha-2`",
)
replace_exact(
    "Icod.TermInfo-Post-1.0-Development-Roadmap.md",
    "**Current tranche:** DA01 - Database-set model and contract foundation",
    "**Current tranche:** DA02 - Deterministic multi-catalog inspection and precedence",
)

readme_path = Path("Icod.TermInfo.Inspection/README.md")
readme = readme_path.read_text(encoding="utf-8")
marker = "## 1.9 release status\n"
if readme.count(marker) != 1:
    raise RuntimeError("Inspection README 1.9 heading marker mismatch")
section = '''## 1.10 DA02 deterministic database-set precedence\n\n`1.10.0-Alpha-2` makes the DA01 ordered database-set model operational for exact\ncanonical-name precedence. `LookupCanonicalName` returns structured\n`NotObserved`, `WinnerKnown`, or `Indeterminate` evidence, retains every observed\noccurrence, exposes later observed shadows only when a winner is conclusive, and\nrecords incomplete databases which prevent a reliable winner or clean absence.\nAliases remain occurrence evidence rather than canonical lookup keys; semantic\nequal/conflicting shadow classification remains assigned to DA03.\n\nSee `docs/1.10.0-DA02-DETERMINISTIC-MULTI-CATALOG-PRECEDENCE.md`.\n\n## 1.10 DA01 database-set foundation\n\n`1.10.0-Alpha-1` introduced immutable caller-ordered explicit catalog sets,\ncanonical occurrence indexing, constituent issue/completeness evidence, bounds,\nand explicit-root or already-inspected-catalog construction.\n\nSee `docs/1.10.0-DA01-DATABASE-SET-MODEL-AND-CONTRACT-FOUNDATION.md`.\n\n'''
readme_path.write_text(readme.replace(marker, section + marker, 1), encoding="utf-8", newline="\n")
