/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
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

namespace Icod.TermInfo.Catalogs;

/// <summary>Indicates a configured acquisition budget was exhausted; no partial catalog is returned.</summary>
public sealed class TerminalCatalogLimitException : Exception {
	/// <summary>Initializes a failure identifying its source and inclusive configured limit.</summary>
	/// <param name="source">Explicit source of the failed acquisition.</param>
	/// <param name="limitName">Stable common limit name, matching a read-option property or MaximumEntrySize/MaximumStoredItemSize.</param>
	/// <param name="limit">Positive maximum, or zero for MaximumIndexHops.</param>
	/// <param name="innerException">Optional lower-layer failure.</param>
	public TerminalCatalogLimitException( TerminalCatalogSource source, string limitName,
		long limit, Exception? innerException = null )
		: base( $"Catalog limit '{limitName}' ({limit}) was exceeded.", innerException ) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentException.ThrowIfNullOrWhiteSpace( limitName );
		if ( limitName is not ("MaximumCandidateCount" or "MaximumEntryCount" or "MaximumIssueCount"
			or "MaximumParsedBytes" or "MaximumDatabaseSize" or "MaximumRecordCount" or "MaximumIndexHops"
			or "MaximumDecodedBytes" or "MaximumEntrySize" or "MaximumStoredItemSize") )
			throw new ArgumentException( "Unknown catalog limit.", nameof( limitName ) );
		if ( limit < 0 || (limit == 0 && limitName != "MaximumIndexHops") )
			throw new ArgumentOutOfRangeException( nameof( limit ) );
		Source = source; LimitName = limitName; Limit = limit;
	}
	/// <summary>Gets the typed acquisition source; hides the base exception's textual Source property.</summary>
	public new TerminalCatalogSource Source { get; }
	/// <summary>Gets the stable common limit name.</summary>
	public string LimitName { get; }
	/// <summary>Gets the inclusive configured maximum.</summary>
	public long Limit { get; }
}
