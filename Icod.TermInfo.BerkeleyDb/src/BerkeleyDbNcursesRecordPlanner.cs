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

internal static class BerkeleyDbNcursesRecordPlanner {
	private const byte DataMarker = 0;
	private const byte PublicationMarker = 2;

	internal static IReadOnlyList<BerkeleyDbHashRecord> CreateRecords(
		IReadOnlyList<
			BerkeleyDbTerminalDatabaseWriter.PreparedPublication
		> publications,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( publications );
		cancellationToken.ThrowIfCancellationRequested();

		var records = new List<BerkeleyDbHashRecord>();
		var keys = new HashSet<byte[]>( ByteArrayComparer.Instance );
		foreach (
			BerkeleyDbTerminalDatabaseWriter.PreparedPublication publication
				in publications
		) {
			cancellationToken.ThrowIfCancellationRequested();
			AddRecord(
				records,
				keys,
				publication.Canonical.Utf8,
				PrependMarker(
					publication.StorageKey,
					PublicationMarker
				)
			);

			foreach (
				BerkeleyDbTerminalDatabaseWriter.PreparedIdentity alias
					in publication.Aliases
			) {
				cancellationToken.ThrowIfCancellationRequested();
				AddRecord(
					records,
					keys,
					alias.Utf8,
					PrependMarker(
						publication.StorageKey,
						PublicationMarker
					)
				);
			}

			AddRecord(
				records,
				keys,
				publication.StorageKey,
				PrependMarker( publication.Data, DataMarker )
			);
		}

		BerkeleyDbHashRecord[] ordered = records.ToArray();
		Array.Sort(
			ordered,
			static ( left, right ) =>
				BerkeleyDbHashV9WriterKeyComparer.Instance.Compare(
					left.Key.Span,
					right.Key.Span
				)
		);
		return Array.AsReadOnly( ordered );
	}

	private static void AddRecord(
		List<BerkeleyDbHashRecord> records,
		HashSet<byte[]> keys,
		byte[] key,
		byte[] value
	) {
		byte[] keySnapshot = key.ToArray();
		if ( !keys.Add( keySnapshot ) ) {
			throw new InvalidOperationException(
				"The ncurses publication set contains a duplicate exact byte key."
			);
		}

		records.Add( new BerkeleyDbHashRecord( keySnapshot, value ) );
	}

	private static byte[] PrependMarker(
		byte[] payload,
		byte marker
	) {
		byte[] result = new byte[checked( payload.Length + 1 )];
		result[0] = marker;
		payload.CopyTo( result.AsSpan( 1 ) );
		return result;
	}
}
