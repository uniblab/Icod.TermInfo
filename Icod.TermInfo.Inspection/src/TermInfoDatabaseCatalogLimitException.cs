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

/// <summary>Indicates that bounded directory inspection exhausted a configured budget.</summary>
/// <remarks>No partial catalog is returned. Limit names are stable property names, not message text.</remarks>
public sealed class TermInfoDatabaseCatalogLimitException : Exception {
	/// <summary>Initializes a failure identifying the source and exhausted budget.</summary>
	/// <param name="sourcePath">Fully qualified source directory path.</param>
	/// <param name="limitName">MaximumCandidateCount, MaximumEntryCount, MaximumIssueCount, MaximumParsedBytes, or MaximumEntrySize.</param>
	/// <param name="limit">The inclusive positive maximum.</param>
	/// <param name="innerException">Optional underlying failure.</param>
	public TermInfoDatabaseCatalogLimitException(
		string sourcePath, string limitName, long limit, Exception? innerException = null
	) : base( $"Catalog limit '{limitName}' ({limit}) was exceeded.", innerException ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );
		if ( !Path.IsPathFullyQualified( sourcePath ) )
			throw new ArgumentException( "The source path must be fully qualified.", nameof( sourcePath ) );
		ArgumentException.ThrowIfNullOrWhiteSpace( limitName );
		if ( limitName is not ("MaximumCandidateCount" or "MaximumEntryCount" or "MaximumIssueCount" or "MaximumParsedBytes" or "MaximumEntrySize") )
			throw new ArgumentException( "Unknown directory catalog limit.", nameof( limitName ) );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( limit );
		SourcePath = sourcePath;
		LimitName = limitName;
		Limit = limit;
	}
	/// <summary>Gets the fully qualified source directory path.</summary>
	public string SourcePath { get; }
	/// <summary>Gets the stable name of the exhausted budget.</summary>
	public string LimitName { get; }
	/// <summary>Gets the inclusive configured maximum.</summary>
	public long Limit { get; }
}
