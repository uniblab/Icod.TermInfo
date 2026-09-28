/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/
using Icod.TermInfo.BerkeleyDb;

namespace Icod.TermInfo.Catalogs;

internal static class HashedTerminalCatalogAdapter {
	internal static TerminalCatalog Read(
		TerminalCatalogSource source, TerminalCatalogReadOptions options, CancellationToken cancellationToken
	) => ReadCore( source, options, cancellationToken, File.GetAttributes,
		static ( path, readerOptions, limits, token ) => new BerkeleyDbTerminalCatalogReader( path, readerOptions ).ReadBounded( limits, token )
	);

	internal static TerminalCatalog ReadCore(
		TerminalCatalogSource source, TerminalCatalogReadOptions options, CancellationToken cancellationToken,
		Func<string, FileAttributes> inspect,
		Func<string, BerkeleyDbTerminalCatalogReaderOptions, BerkeleyDbTerminalCatalogReadLimits,
			CancellationToken, IReadOnlyList<BerkeleyDbTerminalCatalogEntry>> acquire
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( inspect );
		ArgumentNullException.ThrowIfNull( acquire );
		if ( source.Kind != TerminalCatalogSourceKind.BerkeleyDbHash ) {
			throw new ArgumentException( "The hashed adapter requires a Berkeley DB Hash source.", nameof( source ) );
		}
		cancellationToken.ThrowIfCancellationRequested();
		IReadOnlyList<BerkeleyDbTerminalCatalogEntry> physical;
		try {
			FileAttributes attributes = inspect( source.Path );
			cancellationToken.ThrowIfCancellationRequested();
			if ( ( attributes & FileAttributes.Directory ) != 0 ) {
				return Failure( TerminalCatalogStatus.UnsupportedSource, TerminalCatalogIssueKind.UnsupportedSource,
					"The requested hashed source is a directory."
				);
			}
			var readerOptions = new BerkeleyDbTerminalCatalogReaderOptions( options.ParserOptions,
				options.MaximumDatabaseSize, options.MaximumRecordCount, options.MaximumIndexHops
			);
			var limits = new BerkeleyDbTerminalCatalogReadLimits( options.MaximumEntryCount,
				options.MaximumDecodedBytes, options.MaximumParsedBytes
			);
			cancellationToken.ThrowIfCancellationRequested();
			try {
				physical = acquire( source.Path, readerOptions, limits, cancellationToken );
			} catch ( BerkeleyDbCatalogLimitException exception ) {
				throw new TerminalCatalogLimitException( source,
					exception.LimitName == "MaximumPublicationCount" ? nameof( options.MaximumEntryCount ) : exception.LimitName,
					exception.Limit, exception
				);
			} catch ( BerkeleyDbDatabaseFormatException ) {
				return InvalidStore();
			} catch ( CompiledTermInfoFormatException ) {
				return InvalidStore();
			} catch ( InvalidDataException ) {
				return InvalidStore();
			}
		} catch ( FileNotFoundException ) {
			return Missing();
		} catch ( DirectoryNotFoundException ) {
			return Missing();
		} catch ( UnauthorizedAccessException ) {
			return Failure( TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.PermissionFailure,
				"The requested hashed source could not be accessed."
			);
		} catch ( IOException ) {
			return Failure( TerminalCatalogStatus.Unavailable, TerminalCatalogIssueKind.IoFailure,
				"An I/O failure prevented acquisition of the requested hashed source."
			);
		}

		cancellationToken.ThrowIfCancellationRequested();
		List<TerminalCatalogEntry> entries = [];
		foreach ( BerkeleyDbTerminalCatalogEntry entry in physical ) {
			cancellationToken.ThrowIfCancellationRequested();
			TerminalCatalogEntryKind kind = entry.Kind switch {
				BerkeleyDbTerminalCatalogEntryKind.Canonical => TerminalCatalogEntryKind.Canonical,
				BerkeleyDbTerminalCatalogEntryKind.Alias => TerminalCatalogEntryKind.Alias,
				_ => throw new ArgumentOutOfRangeException( nameof( physical ), "Unknown hashed publication kind." ),
			};
			entries.Add( new( entry.Name, kind, entry.Terminal, source.Path, null ) );
		}
		cancellationToken.ThrowIfCancellationRequested();
		TerminalCatalog result = new( source, TerminalCatalogStatus.Complete, entries, [], [] );
		cancellationToken.ThrowIfCancellationRequested();
		return result;

		TerminalCatalog Missing() => Failure( TerminalCatalogStatus.Missing, TerminalCatalogIssueKind.MissingSource,
			"The requested hashed source does not exist."
		);
		TerminalCatalog InvalidStore() => Failure( TerminalCatalogStatus.InvalidStore, TerminalCatalogIssueKind.InvalidHashedStore,
			"The requested hashed source contains malformed or unsupported data."
		);
		TerminalCatalog Failure( TerminalCatalogStatus status, TerminalCatalogIssueKind kind, string message ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Every source failure retains exactly one issue; reserve before construction.
			if ( options.MaximumIssueCount < 1 ) {
				throw new TerminalCatalogLimitException( source, nameof( options.MaximumIssueCount ), options.MaximumIssueCount );
			}
			TerminalCatalog failed = new( source, status, [], [new( kind, source.Path, null, null, message )], [] );
			cancellationToken.ThrowIfCancellationRequested();
			return failed;
		}
	}
}
