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

using System.Collections.ObjectModel;

namespace Icod.TermInfo.Inspection;

public static partial class TermInfoDatabaseInspector {
	/// <summary>Inspects one explicit conventional directory with bounded discovery and parsing.</summary>
	/// <param name="root">Directory path, normalized to an absolute path once.</param>
	/// <param name="options">Independent inclusive resource budgets; null selects defaults.</param>
	/// <param name="cancellationToken">Cancellation checked during traversal and entry reads.</param>
	/// <returns>An immutable physical catalog preserving placement issues and duplicate canonical identities.</returns>
	/// <exception cref="TermInfoDatabaseCatalogLimitException">A configured budget was exhausted; no partial result is returned.</exception>
	/// <remarks>Ignored children consume discovery budget. Links below the explicit root are skipped.
	/// Entry capacity is checked conservatively before parsing; malformed candidates consume byte budget.
	/// The observation is not an atomic filesystem snapshot.</remarks>
	public static TermInfoDatabaseCatalog InspectDirectoryBounded(
		string root, TermInfoDatabaseCatalogReadOptions? options = null,
		CancellationToken cancellationToken = default
	) => InspectDirectoryBoundedCore(
		root, options ?? new(), cancellationToken, Directory.EnumerateFileSystemEntries
	);

	internal static TermInfoDatabaseCatalog InspectDirectoryBoundedCore(
		string root, TermInfoDatabaseCatalogReadOptions options,
		CancellationToken cancellationToken, Func<string, IEnumerable<string>> enumerate
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( enumerate );
		cancellationToken.ThrowIfCancellationRequested();
		string normalizedRoot = Path.GetFullPath( root );
		CatalogReadBudget budget = new( normalizedRoot, options );
		FileAttributes attributes;
		try { attributes = File.GetAttributes( normalizedRoot ); }
		catch ( FileNotFoundException ) { return CreateEmptyCatalog( normalizedRoot, TermInfoDatabaseCatalogKind.Missing ); }
		catch ( DirectoryNotFoundException ) { return CreateEmptyCatalog( normalizedRoot, TermInfoDatabaseCatalogKind.Missing ); }
		catch ( Exception exception ) when ( IsCatalogIoException( exception ) ) {
			budget.ReserveIssue();
			return CreateUnavailableCatalog( normalizedRoot, exception, "terminfo database root" );
		}
		if ( (attributes & FileAttributes.Directory) == 0 ) {
			return CreateEmptyCatalog( normalizedRoot, TermInfoDatabaseCatalogKind.UnsupportedStore );
		}

		List<TermInfoDatabaseCatalogEntry> entries = [];
		BudgetedCatalogIssues issues = new( budget );
		List<string> directories = EnumerateBoundedPaths(
			normalizedRoot, true, budget, cancellationToken, enumerate, out Exception? rootFailure, out bool observed
		);
		if ( rootFailure is not null ) {
			if ( !observed ) {
				if ( rootFailure is DirectoryNotFoundException or FileNotFoundException ) {
					return CreateEmptyCatalog( normalizedRoot, TermInfoDatabaseCatalogKind.Missing );
				}
				budget.ReserveIssue();
				return CreateUnavailableCatalog( normalizedRoot, rootFailure, "terminfo database root" );
			}
			issues.Add( CreateFileSystemIssue( normalizedRoot, rootFailure, "terminfo database root" ) );
		}
		foreach ( string directory in directories ) {
			cancellationToken.ThrowIfCancellationRequested();
			try { attributes = File.GetAttributes( directory ); }
			catch ( Exception exception ) when ( IsCatalogIoException( exception ) ) {
				issues.Add( CreateFileSystemIssue( directory, exception, "terminfo database subdirectory" ) );
				continue;
			}
			if ( (attributes & FileAttributes.Directory) == 0 ) {
				continue;
			}
			if ( IsCatalogReparsePoint( attributes ) ) {
				issues.Add( new( TermInfoDatabaseCatalogIssueKind.LinkSkipped, directory,
					"The terminfo database subdirectory is a link or reparse point and was not traversed."
				)
				);
				continue;
			}
			List<string> files = EnumerateBoundedPaths(
				directory, false, budget, cancellationToken, enumerate, out Exception? failure, out _
			);
			if ( failure is not null ) {
				issues.Add( CreateFileSystemIssue( directory, failure, "terminfo database subdirectory" ) );
			}
			foreach ( string path in files ) {
				cancellationToken.ThrowIfCancellationRequested();
				try { attributes = File.GetAttributes( path ); }
				catch ( Exception exception ) when ( IsCatalogIoException( exception ) ) {
					issues.Add( CreateFileSystemIssue( path, exception, "compiled terminfo candidate" ) );
					continue;
				}
				// Nested regular directories are counted, but never traversed or parsed.
				if ( (attributes & FileAttributes.Directory) != 0 && !IsCatalogReparsePoint( attributes ) ) {
					continue;
				}
				InspectCandidate( GetRequiredFileName( directory ), path, options.ParserOptions,
					entries, issues, cancellationToken, budget
				);
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		TermInfoDatabaseCatalog result = CreateConventionalCatalog( normalizedRoot, entries, issues );
		cancellationToken.ThrowIfCancellationRequested();
		return result;
	}

	private static List<string> EnumerateBoundedPaths(
		string path, bool root, CatalogReadBudget budget, CancellationToken cancellationToken,
		Func<string, IEnumerable<string>> enumerate, out Exception? failure, out bool observed
	) {
		List<string> paths = [];
		failure = null;
		observed = false;
		try {
			using IEnumerator<string> iterator = enumerate( path ).GetEnumerator();
			while ( true ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !iterator.MoveNext() ) {
					break;
				}
				cancellationToken.ThrowIfCancellationRequested();
				budget.ReserveCandidate();
				observed = true;
				string child = iterator.Current;
				if ( !root || IsConventionalCatalogDirectoryName( GetRequiredFileName( child ) ) ) {
					paths.Add( child );
				}
			}
		} catch ( Exception exception ) when ( IsCatalogIoException( exception ) ) {
			failure = exception;
		}
		cancellationToken.ThrowIfCancellationRequested();
		paths.Sort( StringComparer.Ordinal );
		cancellationToken.ThrowIfCancellationRequested();
		return paths;
	}

	private sealed class BudgetedCatalogIssues : Collection<TermInfoDatabaseCatalogIssue> {
		private readonly CatalogReadBudget _budget;
		internal BudgetedCatalogIssues( CatalogReadBudget budget ) => _budget = budget;
		protected override void InsertItem( int index, TermInfoDatabaseCatalogIssue item ) {
			_budget.ReserveIssue();
			base.InsertItem( index, item );
		}
	}
}
