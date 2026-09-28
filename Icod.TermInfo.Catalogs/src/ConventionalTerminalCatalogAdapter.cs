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

using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Catalogs;

internal static class ConventionalTerminalCatalogAdapter {
	internal static TerminalCatalog Read(
		TerminalCatalogSource source, TerminalCatalogReadOptions options,
		CancellationToken cancellationToken
	) => ReadCore( source, options, cancellationToken, TermInfoDatabaseInspector.InspectDirectoryBounded );

	internal static TerminalCatalog ReadCore(
		TerminalCatalogSource source, TerminalCatalogReadOptions options,
		CancellationToken cancellationToken,
		Func<string, TermInfoDatabaseCatalogReadOptions, CancellationToken, TermInfoDatabaseCatalog> acquire
	) {
		ValidateInputs( source, options );
		ArgumentNullException.ThrowIfNull( acquire );
		cancellationToken.ThrowIfCancellationRequested();
		TermInfoDatabaseCatalogReadOptions acquisitionOptions = new(
			options.ParserOptions, options.MaximumCandidateCount, options.MaximumEntryCount,
			options.MaximumIssueCount, options.MaximumParsedBytes
		);
		TermInfoDatabaseCatalog physical;
		try {
			physical = acquire( source.Path, acquisitionOptions, cancellationToken );
		} catch ( TermInfoDatabaseCatalogLimitException exception ) {
			throw new TerminalCatalogLimitException( source, exception.LimitName, exception.Limit, exception );
		}
		return Normalize( source, options, physical, cancellationToken );
	}

	internal static TerminalCatalog Normalize(
		TerminalCatalogSource source, TerminalCatalogReadOptions options,
		TermInfoDatabaseCatalog physical, CancellationToken cancellationToken
	) {
		ValidateInputs( source, options );
		ArgumentNullException.ThrowIfNull( physical );
		if ( !string.Equals( physical.Root, source.Path, StringComparison.Ordinal ) ) {
			throw new ArgumentException( "The physical catalog must describe the requested source.", nameof( physical ) );
		}
		cancellationToken.ThrowIfCancellationRequested();
		List<TerminalCatalogIssue> issues = [];
		HashSet<string> invalidPaths = new( StringComparer.Ordinal );
		foreach ( TermInfoDatabaseCatalogIssue issue in physical.Issues ) {
			cancellationToken.ThrowIfCancellationRequested();
			AddIssue( MapIssueKind( issue.Kind ),
				string.Equals( issue.Path, source.Path, StringComparison.Ordinal ) ? null : issue.Path,
				null, issue.Message
			);
			if ( issue.Kind == TermInfoDatabaseCatalogIssueKind.InvalidPlacement ) {
				invalidPaths.Add( issue.Path );
			}
		}

		List<TerminalCatalogEntry> entries = [];
		Dictionary<string, int> counts = new( StringComparer.Ordinal );
		foreach ( TermInfoDatabaseCatalogEntry entry in physical.Entries ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( invalidPaths.Contains( entry.Path ) ) {
				continue;
			}
			string publicationName = Path.GetFileName( entry.Path );
			TerminalCatalogEntryKind kind = (string.Equals( publicationName, entry.Terminal.Name, StringComparison.Ordinal ))
				? TerminalCatalogEntryKind.Canonical
				: TerminalCatalogEntryKind.Alias
			;
			entries.Add( new( publicationName, kind, entry.Terminal, source.Path, entry.Path ) );
			counts.TryGetValue( publicationName, out int count );
			counts[ publicationName ] = count + 1;
		}

		List<string> duplicates = [];
		foreach ( KeyValuePair<string, int> publication in counts ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( publication.Value > 1 ) {
				AddIssue( TerminalCatalogIssueKind.DuplicatePublication, null, publication.Key,
					"The publication name occurs in more than one physical entry."
				);
				duplicates.Add( publication.Key );
			}
		}

		TerminalCatalogStatus status;
		switch ( physical.Kind ) {
			case TermInfoDatabaseCatalogKind.ConventionalDirectory:
				status = issues.Count == 0 ? TerminalCatalogStatus.Complete : TerminalCatalogStatus.Partial;
				break;
			case TermInfoDatabaseCatalogKind.Missing:
				status = TerminalCatalogStatus.Missing;
				AddIssue( TerminalCatalogIssueKind.MissingSource, null, null, "The requested directory source does not exist." );
				break;
			case TermInfoDatabaseCatalogKind.UnsupportedStore:
				status = TerminalCatalogStatus.UnsupportedSource;
				AddIssue( TerminalCatalogIssueKind.UnsupportedSource, null, null, "The requested source is not a conventional directory." );
				break;
			case TermInfoDatabaseCatalogKind.Unavailable:
				status = TerminalCatalogStatus.Unavailable;
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( physical ), "The physical catalog kind is not supported." );
		}
		// Acquisition has already charged all physical parses, including filtered ones.
		// Result construction performs bounded validation and deterministic sorting.
		cancellationToken.ThrowIfCancellationRequested();
		TerminalCatalog result = new( source, status, entries, issues, duplicates );
		cancellationToken.ThrowIfCancellationRequested();
		return result;

		void AddIssue( TerminalCatalogIssueKind kind, string? entryPath, string? publicationName, string message ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( issues.Count >= options.MaximumIssueCount ) {
				throw new TerminalCatalogLimitException( source, nameof( options.MaximumIssueCount ), options.MaximumIssueCount );
			}
			issues.Add( new( kind, source.Path, entryPath, publicationName, message ) );
		}
	}

	private static void ValidateInputs( TerminalCatalogSource source, TerminalCatalogReadOptions options ) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( options );
		if ( source.Kind != TerminalCatalogSourceKind.ConventionalDirectory ) {
			throw new ArgumentException( "The directory adapter requires a conventional-directory source.", nameof( source ) );
		}
	}

	private static TerminalCatalogIssueKind MapIssueKind( TermInfoDatabaseCatalogIssueKind kind ) => kind switch {
		TermInfoDatabaseCatalogIssueKind.MalformedEntry => TerminalCatalogIssueKind.MalformedEntry,
		TermInfoDatabaseCatalogIssueKind.InvalidPlacement => TerminalCatalogIssueKind.InvalidPlacement,
		TermInfoDatabaseCatalogIssueKind.PermissionFailure => TerminalCatalogIssueKind.PermissionFailure,
		TermInfoDatabaseCatalogIssueKind.IoFailure => TerminalCatalogIssueKind.IoFailure,
		TermInfoDatabaseCatalogIssueKind.LinkSkipped => TerminalCatalogIssueKind.LinkSkipped,
		_ => throw new ArgumentOutOfRangeException( nameof( kind ) ),
	};
}
