/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

namespace Icod.TermInfo.Catalogs;

/// <summary>Reads actual publications from one explicitly selected source.</summary>
public sealed class TerminalCatalogReader {
	private readonly TerminalCatalogReadOptions _options;

	/// <summary>Initializes a reader and snapshots its resource limits.</summary>
	/// <param name="source">The explicit source and storage kind.</param>
	/// <param name="options">Immutable read limits; null selects defaults.</param>
	public TerminalCatalogReader( TerminalCatalogSource source, TerminalCatalogReadOptions? options = null ) {
		ArgumentNullException.ThrowIfNull( source );
		Source = source;
		TerminalCatalogReadOptions selected = options ?? new();
		_options = new TerminalCatalogReadOptions(
			selected.ParserOptions, selected.MaximumCandidateCount, selected.MaximumEntryCount,
			selected.MaximumIssueCount, selected.MaximumParsedBytes, selected.MaximumDatabaseSize,
			selected.MaximumRecordCount, selected.MaximumIndexHops, selected.MaximumDecodedBytes
		);
	}

	/// <summary>Gets the source selected when the reader was created.</summary>
	public TerminalCatalogSource Source { get; }

	/// <summary>Acquires a fresh catalog from the explicit source.</summary>
	public TerminalCatalog Read() => Read( default );

	/// <summary>Acquires a fresh catalog with cancellation and bounded input work.</summary>
	/// <param name="cancellationToken">Cancellation checked before and throughout acquisition.</param>
	public TerminalCatalog Read( CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		return Source.Kind switch {
			TerminalCatalogSourceKind.ConventionalDirectory =>
				ConventionalTerminalCatalogAdapter.Read( Source, _options, cancellationToken ),
			TerminalCatalogSourceKind.BerkeleyDbHash =>
				HashedTerminalCatalogAdapter.Read( Source, _options, cancellationToken ),
			_ => throw new ArgumentOutOfRangeException( nameof( Source ), "Unsupported catalog source kind." ),
		};
	}
}
