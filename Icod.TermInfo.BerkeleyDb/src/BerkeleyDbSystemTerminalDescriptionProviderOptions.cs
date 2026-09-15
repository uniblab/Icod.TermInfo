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

/// <summary>Configures immutable hashed-aware system discovery policy and resource limits.</summary>
public sealed class BerkeleyDbSystemTerminalDescriptionProviderOptions {
	/// <summary>Initializes immutable system-discovery options.</summary>
	public BerkeleyDbSystemTerminalDescriptionProviderOptions(
		bool useEnvironment = true,
		bool useUserDatabase = true,
		bool useSystemDatabases = true,
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumIndexHops =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumIndexHops
	) {
		UseEnvironment = useEnvironment;
		UseUserDatabase = useUserDatabase;
		UseSystemDatabases = useSystemDatabases;
		ParserOptions = parserOptions ?? new CompiledTermInfoParserOptions();
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumIndexHops = maximumIndexHops;
	}

	/// <summary>Gets whether environment-controlled discovery inputs may be consulted.</summary>
	public bool UseEnvironment { get; }
	/// <summary>Gets whether the user-local database may be consulted.</summary>
	public bool UseUserDatabase { get; }
	/// <summary>Gets whether platform system databases may be consulted.</summary>
	public bool UseSystemDatabases { get; }
	/// <summary>Gets compiled-entry parser options.</summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
	/// <summary>Gets the maximum hashed-database size.</summary>
	public int MaximumDatabaseSize { get; }
	/// <summary>Gets the maximum number of followed ncurses index links.</summary>
	public int MaximumIndexHops { get; }
}
