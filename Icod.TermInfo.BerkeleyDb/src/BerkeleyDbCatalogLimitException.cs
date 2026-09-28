/*
	Icod.TermInfo.BerkeleyDb
	Bounded acquisition of ncurses-compatible Berkeley DB Hash-v9 catalogs.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Indicates that bounded hashed catalog acquisition exhausted a configured budget.</summary>
/// <remarks>No partial result is returned. Limit names, not message text, identify the budget.</remarks>
public sealed class BerkeleyDbCatalogLimitException : Exception {
	/// <summary>Initializes a failure identifying the source and exhausted budget.</summary>
	/// <param name="sourcePath">Fully qualified database path.</param>
	/// <param name="limitName">MaximumDatabaseSize, MaximumRecordCount, MaximumIndexHops, MaximumDecodedBytes, MaximumParsedBytes, MaximumEntrySize, MaximumStoredItemSize, or MaximumPublicationCount.</param>
	/// <param name="limit">Inclusive maximum; positive except MaximumIndexHops may be zero.</param>
	/// <param name="innerException">Optional underlying failure.</param>
	public BerkeleyDbCatalogLimitException(
		string sourcePath, string limitName, long limit, Exception? innerException = null
	) : base( $"Catalog limit '{limitName}' ({limit}) was exceeded.", innerException ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );
		if ( !Path.IsPathFullyQualified( sourcePath ) ) {
			throw new ArgumentException( "The source path must be fully qualified.", nameof( sourcePath ) );
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( limitName );
		if ( limitName is not ("MaximumDatabaseSize" or "MaximumRecordCount" or "MaximumIndexHops" or "MaximumDecodedBytes" or "MaximumParsedBytes" or "MaximumEntrySize" or "MaximumStoredItemSize" or "MaximumPublicationCount") ) {
			throw new ArgumentException( "Unknown hashed catalog limit.", nameof( limitName ) );
		}
		if ( limitName == "MaximumIndexHops" ) {
			ArgumentOutOfRangeException.ThrowIfNegative( limit );
		} else {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( limit );
		}
		SourcePath = sourcePath;
		LimitName = limitName;
		Limit = limit;
	}
	/// <summary>Gets the fully qualified database path.</summary>
	public string SourcePath { get; }
	/// <summary>Gets the stable name of the exhausted budget.</summary>
	public string LimitName { get; }
	/// <summary>Gets the inclusive configured maximum.</summary>
	public long Limit { get; }
}
