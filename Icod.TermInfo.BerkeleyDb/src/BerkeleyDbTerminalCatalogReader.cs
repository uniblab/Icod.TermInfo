/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
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

/// <summary>Reads immutable logical terminal publications from one explicit Berkeley DB Hash-v9 file.</summary>
/// <remarks>Each read acquires a fresh database image. Catalog results are not cached.</remarks>
public sealed class BerkeleyDbTerminalCatalogReader {
	private readonly BerkeleyDbTerminalCatalogReaderOptions _options;

	/// <summary>Initializes a hashed terminal catalog reader.</summary>
	public BerkeleyDbTerminalCatalogReader(
		string databasePath,
		BerkeleyDbTerminalCatalogReaderOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( databasePath );

		if ( string.IsNullOrWhiteSpace( databasePath ) ) {
			throw new ArgumentException(
				"The Berkeley DB path cannot be empty or whitespace.",
				nameof( databasePath )
			);
		}

		DatabasePath = Path.GetFullPath( databasePath );

		BerkeleyDbTerminalCatalogReaderOptions effectiveOptions =
			options ?? new BerkeleyDbTerminalCatalogReaderOptions();
		_options =
			new BerkeleyDbTerminalCatalogReaderOptions(
				effectiveOptions.ParserOptions,
				effectiveOptions.MaximumDatabaseSize,
				effectiveOptions.MaximumRecordCount,
				effectiveOptions.MaximumIndexHops
			);
	}

	/// <summary>Gets the canonical absolute database path.</summary>
	public string DatabasePath { get; }

	/// <summary>Reads a fresh immutable catalog snapshot.</summary>
	public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read() {
		return Read( CancellationToken.None );
	}

	/// <summary>Reads a fresh immutable catalog snapshot with cancellation.</summary>
	public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read(
		CancellationToken cancellationToken
	) {
		return ReadCore( cancellationToken );
	}

	/// <summary>Reads a fresh catalog with inclusive per-call acquisition and logical budgets.</summary>
	/// <param name="limits">Publication, decoded-byte and parsed-byte limits, or null for defaults.</param>
	/// <param name="cancellationToken">Cancellation observed between acquisition and normalization work units.</param>
	/// <returns>The complete immutable catalog; no partial result is returned.</returns>
	/// <exception cref="BerkeleyDbCatalogLimitException">An inclusive resource limit would be exceeded.</exception>
	/// <exception cref="OperationCanceledException">Cancellation was requested.</exception>
	/// <remarks>Aliases share a parsed storage record. Orphan storage records are also validated and charged.
	/// Each call has independent budgets. Blocking operating-system calls cannot be interrupted.</remarks>
	public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> ReadBounded(
		BerkeleyDbTerminalCatalogReadLimits? limits = null,
		CancellationToken cancellationToken = default
	) {
		cancellationToken.ThrowIfCancellationRequested();
		return ReadCore( cancellationToken, new BerkeleyDbCatalogReadBudget(
			DatabasePath, _options, limits ?? new BerkeleyDbTerminalCatalogReadLimits(), cancellationToken
		)
		);
	}

	private IReadOnlyList<BerkeleyDbTerminalCatalogEntry> ReadCore(
		CancellationToken cancellationToken,
		BerkeleyDbCatalogReadBudget? budget = null
	) {
		cancellationToken.ThrowIfCancellationRequested();

		IReadOnlyList<BerkeleyDbHashRecord> records;
		try {
			byte[] database = BerkeleyDbHashReader.ReadDatabase(
				DatabasePath,
				_options.MaximumDatabaseSize,
				budget
			);
			records = BerkeleyDbHashReader.ReadRecords(
				database,
				checked(
					_options.ParserOptions.MaximumEntrySize + 1
				),
				_options.MaximumRecordCount,
				cancellationToken,
				budget
			);
		} catch ( InvalidDataException exception ) {
			throw new BerkeleyDbDatabaseFormatException(
				"The Berkeley DB file is malformed or unsupported.",
				exception
			);
		}

		return NcursesCatalogReader.Read(
			records,
			_options.ParserOptions,
			_options.MaximumIndexHops,
			cancellationToken,
			budget
		);
	}
}
