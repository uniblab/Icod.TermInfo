/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable inspection snapshot of an explicit terminfo
/// database root.
/// </summary>
public sealed class TermInfoDatabaseCatalog {
	internal TermInfoDatabaseCatalog(
		string root,
		TermInfoDatabaseCatalogKind kind,
		IEnumerable<TermInfoDatabaseCatalogEntry> entries,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues,
		IEnumerable<string> duplicateCanonicalNames
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( issues );
		ArgumentNullException.ThrowIfNull( duplicateCanonicalNames );

		if ( !System.IO.Path.IsPathFullyQualified( root ) ) {
			throw new ArgumentException(
				"A terminfo database catalog root must be fully qualified.",
				nameof( root )
			);
		}

		TermInfoDatabaseCatalogEntry[] entryArray =
			entries.ToArray();
		TermInfoDatabaseCatalogIssue[] issueArray =
			issues.ToArray();
		string[] duplicateArray =
			duplicateCanonicalNames.ToArray();

		if ( entryArray.Any( entry => entry is null ) ) {
			throw new ArgumentException(
				"A catalog entry collection cannot contain null.",
				nameof( entries )
			);
		}

		if ( issueArray.Any( issue => issue is null ) ) {
			throw new ArgumentException(
				"A catalog issue collection cannot contain null.",
				nameof( issues )
			);
		}

		if ( duplicateArray.Any( string.IsNullOrWhiteSpace ) ) {
			throw new ArgumentException(
				"Duplicate canonical names cannot contain null, empty, or whitespace values.",
				nameof( duplicateCanonicalNames )
			);
		}

		Root = root;
		Kind = kind;
		Entries = Array.AsReadOnly( entryArray );
		Issues = Array.AsReadOnly( issueArray );
		DuplicateCanonicalNames = Array.AsReadOnly( duplicateArray );
	}

	/// <summary>
	/// Gets the normalized absolute database root supplied to the inspection.
	/// </summary>
	public string Root {
		get;
	}

	/// <summary>
	/// Gets the observed storage state of the requested root.
	/// </summary>
	public TermInfoDatabaseCatalogKind Kind {
		get;
	}

	/// <summary>
	/// Gets successfully parsed physical entry files in deterministic order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseCatalogEntry> Entries {
		get;
	}

	/// <summary>
	/// Gets non-fatal inspection issues in deterministic order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseCatalogIssue> Issues {
		get;
	}

	/// <summary>
	/// Gets canonical terminal names represented by more than one successfully
	/// parsed physical file.
	/// </summary>
	public IReadOnlyList<string> DuplicateCanonicalNames {
		get;
	}

	/// <summary>
	/// Gets whether any non-fatal inspection issue was observed.
	/// </summary>
	public bool HasIssues =>
		Issues.Count != 0;
}
