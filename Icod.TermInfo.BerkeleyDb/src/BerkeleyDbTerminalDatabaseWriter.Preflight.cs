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

using System.Buffers.Binary;
using System.Text;

namespace Icod.TermInfo.BerkeleyDb;

public static partial class BerkeleyDbTerminalDatabaseWriter {
	private static readonly UTF8Encoding StrictUtf8 = new(
		encoderShouldEmitUTF8Identifier: false,
		throwOnInvalidBytes: true
	);
	private static readonly HashSet<string> ReservedDeviceStems = new(
		new[] {
			"CON",
			"PRN",
			"AUX",
			"NUL",
			"CLOCK$",
			"COM1",
			"COM2",
			"COM3",
			"COM4",
			"COM5",
			"COM6",
			"COM7",
			"COM8",
			"COM9",
			"LPT1",
			"LPT2",
			"LPT3",
			"LPT4",
			"LPT5",
			"LPT6",
			"LPT7",
			"LPT8",
			"LPT9",
		},
		StringComparer.OrdinalIgnoreCase
	);

	private static BerkeleyDbTerminalDatabaseEntry[] SnapshotEntries(
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		CancellationToken cancellationToken
	) {
		List<BerkeleyDbTerminalDatabaseEntry> snapshot = [];
		foreach ( BerkeleyDbTerminalDatabaseEntry? entry in entries ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( entry is null ) {
				throw new ArgumentException(
					"The entry sequence cannot contain null.",
					nameof( entries )
				);
			}
			snapshot.Add( entry );
		}
		if ( snapshot.Count == 0 ) {
			throw new ArgumentException(
				"The entry sequence cannot be empty.",
				nameof( entries )
			);
		}
		return snapshot.ToArray();
	}

	internal static PreparedPublication[] PreparePublications(
		IReadOnlyList<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions options,
		CancellationToken cancellationToken
	) {
		Dictionary<string, string> owners = new( StringComparer.Ordinal );
		List<PreparedPublication> publications = new( entries.Count );
		int recordCount = 0;

		foreach ( BerkeleyDbTerminalDatabaseEntry entry in entries ) {
			cancellationToken.ThrowIfCancellationRequested();
			PreparedIdentity canonical = PrepareIdentity( entry.CanonicalName );
			AddOwner( owners, canonical.Name, entry.CanonicalName );

			PreparedIdentity[] aliases = new PreparedIdentity[entry.Aliases.Count];
			for ( int index = 0; index < aliases.Length; index++ ) {
				PreparedIdentity alias = PrepareIdentity( entry.Aliases[index] );
				AddOwner( owners, alias.Name, entry.CanonicalName );
				aliases[index] = alias;
			}

			try {
				recordCount = checked( recordCount + 2 + aliases.Length );
			} catch ( OverflowException exception ) {
				throw new InvalidOperationException(
					"The terminal publications exceed the configured Hash record limit.",
					exception
				);
			}
			if ( recordCount > options.MaximumRecordCount ) {
				throw new InvalidOperationException(
					"The terminal publications exceed the configured Hash record limit."
				);
			}

			byte[] data = (byte[])entry.Data.Clone();
			TerminalDescription parsed = CompiledTermInfoParser.Parse(
				data,
				options.ParserOptions
			);
			RequireIdentityAgreement( entry, parsed );
			publications.Add(
				new PreparedPublication(
					canonical,
					Array.AsReadOnly( aliases ),
					ExtractStorageKey( data ),
					data
				)
			);
		}

		return publications.ToArray();
	}

	private static byte[] ExtractStorageKey( byte[] data ) {
		int namesLength = BinaryPrimitives.ReadUInt16LittleEndian(
			data.AsSpan( 2, sizeof( ushort ) )
		);
		return data.AsSpan( 12, namesLength - 1 ).ToArray();
	}

	private static PreparedIdentity PrepareIdentity( string name ) {
		if (
			string.IsNullOrWhiteSpace( name )
			|| name == "."
			|| name == ".."
			|| name[^1] is '.' or ' '
		) {
			throw InvalidIdentity();
		}

		foreach ( char character in name ) {
			if (
				char.IsControl( character )
				|| character is '/' or '\\' or '<' or '>' or ':' or '"'
					or '|' or '?' or '*'
			) {
				throw InvalidIdentity();
			}
		}

		int dotIndex = name.IndexOf( '.' );
		string stem = ( dotIndex < 0 ) ? name : name[..dotIndex];
		if ( ReservedDeviceStems.Contains( stem ) ) {
			throw InvalidIdentity();
		}

		try {
			return new PreparedIdentity( name, StrictUtf8.GetBytes( name ) );
		} catch ( EncoderFallbackException exception ) {
			throw new ArgumentException(
				"Terminal names must be valid Unicode and portable filenames.",
				"entries",
				exception
			);
		}
	}

	private static ArgumentException InvalidIdentity() => new(
		"Terminal names must be valid Unicode and portable filenames.",
		"entries"
	);

	private static void AddOwner(
		Dictionary<string, string> owners,
		string name,
		string canonicalName
	) {
		if ( !owners.TryAdd( name, canonicalName ) ) {
			throw new InvalidOperationException(
				$"The terminal name '{name}' is owned by more than one publication."
			);
		}
	}

	private static void RequireIdentityAgreement(
		BerkeleyDbTerminalDatabaseEntry entry,
		TerminalDescription parsed
	) {
		if (
			!string.Equals(
				entry.CanonicalName,
				parsed.Name,
				StringComparison.Ordinal
			)
		) {
			throw new InvalidOperationException(
				"The declared canonical name does not match the compiled entry."
			);
		}

		if ( entry.Aliases.Count != parsed.Aliases.Count ) {
			throw AliasMismatch();
		}
		for ( int index = 0; index < entry.Aliases.Count; index++ ) {
			if (
				!string.Equals(
					entry.Aliases[index],
					parsed.Aliases[index],
					StringComparison.Ordinal
				)
			) {
				throw AliasMismatch();
			}
		}
	}

	private static InvalidOperationException AliasMismatch() => new(
		"The declared alias order does not match the compiled entry."
	);

	internal sealed record PreparedIdentity(
		string Name,
		byte[] Utf8
	);

	internal sealed record PreparedPublication(
		PreparedIdentity Canonical,
		IReadOnlyList<PreparedIdentity> Aliases,
		byte[] StorageKey,
		byte[] Data
	);
}
