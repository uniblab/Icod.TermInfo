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

/// <summary>One observed publication occurrence and its immutable parsed terminal.</summary>
public sealed class TerminalCatalogEntry {
	internal TerminalCatalogEntry( string publicationName, TerminalCatalogEntryKind kind,
		TerminalDescription terminal, string sourcePath, string? entryPath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( publicationName );
		CatalogValidation.Defined( kind, nameof( kind ) );
		ArgumentNullException.ThrowIfNull( terminal );
		CatalogValidation.AbsolutePath( sourcePath, nameof( sourcePath ) );
		if ( entryPath is not null ) {
			CatalogValidation.AbsolutePath( entryPath, nameof( entryPath ) );
		}
		bool canonical = string.Equals( publicationName, terminal.Name, StringComparison.Ordinal );
		if ( kind == TerminalCatalogEntryKind.Canonical ? !canonical
			: canonical || !terminal.Aliases.Contains( publicationName, StringComparer.Ordinal )
		) {
			throw new ArgumentException( "The publication name and kind must match the parsed terminal.", nameof( publicationName ) );
		}
		PublicationName = publicationName;
		Kind = kind;
		Terminal = terminal;
		SourcePath = sourcePath;
		EntryPath = entryPath;
	}
	/// <summary>Gets the name actually observed in this storage publication.</summary>
	public string PublicationName { get; }
	/// <summary>Gets whether the name is canonical or an alias.</summary>
	public TerminalCatalogEntryKind Kind { get; }
	/// <summary>Gets the immutable terminal, including canonical identity and declared aliases.</summary>
	public TerminalDescription Terminal { get; }
	/// <summary>Gets the absolute directory root or hashed database path.</summary>
	public string SourcePath { get; }
	/// <summary>Gets the actual directory entry path, or null for a hashed key.</summary>
	public string? EntryPath { get; }
}
