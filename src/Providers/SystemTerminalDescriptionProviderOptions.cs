/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
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

namespace Icod.TermInfo;

/// <summary>
/// Configures which system terminfo discovery sources a system provider is
/// permitted to consult.
/// </summary>
/// <remarks>
/// <see cref="UseEnvironment"/> controls <c>TERMINFO</c> and
/// <c>TERMINFO_DIRS</c>. <see cref="UseUserDatabase"/> controls the non-Windows
/// user <c>.terminfo</c> source. <see cref="UseSystemDatabases"/> controls
/// implicit platform roots and expansion of empty <c>TERMINFO_DIRS</c>
/// components to those roots. The three controls are independent.
/// </remarks>
public sealed class SystemTerminalDescriptionProviderOptions {
	/// <summary>
	/// Initializes immutable system-discovery options.
	/// </summary>
	/// <param name="useEnvironment">
	/// Whether environment-controlled discovery inputs may be consulted.
	/// </param>
	/// <param name="useUserDatabase">
	/// Whether user-local terminfo discovery may be consulted.
	/// </param>
	/// <param name="useSystemDatabases">
	/// Whether platform system terminfo databases may be consulted.
	/// </param>
	/// <param name="parserOptions">
	/// Optional compiled-entry parser limits. Values are snapshotted by this
	/// options instance.
	/// </param>
	public SystemTerminalDescriptionProviderOptions(
		bool useEnvironment = true,
		bool useUserDatabase = true,
		bool useSystemDatabases = true,
		CompiledTermInfoParserOptions? parserOptions = null
	) {
		UseEnvironment = useEnvironment;
		UseUserDatabase = useUserDatabase;
		UseSystemDatabases = useSystemDatabases;

		CompiledTermInfoParserOptions effectiveParserOptions =
			parserOptions ?? new CompiledTermInfoParserOptions();
		ParserOptions =
			new CompiledTermInfoParserOptions(
				effectiveParserOptions.MaximumEntrySize
			);
	}

	/// <summary>
	/// Gets whether environment-controlled discovery inputs may be consulted.
	/// </summary>
	public bool UseEnvironment { get; }

	/// <summary>
	/// Gets whether user-local terminfo discovery may be consulted.
	/// </summary>
	public bool UseUserDatabase { get; }

	/// <summary>
	/// Gets whether platform system terminfo databases may be consulted.
	/// </summary>
	public bool UseSystemDatabases { get; }

	/// <summary>
	/// Gets the immutable compiled-entry parser limits.
	/// </summary>
	public CompiledTermInfoParserOptions ParserOptions { get; }
}
