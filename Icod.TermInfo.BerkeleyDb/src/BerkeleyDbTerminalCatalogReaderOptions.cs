/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Configures immutable resource limits for hashed catalog enumeration.</summary>
public sealed class BerkeleyDbTerminalCatalogReaderOptions {
	/// <summary>The default maximum number of Hash key/value records.</summary>
	public const int DefaultMaximumRecordCount = 65_536;

	/// <summary>Initializes immutable catalog reader options.</summary>
	public BerkeleyDbTerminalCatalogReaderOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumRecordCount = DefaultMaximumRecordCount,
		int maximumIndexHops =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumIndexHops
	) {
		if ( maximumDatabaseSize <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumDatabaseSize ),
				maximumDatabaseSize,
				"The maximum database size must be greater than zero."
			);
		}
		if ( maximumRecordCount <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumRecordCount ),
				maximumRecordCount,
				"The maximum record count must be greater than zero."
			);
		}
		if (
			maximumIndexHops < 0
			|| maximumIndexHops
				> BerkeleyDbTerminalDescriptionProviderOptions
					.MaximumSupportedIndexHops
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumIndexHops ),
				maximumIndexHops,
				$"The maximum index-hop count must be between 0 and {BerkeleyDbTerminalDescriptionProviderOptions.MaximumSupportedIndexHops}."
			);
		}

		CompiledTermInfoParserOptions effectiveParserOptions =
			parserOptions ?? new CompiledTermInfoParserOptions();
		ParserOptions =
			new CompiledTermInfoParserOptions(
				effectiveParserOptions.MaximumEntrySize
			);
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumRecordCount = maximumRecordCount;
		MaximumIndexHops = maximumIndexHops;
	}

	/// <summary>Gets the compiled-entry parser options snapshot.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum database size in bytes.</summary>
	public int MaximumDatabaseSize { get; }
	/// <summary>Gets the maximum number of Hash key/value records.</summary>
	public int MaximumRecordCount { get; }
	/// <summary>Gets the maximum number of followed ncurses index links.</summary>
	public int MaximumIndexHops { get; }
}
