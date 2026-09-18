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

/// <summary>Describes one compiled terminal entry for Hash-v9 publication.</summary>
public sealed class BerkeleyDbTerminalDatabaseEntry {
	private readonly byte[] _data;

	/// <summary>Initializes an immutable terminal-database entry.</summary>
	/// <param name="canonicalName">The compiled entry's canonical terminal name.</param>
	/// <param name="aliases">The compiled entry's aliases in publication order.</param>
	/// <param name="data">The complete compiled terminfo entry.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="canonicalName"/>, <paramref name="aliases"/>, or
	/// <paramref name="data"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="aliases"/> contains a <see langword="null"/> element.
	/// </exception>
	public BerkeleyDbTerminalDatabaseEntry(
		string canonicalName,
		IEnumerable<string> aliases,
		byte[] data
	) {
		ArgumentNullException.ThrowIfNull( canonicalName );
		ArgumentNullException.ThrowIfNull( aliases );
		ArgumentNullException.ThrowIfNull( data );

		string[] aliasSnapshot = aliases.ToArray();
		if ( aliasSnapshot.Any( static alias => alias is null ) ) {
			throw new ArgumentException(
				"The alias sequence cannot contain null.",
				nameof( aliases )
			);
		}

		CanonicalName = canonicalName;
		Aliases = Array.AsReadOnly( aliasSnapshot );
		_data = (byte[])data.Clone();
	}

	/// <summary>Gets the canonical terminal name.</summary>
	public string CanonicalName { get; }
	/// <summary>Gets the immutable alias snapshot in publication order.</summary>
	public IReadOnlyList<string> Aliases { get; }
	/// <summary>Gets a copy of the complete compiled terminfo entry.</summary>
	public byte[] Data => (byte[])_data.Clone();
}
