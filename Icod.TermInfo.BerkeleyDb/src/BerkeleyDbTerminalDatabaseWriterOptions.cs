/*
	Icod.TermInfo.BerkeleyDb
	Managed read and write support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
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

/// <summary>Configures immutable limits and destination policy for Hash-v9 writing.</summary>
public sealed class BerkeleyDbTerminalDatabaseWriterOptions {
	/// <summary>The default maximum physical Hash record count.</summary>
	public const int DefaultMaximumRecordCount = 65_536;

	/// <summary>Initializes immutable writer options.</summary>
	/// <param name="parserOptions">Compiled-entry parser limits, or defaults.</param>
	/// <param name="maximumDatabaseSize">The maximum output database size in bytes.</param>
	/// <param name="maximumRecordCount">The maximum physical Hash record count.</param>
	/// <param name="overwriteExisting">Whether an existing destination may be replaced.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="maximumDatabaseSize"/> or
	/// <paramref name="maximumRecordCount"/> is not positive.
	/// </exception>
	public BerkeleyDbTerminalDatabaseWriterOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumRecordCount = DefaultMaximumRecordCount,
		bool overwriteExisting = false
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumRecordCount );

		CompiledTermInfoParserOptions effectiveParserOptions =
			parserOptions ?? new CompiledTermInfoParserOptions();
		ParserOptions = new CompiledTermInfoParserOptions(
			effectiveParserOptions.MaximumEntrySize
		);
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumRecordCount = maximumRecordCount;
		OverwriteExisting = overwriteExisting;
	}

	/// <summary>Gets the compiled-entry parser options snapshot.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum output database size in bytes.</summary>
	public int MaximumDatabaseSize { get; }
	/// <summary>Gets the maximum physical Hash record count.</summary>
	public int MaximumRecordCount { get; }
	/// <summary>Gets whether an existing destination may be replaced.</summary>
	public bool OverwriteExisting { get; }
}
