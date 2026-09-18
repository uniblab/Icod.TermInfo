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

/// <summary>Writes ncurses-compatible Berkeley DB Hash-v9 terminal databases.</summary>
public static partial class BerkeleyDbTerminalDatabaseWriter {
	/// <summary>Writes terminal entries to one Hash-v9 database.</summary>
	/// <param name="databasePath">The destination database path.</param>
	/// <param name="entries">The terminal entries to publish.</param>
	/// <param name="options">Writer limits and destination policy, or defaults.</param>
	/// <param name="cancellationToken">A token that may cancel the operation.</param>
	/// <exception cref="ArgumentException">
	/// <paramref name="databasePath"/> is empty or whitespace, or
	/// <paramref name="entries"/> is empty or contains a null element.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="databasePath"/> or <paramref name="entries"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is cancelled.
	/// </exception>
	/// <exception cref="NotSupportedException">
	/// HW01 contract validation completed; image construction begins in HW02.
	/// </exception>
	public static void Write(
		string databasePath,
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions? options = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( databasePath );
		ArgumentNullException.ThrowIfNull( entries );
		cancellationToken.ThrowIfCancellationRequested();

		_ = Path.GetFullPath( databasePath );
		BerkeleyDbTerminalDatabaseWriterOptions effectiveOptions =
			SnapshotOptions( options );
		BerkeleyDbTerminalDatabaseEntry[] entrySnapshot =
			SnapshotEntries( entries, cancellationToken );
		_ = PreparePublications(
			entrySnapshot,
			effectiveOptions,
			cancellationToken
		);

		cancellationToken.ThrowIfCancellationRequested();
		throw new NotSupportedException(
			"HW01 freezes writer contracts; Hash-v9 image construction begins in HW02."
		);
	}

	private static BerkeleyDbTerminalDatabaseWriterOptions SnapshotOptions(
		BerkeleyDbTerminalDatabaseWriterOptions? options
	) {
		BerkeleyDbTerminalDatabaseWriterOptions effective =
			options ?? new BerkeleyDbTerminalDatabaseWriterOptions();
		return new BerkeleyDbTerminalDatabaseWriterOptions(
			effective.ParserOptions,
			effective.MaximumDatabaseSize,
			effective.MaximumRecordCount,
			effective.OverwriteExisting
		);
	}
}
