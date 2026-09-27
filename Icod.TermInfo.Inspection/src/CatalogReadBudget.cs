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

internal sealed class CatalogReadBudget {
	private readonly string _sourcePath;
	private readonly TermInfoDatabaseCatalogReadOptions _options;
	private int _candidates;
	private int _entries;
	private int _issues;
	private long _parsedBytes;

	internal CatalogReadBudget( string sourcePath, TermInfoDatabaseCatalogReadOptions options ) {
		_sourcePath = sourcePath;
		_options = options;
	}
	internal void ReserveCandidate() => Reserve( ref _candidates, _options.MaximumCandidateCount, "MaximumCandidateCount" );
	internal void EnsureEntryCapacity() {
		if ( _entries >= _options.MaximumEntryCount ) ThrowLimit( "MaximumEntryCount", _options.MaximumEntryCount );
	}
	internal void ReserveEntry() => Reserve( ref _entries, _options.MaximumEntryCount, "MaximumEntryCount" );
	internal void ReserveIssue() => Reserve( ref _issues, _options.MaximumIssueCount, "MaximumIssueCount" );
	internal void ReserveParsedBytes( long length ) {
		ArgumentOutOfRangeException.ThrowIfNegative( length );
		if ( length > _options.MaximumParsedBytes - _parsedBytes ) ThrowLimit( "MaximumParsedBytes", _options.MaximumParsedBytes );
		_parsedBytes += length;
	}
	internal void ThrowLimit( string name, long limit ) => throw new TermInfoDatabaseCatalogLimitException( _sourcePath, name, limit );
	private void Reserve( ref int count, int limit, string name ) {
		if ( count >= limit ) ThrowLimit( name, limit );
		count++;
	}
}
