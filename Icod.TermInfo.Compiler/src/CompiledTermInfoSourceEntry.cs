/*
	Icod.TermInfo.Compiler
	Compiles terminfo source and terminal descriptions into deterministic terminfo databases.
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

using Icod.TermInfo.Source;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Contains one independently loadable compiled terminfo entry produced from
/// source.
/// </summary>
public sealed class CompiledTermInfoSourceEntry {
	private readonly byte[] _data;

	internal CompiledTermInfoSourceEntry(
		TermInfoSourceEntry sourceEntry,
		byte[] data
	) {
		ArgumentNullException.ThrowIfNull( sourceEntry );
		ArgumentNullException.ThrowIfNull( data );

		CanonicalName = sourceEntry.CanonicalName;
		Aliases = sourceEntry.Aliases.ToArray();
		_data = (byte[])data.Clone();
	}

	/// <summary>
	/// Gets the canonical source entry name.
	/// </summary>
	public string CanonicalName { get; }

	/// <summary>
	/// Gets the source aliases in source order.
	/// </summary>
	public IReadOnlyList<string> Aliases { get; }

	/// <summary>
	/// Gets a copy of the complete compiled terminfo entry.
	/// </summary>
	/// <remarks>
	/// A new array is returned on every access so callers cannot mutate the
	/// compilation result retained by this object.
	/// </remarks>
	public byte[] Data {
		get {
			return (byte[])_data.Clone();
		}
	}
}
