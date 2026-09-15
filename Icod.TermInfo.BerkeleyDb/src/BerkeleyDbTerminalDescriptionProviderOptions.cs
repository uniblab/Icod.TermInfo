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

/// <summary>Configures immutable resource limits for explicit hashed-database acquisition.</summary>
public sealed class BerkeleyDbTerminalDescriptionProviderOptions {
	/// <summary>The default maximum database size, 64 MiB.</summary>
	public const int DefaultMaximumDatabaseSize = 64 * 1024 * 1024;
	/// <summary>The default maximum number of ncurses index links.</summary>
	public const int DefaultMaximumIndexHops = 16;
	/// <summary>The largest supported ncurses index-link limit.</summary>
	public const int MaximumSupportedIndexHops = 1024;

	/// <summary>Initializes immutable provider options.</summary>
	public BerkeleyDbTerminalDescriptionProviderOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize = DefaultMaximumDatabaseSize,
		int maximumIndexHops = DefaultMaximumIndexHops
	) {
		ParserOptions = parserOptions ?? new CompiledTermInfoParserOptions();
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumIndexHops = maximumIndexHops;
	}

	/// <summary>Gets the compiled-entry parser options.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum database size in bytes.</summary>
	public int MaximumDatabaseSize { get; }
	/// <summary>Gets the maximum number of followed index links.</summary>
	public int MaximumIndexHops { get; }
}
