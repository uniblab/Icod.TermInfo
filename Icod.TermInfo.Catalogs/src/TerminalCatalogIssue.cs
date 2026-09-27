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

/// <summary>A typed acquisition diagnostic with storage provenance.</summary>
public sealed class TerminalCatalogIssue {
	internal TerminalCatalogIssue( TerminalCatalogIssueKind kind, string sourcePath,
		string? entryPath, string? publicationName, string message ) {
		CatalogValidation.Defined( kind, nameof( kind ) );
		CatalogValidation.AbsolutePath( sourcePath, nameof( sourcePath ) );
		if ( entryPath is not null ) CatalogValidation.AbsolutePath( entryPath, nameof( entryPath ) );
		if ( publicationName is not null ) ArgumentException.ThrowIfNullOrWhiteSpace( publicationName );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );
		Kind = kind; SourcePath = sourcePath; EntryPath = entryPath;
		PublicationName = publicationName; Message = message;
	}
	/// <summary>Gets the stable diagnostic category.</summary>
	public TerminalCatalogIssueKind Kind { get; }
	/// <summary>Gets the absolute directory root or database file path.</summary>
	public string SourcePath { get; }
	/// <summary>Gets the affected directory child path, or null for a whole-source issue.</summary>
	public string? EntryPath { get; }
	/// <summary>Gets the affected publication name when known.</summary>
	public string? PublicationName { get; }
	/// <summary>Gets explanatory text, which is not a stable machine-readable code.</summary>
	public string Message { get; }
}
