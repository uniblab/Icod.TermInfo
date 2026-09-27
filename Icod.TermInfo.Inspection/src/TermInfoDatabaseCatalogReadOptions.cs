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

/// <summary>Immutable resource budgets for bounded conventional-directory inspection.</summary>
public sealed class TermInfoDatabaseCatalogReadOptions {
	/// <summary>Initializes independent, inclusive acquisition budgets.</summary>
	/// <param name="parserOptions">Per-entry parser limits, copied into these options.</param>
	/// <param name="maximumCandidateCount">Maximum discovered children, including ignored children.</param>
	/// <param name="maximumEntryCount">Maximum retained physical parses.</param>
	/// <param name="maximumIssueCount">Maximum retained inspection issues.</param>
	/// <param name="maximumParsedBytes">Maximum cumulative bytes admitted for parsing.</param>
	public TermInfoDatabaseCatalogReadOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumCandidateCount = 131_072,
		int maximumEntryCount = 65_536,
		int maximumIssueCount = 4_096,
		long maximumParsedBytes = 67_108_864
	) {
		CompiledTermInfoParserOptions parser = parserOptions ?? new();
		ParserOptions = new( parser.MaximumEntrySize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumCandidateCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumEntryCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumIssueCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumParsedBytes );
		MaximumCandidateCount = maximumCandidateCount;
		MaximumEntryCount = maximumEntryCount;
		MaximumIssueCount = maximumIssueCount;
		MaximumParsedBytes = maximumParsedBytes;
	}
	/// <summary>Gets the per-entry parser options snapshot.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum discovered candidate count.</summary>
	public int MaximumCandidateCount { get; }
	/// <summary>Gets the maximum retained physical-entry count.</summary>
	public int MaximumEntryCount { get; }
	/// <summary>Gets the maximum retained issue count.</summary>
	public int MaximumIssueCount { get; }
	/// <summary>Gets the maximum cumulative input bytes admitted for parsing.</summary>
	public long MaximumParsedBytes { get; }
}
