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

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Resolves a fixed, immutable set of terminal descriptions from memory.
/// </summary>
public sealed class InMemoryTerminalDescriptionProvider : ITerminalDescriptionProvider {
	private readonly IReadOnlyDictionary<string, TerminalDescription> _terminals;

	/// <summary>
	/// Initializes a provider from the specified terminal descriptions.
	/// </summary>
	public InMemoryTerminalDescriptionProvider(
		IEnumerable<TerminalDescription> terminals
	) {
		ArgumentNullException.ThrowIfNull( terminals );

		Dictionary<string, TerminalDescription> byName =
			new( StringComparer.Ordinal );

		foreach ( TerminalDescription terminal in terminals ) {
			ArgumentNullException.ThrowIfNull( terminal );

			AddName( byName, terminal.Name, terminal );

			foreach ( string alias in terminal.Aliases ) {
				AddName( byName, alias, terminal );
			}
		}

		_terminals = byName;
	}

	/// <inheritdoc/>
	public bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	) {
		ValidateTerminalName( name );
		return _terminals.TryGetValue( name, out terminal );
	}

	private static void AddName(
		IDictionary<string, TerminalDescription> terminals,
		string name,
		TerminalDescription terminal
	) {
		ArgumentNullException.ThrowIfNull( terminals );
		ArgumentNullException.ThrowIfNull( name );
		ArgumentNullException.ThrowIfNull( terminal );

		if ( terminals.ContainsKey( name ) ) {
			throw new ArgumentException(
				$"Duplicate terminal name or alias '{name}'.",
				nameof( terminals )
			);
		}

		terminals.Add( name, terminal );
	}

	private static void ValidateTerminalName( string name ) {
		ArgumentNullException.ThrowIfNull( name );

		if ( string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"The terminal name cannot be empty or whitespace.",
				nameof( name )
			);
		}
	}
}
