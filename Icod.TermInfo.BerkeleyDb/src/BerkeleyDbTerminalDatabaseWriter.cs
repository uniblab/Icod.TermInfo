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
	/// <remarks>
	/// Stages, flushes, closes, and verifies a unique sibling file before a single
	/// same-directory move. A persistent sibling lock serializes cooperative Icod
	/// writers; contention waits until acquisition or cancellation, without a timeout.
	/// Replacement uses fresh file metadata. Cancellation is observed until the
	/// final pre-move check, never after commit begins. Atomic visibility depends on
	/// the host filesystem; directory power-loss durability and coordination with
	/// native writers are not provided. The immediate parent, destination, and lock
	/// must not be symbolic links or reparse points. The lock sidecar remains on disk.
	/// </remarks>
	/// <exception cref="ArgumentException">
	/// <paramref name="databasePath"/> is empty or whitespace, or
	/// <paramref name="entries"/> is empty or contains a null element.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="databasePath"/> or <paramref name="entries"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is cancelled before commit begins.
	/// </exception>
	/// <exception cref="IOException">
	/// The destination cannot be created or written, including when it already
	/// exists and <see cref="BerkeleyDbTerminalDatabaseWriterOptions.OverwriteExisting"/>
	/// is <see langword="false"/>.
	/// </exception>
	/// <exception cref="UnauthorizedAccessException">
	/// Access to <paramref name="databasePath"/> is denied.
	/// </exception>
	/// <exception cref="NotSupportedException">
	/// The filesystem does not enforce the required exclusive publication lock.
	/// </exception>
	/// <exception cref="InvalidDataException">
	/// The reopened staged image, records, or catalog differ from the publication plan.
	/// </exception>
	public static void Write(
		string databasePath,
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions? options = null,
		CancellationToken cancellationToken = default
	) => WriteCore( databasePath, entries, options, cancellationToken, new SystemBerkeleyDbDatabasePublicationFileSystem() );

	internal static void WriteCore(
		string databasePath, IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions? options, CancellationToken cancellationToken,
		BerkeleyDbDatabasePublicationFileSystem fileSystem
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( databasePath );
		ArgumentNullException.ThrowIfNull( entries );
		cancellationToken.ThrowIfCancellationRequested();

		string fullPath = Path.GetFullPath( databasePath );
		BerkeleyDbTerminalDatabaseWriterOptions effectiveOptions =
			SnapshotOptions( options );
		BerkeleyDbTerminalDatabaseEntry[] entrySnapshot =
			SnapshotEntries( entries, cancellationToken );
		PreparedPublication[] publications = PreparePublications(
			entrySnapshot,
			effectiveOptions,
			cancellationToken
		);
		IReadOnlyList<BerkeleyDbHashRecord> records =
			BerkeleyDbNcursesRecordPlanner.CreateRecords(
				publications,
				cancellationToken
			);
		byte[] image = BerkeleyDbHashV9ImageBuilder.Build(
			records,
			effectiveOptions.MaximumDatabaseSize,
			cancellationToken
		);

		BerkeleyDbDatabasePublisher.Publish(
			fullPath, image, records, publications, effectiveOptions, fileSystem, cancellationToken
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
