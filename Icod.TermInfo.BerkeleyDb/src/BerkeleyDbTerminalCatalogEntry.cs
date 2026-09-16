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

/// <summary>Describes one logical canonical or alias terminal-name publication.</summary>
public sealed class BerkeleyDbTerminalCatalogEntry {
	internal BerkeleyDbTerminalCatalogEntry(
		string name,
		BerkeleyDbTerminalCatalogEntryKind kind,
		TerminalDescription terminal
	) {
		ArgumentNullException.ThrowIfNull( name );
		ArgumentNullException.ThrowIfNull( terminal );

		Name = name;
		Kind = kind;
		Terminal = terminal;
	}

	/// <summary>Gets the exact published terminal name.</summary>
	public string Name { get; }
	/// <summary>Gets whether the published name is canonical or an alias.</summary>
	public BerkeleyDbTerminalCatalogEntryKind Kind { get; }
	/// <summary>Gets the parsed terminal description.</summary>
	public TerminalDescription Terminal { get; }
}
