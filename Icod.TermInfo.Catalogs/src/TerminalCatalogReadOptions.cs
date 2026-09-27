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

/// <summary>Immutable independent acquisition budgets for one catalog read.</summary>
public sealed class TerminalCatalogReadOptions {
	/// <summary>Initializes inclusive limits and snapshots the parser options.</summary>
	/// <param name="parserOptions">Per-entry parser limits; null selects Runtime defaults.</param>
	/// <param name="maximumCandidateCount">Maximum discovered filesystem children, including ignored candidates.</param>
	/// <param name="maximumEntryCount">Maximum retained physical parses or hashed publication rows.</param>
	/// <param name="maximumIssueCount">Maximum retained issues, including duplicate-publication issues.</param>
	/// <param name="maximumParsedBytes">Maximum cumulative compiled bytes admitted for parsing.</param>
	/// <param name="maximumDatabaseSize">Maximum hashed image bytes.</param>
	/// <param name="maximumRecordCount">Maximum physical hashed records.</param>
	/// <param name="maximumIndexHops">Maximum followed hashed index links per publication, from zero through 1024.</param>
	/// <param name="maximumDecodedBytes">Maximum cumulative decoded hashed key and value bytes.</param>
	public TerminalCatalogReadOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumCandidateCount = 131_072,
		int maximumEntryCount = 65_536,
		int maximumIssueCount = 4_096,
		long maximumParsedBytes = 67_108_864,
		int maximumDatabaseSize = 67_108_864,
		int maximumRecordCount = 65_536,
		int maximumIndexHops = 16,
		long maximumDecodedBytes = 67_108_864
	) {
		ParserOptions = new( (parserOptions ?? new()).MaximumEntrySize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumCandidateCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumEntryCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumIssueCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumParsedBytes );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumRecordCount );
		if ( maximumIndexHops < 0 || maximumIndexHops > 1024 ) throw new ArgumentOutOfRangeException( nameof( maximumIndexHops ) );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDecodedBytes );
		MaximumCandidateCount = maximumCandidateCount;
		MaximumEntryCount = maximumEntryCount;
		MaximumIssueCount = maximumIssueCount;
		MaximumParsedBytes = maximumParsedBytes;
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumRecordCount = maximumRecordCount;
		MaximumIndexHops = maximumIndexHops;
		MaximumDecodedBytes = maximumDecodedBytes;
	}
	/// <summary>Gets the per-entry parser options snapshot.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum discovered filesystem children, including ignored candidates.</summary>
	public int MaximumCandidateCount { get; }
	/// <summary>Gets the maximum retained physical parses or hashed publication rows.</summary>
	public int MaximumEntryCount { get; }
	/// <summary>Gets the maximum retained issues, including duplicate-publication issues.</summary>
	public int MaximumIssueCount { get; }
	/// <summary>Gets the maximum cumulative compiled bytes admitted for parsing.</summary>
	public long MaximumParsedBytes { get; }
	/// <summary>Gets the maximum hashed image bytes.</summary>
	public int MaximumDatabaseSize { get; }
	/// <summary>Gets the maximum physical hashed records.</summary>
	public int MaximumRecordCount { get; }
	/// <summary>Gets the maximum followed hashed index links per publication, from zero through 1024.</summary>
	public int MaximumIndexHops { get; }
	/// <summary>Gets the maximum cumulative decoded hashed key and value bytes.</summary>
	public long MaximumDecodedBytes { get; }
}
