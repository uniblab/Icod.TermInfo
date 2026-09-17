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

internal static class NcursesCatalogReader {
	internal static IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		CompiledTermInfoParserOptions parserOptions,
		int maximumIndexHops,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( records );
		ArgumentNullException.ThrowIfNull( parserOptions );
		ArgumentOutOfRangeException.ThrowIfNegative( maximumIndexHops );
		cancellationToken.ThrowIfCancellationRequested();

		var recordsByKey =
			new Dictionary<byte[], BerkeleyDbHashRecord>(
				ByteArrayComparer.Instance
			);
		foreach ( BerkeleyDbHashRecord record in records ) {
			byte[] key = record.Key.ToArray();
			if ( !recordsByKey.TryAdd( key, record ) ) {
				throw CreateFormatException(
					"The Berkeley DB contains a duplicate exact byte key."
				);
			}
		}

		var publicationNames =
			new Dictionary<BerkeleyDbHashRecord, string>();
		var publicationKeysByName =
			new Dictionary<string, byte[]>( StringComparer.Ordinal );
		foreach ( BerkeleyDbHashRecord record in records ) {
			ReadOnlySpan<byte> value = record.Value.Span;
			if (
				value.Length == 0
				|| value[0] != 2
			) {
				continue;
			}

			string name = DecodePublicationName( record.Key.Span );
			byte[] key = record.Key.ToArray();
			if (
				publicationKeysByName.TryGetValue(
					name,
					out byte[]? existingKey
				)
				&& !existingKey.AsSpan().SequenceEqual( key )
			) {
				throw CreateFormatException(
					$"The ncurses catalog contains more than one exact byte key for logical publication '{name}'."
				);
			}

			publicationKeysByName.TryAdd( name, key );
			publicationNames.Add( record, name );
		}

		var terminalsByStorageKey =
			new Dictionary<byte[], TerminalDescription>(
				ByteArrayComparer.Instance
			);
		var entries = new List<BerkeleyDbTerminalCatalogEntry>();

		foreach ( BerkeleyDbHashRecord record in records ) {
			cancellationToken.ThrowIfCancellationRequested();

			ReadOnlySpan<byte> value = record.Value.Span;
			if ( value.Length == 0 ) {
				throw CreateFormatException(
					"The ncurses hashed-term record is empty."
				);
			}

			switch ( value[0] ) {
				case 0:
					ParseStorageRecord(
						record,
						terminalsByStorageKey,
						parserOptions
					);
					break;

				case 2:
					string name = publicationNames[record];
					TerminalDescription terminal = ResolvePublication(
						record,
						recordsByKey,
						terminalsByStorageKey,
						parserOptions,
						maximumIndexHops,
						cancellationToken
					);
					BerkeleyDbTerminalCatalogEntryKind kind =
						ClassifyPublication( name, terminal );
					entries.Add(
						new BerkeleyDbTerminalCatalogEntry(
							name,
							kind,
							terminal
						)
					);
					break;

				default:
					throw CreateFormatException(
						$"Ncurses hashed-term marker {value[0]} is not supported."
					);
			}
		}

		entries.Sort(
			static ( left, right ) => {
				int comparison = StringComparer.Ordinal.Compare(
					left.Name,
					right.Name
				);
				if ( comparison != 0 ) {
					return comparison;
				}

				comparison = left.Kind.CompareTo( right.Kind );
				if ( comparison != 0 ) {
					return comparison;
				}

				return StringComparer.Ordinal.Compare(
					left.Terminal.Name,
					right.Terminal.Name
				);
			}
		);
		return Array.AsReadOnly( entries.ToArray() );
	}

	private static string DecodePublicationName(
		ReadOnlySpan<byte> bytes
	) {
		try {
			string name = TerminalNameEncoding.DecodePublicationName(
				bytes
			);
			TerminalNameValidator.Validate( name );
			return name;
		} catch ( ArgumentException exception ) {
			throw CreateFormatException(
				"The ncurses publication key is not a safe exact UTF-8 or Latin-1 terminal name.",
				exception
			);
		}
	}

	private static TerminalDescription ResolvePublication(
		BerkeleyDbHashRecord publication,
		Dictionary<byte[], BerkeleyDbHashRecord> recordsByKey,
		Dictionary<byte[], TerminalDescription> terminalsByStorageKey,
		CompiledTermInfoParserOptions parserOptions,
		int maximumIndexHops,
		CancellationToken cancellationToken
	) {
		BerkeleyDbHashRecord current = publication;
		var visited =
			new HashSet<byte[]>( ByteArrayComparer.Instance );
		int followedLinks = 0;

		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();

			if ( !visited.Add( current.Key.ToArray() ) ) {
				throw CreateFormatException(
					"The ncurses index chain contains a cycle."
				);
			}

			ReadOnlySpan<byte> value = current.Value.Span;
			if ( value.Length == 0 ) {
				throw CreateFormatException(
					"The ncurses hashed-term record is empty."
				);
			}
			if ( value[0] == 0 ) {
				return ParseStorageRecord(
					current,
					terminalsByStorageKey,
					parserOptions
				);
			}
			if ( value[0] != 2 ) {
				throw CreateFormatException(
					$"Ncurses hashed-term marker {value[0]} is not supported."
				);
			}
			if ( value.Length == 1 ) {
				throw CreateFormatException(
					"The ncurses index record has an empty target."
				);
			}
			if ( followedLinks >= maximumIndexHops ) {
				throw CreateFormatException(
					"The ncurses index chain exceeds the configured hop limit."
				);
			}

			followedLinks++;
			byte[] target = value[1..].ToArray();
			if (
				!recordsByKey.TryGetValue(
					target,
					out BerkeleyDbHashRecord? next
				)
			) {
				throw CreateFormatException(
					"The ncurses index record references a missing key."
				);
			}
			current = next!;
		}
	}

	private static TerminalDescription ParseStorageRecord(
		BerkeleyDbHashRecord record,
		Dictionary<byte[], TerminalDescription> terminalsByStorageKey,
		CompiledTermInfoParserOptions parserOptions
	) {
		byte[] key = record.Key.ToArray();
		if (
			terminalsByStorageKey.TryGetValue(
				key,
				out TerminalDescription? terminal
			)
		) {
			return terminal!;
		}

		ReadOnlySpan<byte> value = record.Value.Span;
		if ( value.Length == 0 ) {
			throw CreateFormatException(
				"The ncurses hashed-term record is empty."
			);
		}
		if ( value[0] != 0 ) {
			throw CreateFormatException(
				"The ncurses storage record does not contain compiled terminfo bytes."
			);
		}

		terminal = CompiledTermInfoParser.Parse(
			value[1..],
			parserOptions
		);
		terminalsByStorageKey.Add( key, terminal );
		return terminal;
	}

	private static BerkeleyDbTerminalCatalogEntryKind ClassifyPublication(
		string name,
		TerminalDescription terminal
	) {
		if (
			string.Equals(
				name,
				terminal.Name,
				StringComparison.Ordinal
			)
		) {
			return BerkeleyDbTerminalCatalogEntryKind.Canonical;
		}

		foreach ( string alias in terminal.Aliases ) {
			if (
				string.Equals(
					name,
					alias,
					StringComparison.Ordinal
				)
			) {
				return BerkeleyDbTerminalCatalogEntryKind.Alias;
			}
		}

		throw new InvalidDataException(
			$"Compiled terminfo entry in the Berkeley DB identifies terminal '{terminal.Name}' and does not declare published name '{name}'."
		);
	}

	private static BerkeleyDbDatabaseFormatException CreateFormatException(
		string message,
		Exception? innerException = null
	) {
		return new BerkeleyDbDatabaseFormatException(
			message,
			innerException
		);
	}
}
