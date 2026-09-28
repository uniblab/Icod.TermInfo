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

/// <summary>An immutable catalog observation of one explicitly selected source.</summary>
public sealed class TerminalCatalog {
	internal TerminalCatalog( TerminalCatalogSource source, TerminalCatalogStatus status,
		IEnumerable<TerminalCatalogEntry> entries, IEnumerable<TerminalCatalogIssue> issues,
		IEnumerable<string> duplicatePublicationNames
	) {
		ArgumentNullException.ThrowIfNull( source );
		CatalogValidation.Defined( status, nameof( status ) );
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( issues );
		ArgumentNullException.ThrowIfNull( duplicatePublicationNames );
		TerminalCatalogEntry[] entryArray = entries.ToArray();
		TerminalCatalogIssue[] issueArray = issues.ToArray();
		string[] duplicates = duplicatePublicationNames.ToArray();
		if ( entryArray.Any( item => item is null ) ) {
			throw new ArgumentException( "Entries cannot contain null.", nameof( entries ) );
		}
		if ( issueArray.Any( item => item is null ) ) {
			throw new ArgumentException( "Issues cannot contain null.", nameof( issues ) );
		}
		if ( duplicates.Any( string.IsNullOrWhiteSpace ) ) {
			throw new ArgumentException( "Duplicate names cannot be blank.", nameof( duplicatePublicationNames ) );
		}
		if ( entryArray.Any( item => !string.Equals( item.SourcePath, source.Path, StringComparison.Ordinal )
			|| (item.EntryPath is null) != (source.Kind == TerminalCatalogSourceKind.BerkeleyDbHash)
		)
		) {
			throw new ArgumentException( "Entry provenance must match the source.", nameof( entries ) );
		}
		if ( issueArray.Any( item => !string.Equals( item.SourcePath, source.Path, StringComparison.Ordinal ) ) ) {
			throw new ArgumentException( "Issue provenance must match the source.", nameof( issues ) );
		}
		if ( (status == TerminalCatalogStatus.Complete && issueArray.Length != 0)
			|| (status != TerminalCatalogStatus.Complete && issueArray.Length == 0)
			|| (status is not (TerminalCatalogStatus.Complete or TerminalCatalogStatus.Partial) && entryArray.Length != 0)
		) {
			throw new ArgumentException( "Status does not describe the supplied observations.", nameof( status ) );
		}
		string[] repeated = entryArray.GroupBy( entry => entry.PublicationName, StringComparer.Ordinal )
			.Where( group => group.Count() > 1 ).Select( group => group.Key )
			.OrderBy( name => name, StringComparer.Ordinal ).ToArray();
		Array.Sort( duplicates, StringComparer.Ordinal );
		if ( !duplicates.SequenceEqual( repeated, StringComparer.Ordinal ) ) {
			throw new ArgumentException( "Duplicate names must identify each repeated publication exactly once.", nameof( duplicatePublicationNames ) );
		}
		TerminalCatalogIssue[] duplicateIssues = issueArray.Where( item => item.Kind == TerminalCatalogIssueKind.DuplicatePublication ).ToArray();
		if ( duplicateIssues.Any( item => item.EntryPath is not null || item.PublicationName is null )
			|| !duplicateIssues.Select( item => item.PublicationName! ).OrderBy( name => name, StringComparer.Ordinal ).SequenceEqual( repeated, StringComparer.Ordinal )
		) {
			throw new ArgumentException( "Each repeated publication needs exactly one source-level duplicate issue.", nameof( issues ) );
		}
		Source = source; Status = status;
		Entries = Array.AsReadOnly( entryArray.OrderBy( item => item.PublicationName, StringComparer.Ordinal )
			.ThenBy( item => item.EntryPath ?? item.SourcePath, StringComparer.Ordinal ).ThenBy( item => item.Kind ).ToArray()
		);
		Issues = Array.AsReadOnly( issueArray.OrderBy( item => item.EntryPath ?? item.SourcePath, StringComparer.Ordinal )
			.ThenBy( item => item.Kind ).ThenBy( item => item.PublicationName ?? "", StringComparer.Ordinal )
			.ThenBy( item => item.Message, StringComparer.Ordinal ).ToArray()
		);
		DuplicatePublicationNames = Array.AsReadOnly( duplicates );
	}
	/// <summary>Gets the explicitly selected source.</summary>
	public TerminalCatalogSource Source { get; }
	/// <summary>Gets observation completeness and availability.</summary>
	public TerminalCatalogStatus Status { get; }
	/// <summary>Gets observed publication occurrences in deterministic ordinal order.</summary>
	public IReadOnlyList<TerminalCatalogEntry> Entries { get; }
	/// <summary>Gets typed acquisition diagnostics in deterministic order.</summary>
	public IReadOnlyList<TerminalCatalogIssue> Issues { get; }
	/// <summary>Gets distinct publication names with multiple observed occurrences.</summary>
	public IReadOnlyList<string> DuplicatePublicationNames { get; }
	/// <summary>Gets whether any diagnostic was recorded.</summary>
	public bool HasIssues => Issues.Count != 0;
}
