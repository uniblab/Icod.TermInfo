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

internal static class BerkeleyDbDatabasePublicationVerifier {
	internal static void Verify(
		byte[] reopened, byte[] expected, IReadOnlyList<BerkeleyDbHashRecord> plannedRecords,
		IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
		BerkeleyDbTerminalDatabaseWriterOptions options, CancellationToken cancellationToken
	) {
		RequireEqual( reopened, expected, cancellationToken );
		var records = BerkeleyDbHashReader.ReadRecords(
			reopened, checked( options.ParserOptions.MaximumEntrySize + 1 ),
			options.MaximumRecordCount, cancellationToken
		);
		if ( records.Count != plannedRecords.Count ) {
			throw new InvalidDataException( "The staged database physical record count differs from the plan." );
		}
		var expectedRecords = new Dictionary<byte[], BerkeleyDbHashRecord>( ByteArrayComparer.Instance );
		foreach ( var record in plannedRecords ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !expectedRecords.TryAdd( record.Key.ToArray(), record ) ) {
				throw new InvalidDataException( "The publication plan contains duplicate keys." );
			}
		}
		foreach ( var record in records ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !expectedRecords.Remove( record.Key.ToArray(), out var planned ) ) {
				throw new InvalidDataException( "The staged database contains an unexpected physical key." );
			}
			RequireEqual( record.Value.Span, planned.Value.Span, cancellationToken );
		}
		var catalog = NcursesCatalogReader.Read(
			records, options.ParserOptions,
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumIndexHops, cancellationToken
		);
		var expectedNames = new Dictionary<string, (BerkeleyDbTerminalDatabaseWriter.PreparedPublication Publication, BerkeleyDbTerminalCatalogEntryKind Kind)>( StringComparer.Ordinal );
		foreach ( var publication in publications ) {
			cancellationToken.ThrowIfCancellationRequested();
			expectedNames.Add( publication.Canonical.Name, (publication, BerkeleyDbTerminalCatalogEntryKind.Canonical) );
			foreach ( var alias in publication.Aliases ) {
				expectedNames.Add( alias.Name, (publication, BerkeleyDbTerminalCatalogEntryKind.Alias) );
			}
		}
		foreach ( var entry in catalog ) {
			cancellationToken.ThrowIfCancellationRequested();
			if (
				!expectedNames.Remove( entry.Name, out var planned )
				|| entry.Kind != planned.Kind
				|| !StringComparer.Ordinal.Equals( entry.Terminal.Name, planned.Publication.Canonical.Name )
				|| !entry.Terminal.Aliases.SequenceEqual( planned.Publication.Aliases.Select( alias => alias.Name ), StringComparer.Ordinal )
			) {
				throw new InvalidDataException( "The staged database logical catalog differs from the publication plan." );
			}
		}
		if ( expectedNames.Count != 0 ) {
			throw new InvalidDataException( "The staged database is missing planned logical names." );
		}
	}

	private static void RequireEqual( ReadOnlySpan<byte> actual, ReadOnlySpan<byte> expected, CancellationToken token ) {
		token.ThrowIfCancellationRequested();
		if ( actual.Length != expected.Length ) {
			throw new InvalidDataException( "The staged database bytes differ from the publication plan." );
		}
		for ( int offset = 0; offset < actual.Length; ) {
			token.ThrowIfCancellationRequested();
			int count = Math.Min( 81_920, actual.Length - offset );
			if ( !actual.Slice( offset, count ).SequenceEqual( expected.Slice( offset, count ) ) ) {
				throw new InvalidDataException( "The staged database bytes differ from the publication plan." );
			}
			offset += count;
		}
	}
}
