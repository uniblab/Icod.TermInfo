/*
	Icod.TermInfo.BerkeleyDb
	Bounded acquisition of ncurses-compatible Berkeley DB Hash-v9 catalogs.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

namespace Icod.TermInfo.BerkeleyDb;

internal sealed class BerkeleyDbCatalogReadBudget(
	string sourcePath, BerkeleyDbTerminalCatalogReaderOptions options,
	BerkeleyDbTerminalCatalogReadLimits limits, CancellationToken cancellationToken
) {
	private long _records;
	private long _decoded;
	private long _publications;
	private long _parsed;

	internal CancellationToken CancellationToken { get; } = cancellationToken;
	internal void CheckImage( long length ) => Check( length, options.MaximumDatabaseSize, nameof( options.MaximumDatabaseSize ) );
	internal void CheckStoredItem( long length ) => Check( length, (long)options.ParserOptions.MaximumEntrySize + 1, "MaximumStoredItemSize" );
	internal void CheckEntry( long length ) => Check( length, options.ParserOptions.MaximumEntrySize, nameof( options.ParserOptions.MaximumEntrySize ) );
	internal void CheckIndexHop( int followedLinks ) => Check( (long)followedLinks + 1, options.MaximumIndexHops, nameof( options.MaximumIndexHops ) );
	internal void ReserveRecord() => Reserve( ref _records, 1, options.MaximumRecordCount, nameof( options.MaximumRecordCount ) );
	internal void ReserveDecoded( long length ) => Reserve( ref _decoded, length, limits.MaximumDecodedBytes, nameof( limits.MaximumDecodedBytes ) );
	internal void ReservePublication() => Reserve( ref _publications, 1, limits.MaximumPublicationCount, nameof( limits.MaximumPublicationCount ) );
	internal void ReserveParsed( long length ) => Reserve( ref _parsed, length, limits.MaximumParsedBytes, nameof( limits.MaximumParsedBytes ) );

	private void Check( long length, long maximum, string name ) {
		CancellationToken.ThrowIfCancellationRequested();
		if ( length > maximum ) {
			throw new BerkeleyDbCatalogLimitException( sourcePath, name, maximum );
		}
	}
	private void Reserve( ref long used, long length, long maximum, string name ) {
		CancellationToken.ThrowIfCancellationRequested();
		if ( length > maximum - used ) {
			throw new BerkeleyDbCatalogLimitException( sourcePath, name, maximum );
		}
		used += length;
	}
}
