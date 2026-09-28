/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
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

namespace Icod.TermInfo.Catalogs;

/// <summary>Identifies one explicit, normalized source without acquiring it.</summary>
public sealed class TerminalCatalogSource {
	/// <summary>Initializes a source and resolves its path against the current directory once.</summary>
	/// <param name="path">Nonblank directory or database-file path.</param>
	/// <param name="kind">Explicit storage kind; no autodetection or discovery is performed.</param>
	public TerminalCatalogSource( string path, TerminalCatalogSourceKind kind ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );
		CatalogValidation.Defined( kind, nameof( kind ) );
		Path = System.IO.Path.GetFullPath( path );
		Kind = kind;
	}
	/// <summary>Gets the absolute path captured at construction.</summary>
	public string Path { get; }
	/// <summary>Gets the explicit storage kind.</summary>
	public TerminalCatalogSourceKind Kind { get; }
}
