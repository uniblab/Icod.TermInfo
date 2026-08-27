using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Serializes immutable terminal descriptions into conventional compiled
/// terminfo entries.
/// </summary>
/// <remarks>
/// C03 implements deterministic legacy <c>0432</c> entries containing terminal
/// identity metadata, complete standard capability tables, and the supported
/// ncurses extended-capability section. Wide-numeric format policy is added by
/// C04. Writing is pure with respect to filesystem, environment, and native
/// ncurses state.
/// </remarks>
public static partial class CompiledTermInfoWriter {
	private const ushort LegacyMagic = 0x011A;
	private const int HeaderSize = 12;
	private const byte BooleanPresent = 0x01;
	private const short ValueAbsent = -1;

	/// <summary>
	/// Writes one representable terminal description as deterministic legacy
	/// <c>0432</c> compiled bytes.
	/// </summary>
	/// <param name="description">
	/// The immutable terminal description to serialize.
	/// </param>
	/// <returns>A newly allocated compiled terminfo entry.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The terminal identity or a standard or extended capability cannot be
	/// represented exactly by the C03 legacy format.
	/// </exception>
	public static byte[] Write(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		string identity =
			CreateIdentity( description );
		byte[] identityBytes =
			Encoding.Latin1.GetBytes( identity );
		int namesSize =
			checked( identityBytes.Length + 1 );

		if ( namesSize > ushort.MaxValue ) {
			throw new InvalidOperationException(
				$"The compiled names section requires {namesSize} bytes, exceeding the legacy 16-bit section-size field."
			);
		}

		int booleanCount =
			GetBooleanCount( description );
		int numericCount =
			GetNumericCount( description );
		int stringCount =
			GetStringCount( description );
		(short[] stringOffsets, byte[] stringTable) =
			CreateStringTable(
				description,
				stringCount
			);

		int booleanOffset =
			checked( HeaderSize + namesSize );
		int unalignedNumericOffset =
			checked( booleanOffset + booleanCount );
		int alignmentSize =
			( ( unalignedNumericOffset & 1 ) == 0 )
				? 0
				: 1
		;
		int numericOffset =
			checked( unalignedNumericOffset + alignmentSize );
		int stringOffsetTableOffset =
			checked(
				numericOffset
				+ checked( numericCount * sizeof( short ) )
			);
		int stringTableOffset =
			checked(
				stringOffsetTableOffset
				+ checked( stringCount * sizeof( short ) )
			);
		int entrySize =
			checked(
				stringTableOffset
				+ stringTable.Length
			);

		byte[] entry =
			new byte[entrySize];

		WriteHeader(
			entry,
			namesSize,
			booleanCount,
			numericCount,
			stringCount,
			stringTable.Length
		);

		identityBytes.CopyTo(
			entry.AsSpan( HeaderSize )
		);
		entry[HeaderSize + identityBytes.Length] = 0;

		WriteBooleans(
			entry,
			booleanOffset,
			description
		);
		WriteNumerics(
			entry,
			numericOffset,
			numericCount,
			description
		);
		WriteStringOffsets(
			entry,
			stringOffsetTableOffset,
			stringOffsets
		);
		stringTable.CopyTo(
			entry.AsSpan( stringTableOffset )
		);

		if ( description.ExtendedCapabilities.Count == 0 ) {
			return entry;
		}

		return AppendExtendedSection(
			entry,
			description
		);
	}

	private static void WriteHeader(
		Span<byte> entry,
		int namesSize,
		int booleanCount,
		int numericCount,
		int stringCount,
		int stringTableSize
	) {
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[0..2],
			LegacyMagic
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[2..4],
			ToUnsignedShort(
				namesSize,
				"names section size"
			)
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[4..6],
			ToUnsignedShort(
				booleanCount,
				"Boolean table count"
			)
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[6..8],
			ToUnsignedShort(
				numericCount,
				"numeric table count"
			)
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[8..10],
			ToUnsignedShort(
				stringCount,
				"string table count"
			)
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[10..12],
			ToUnsignedShort(
				stringTableSize,
				"string table size"
			)
		);
	}

	private static void WriteBooleans(
		Span<byte> entry,
		int booleanOffset,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		foreach ( BooleanCapability capability in description.BooleanCapabilities ) {
			int binaryIndex =
				StandardCapabilityCatalog
					.GetMetadata( capability )
					.BinaryIndex;
			entry[booleanOffset + binaryIndex] = BooleanPresent;
		}
	}

	private static void WriteNumerics(
		Span<byte> entry,
		int numericOffset,
		int numericCount,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		for ( int index = 0; index < numericCount; index++ ) {
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.Slice(
					numericOffset + ( index * sizeof( short ) ),
					sizeof( short )
				),
				ValueAbsent
			);
		}

		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			StandardCapabilityMetadata<NumericCapability> metadata =
				StandardCapabilityCatalog.GetMetadata( pair.Key );
			ValidateLegacyNumericValue(
				pair.Value,
				metadata.ShortName
			);
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.Slice(
					numericOffset
						+ ( metadata.BinaryIndex * sizeof( short ) ),
					sizeof( short )
				),
				(short)pair.Value
			);
		}
	}

	private static void WriteStringOffsets(
		Span<byte> entry,
		int stringOffsetTableOffset,
		IReadOnlyList<short> stringOffsets
	) {
		ArgumentNullException.ThrowIfNull( stringOffsets );

		for ( int index = 0; index < stringOffsets.Count; index++ ) {
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.Slice(
					stringOffsetTableOffset
						+ ( index * sizeof( short ) ),
					sizeof( short )
				),
				stringOffsets[index]
			);
		}
	}

	private static int GetBooleanCount(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		int count = 0;
		foreach ( BooleanCapability capability in description.BooleanCapabilities ) {
			int binaryIndex =
				StandardCapabilityCatalog
					.GetMetadata( capability )
					.BinaryIndex;
			count =
				Math.Max(
					count,
					checked( binaryIndex + 1 )
				);
		}

		return count;
	}

	private static int GetNumericCount(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		int count = 0;
		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			int binaryIndex =
				StandardCapabilityCatalog
					.GetMetadata( pair.Key )
					.BinaryIndex;
			count =
				Math.Max(
					count,
					checked( binaryIndex + 1 )
				);
		}

		return count;
	}

	private static int GetStringCount(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		int count = 0;
		foreach (
			KeyValuePair<StringCapability, string> pair
			in description.StringCapabilities
		) {
			int binaryIndex =
				StandardCapabilityCatalog
					.GetMetadata( pair.Key )
					.BinaryIndex;
			count =
				Math.Max(
					count,
					checked( binaryIndex + 1 )
				);
		}

		return count;
	}

	private static (
		short[] Offsets,
		byte[] Bytes
	) CreateStringTable(
		TerminalDescription description,
		int stringCount
	) {
		ArgumentNullException.ThrowIfNull( description );

		short[] offsets =
			new short[stringCount];
		Array.Fill(
			offsets,
			ValueAbsent
		);
		byte[]?[] encodedStrings =
			new byte[]?[stringCount];
		int tableSize = 0;

		foreach (
			KeyValuePair<StringCapability, string> pair
			in description.StringCapabilities
		) {
			StandardCapabilityMetadata<StringCapability> metadata =
				StandardCapabilityCatalog.GetMetadata( pair.Key );
			string role =
				$"standard string capability '{metadata.ShortName}'";
			ValidateLatinOneTerminatedValue(
				pair.Value,
				role
			);

			if ( tableSize > short.MaxValue ) {
				throw new InvalidOperationException(
					$"The {role} would begin at string-table offset {tableSize}, exceeding the signed 16-bit offset field."
				);
			}

			int remaining =
				ushort.MaxValue - tableSize;
			if ( pair.Value.Length >= remaining ) {
				throw new InvalidOperationException(
					$"The {role} would grow the compiled string table beyond the unsigned 16-bit size field."
				);
			}

			byte[] encoded =
				Encoding.Latin1.GetBytes( pair.Value );
			offsets[metadata.BinaryIndex] =
				(short)tableSize;
			encodedStrings[metadata.BinaryIndex] = encoded;
			tableSize =
				checked(
					tableSize
					+ encoded.Length
					+ 1
				);
		}

		byte[] table =
			new byte[tableSize];
		for ( int index = 0; index < encodedStrings.Length; index++ ) {
			byte[]? encoded =
				encodedStrings[index];
			if ( encoded is null ) {
				continue;
			}

			int offset =
				offsets[index];
			encoded.CopyTo(
				table.AsSpan( offset )
			);
		}

		return (
			offsets,
			table
		);
	}

	private static void ValidateLegacyNumericValue(
		int value,
		string capabilityName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );

		if ( value < 0 || value > short.MaxValue ) {
			throw new InvalidOperationException(
				$"Standard numeric capability '{capabilityName}' has value {value}, which cannot be represented by legacy 0432 without colliding with sentinels or exceeding the signed 16-bit range."
			);
		}
	}

	private static ushort ToUnsignedShort(
		int value,
		string role
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( role );

		if ( value < 0 || value > ushort.MaxValue ) {
			throw new InvalidOperationException(
				$"The {role} value {value} cannot be represented by the unsigned 16-bit header field."
			);
		}

		return (ushort)value;
	}

	private static string CreateIdentity(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		string descriptionText =
			description.Description
			?? throw new InvalidOperationException(
				"The conventional compiled names section requires a verbose terminal description."
			);

		ValidateIdentityComponent(
			description.Name,
			"canonical terminal name"
		);
		foreach ( string alias in description.Aliases ) {
			ValidateIdentityComponent(
				alias,
				"terminal alias"
			);
		}
		ValidateIdentityComponent(
			descriptionText,
			"verbose terminal description"
		);

		string[] fields =
			new string[description.Aliases.Count + 2];
		fields[0] = description.Name;
		for ( int index = 0; index < description.Aliases.Count; index++ ) {
			fields[index + 1] = description.Aliases[index];
		}
		fields[^1] = descriptionText;

		return string.Join(
			'|',
			fields
		);
	}

	private static void ValidateIdentityComponent(
		string value,
		string role
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentException.ThrowIfNullOrWhiteSpace( role );

		ValidateLatinOneTerminatedValue(
			value,
			role
		);

		if ( value.IndexOf( '|' ) >= 0 ) {
			throw new InvalidOperationException(
				$"The {role} contains the compiled names separator '|'."
			);
		}
	}

	private static void ValidateLatinOneTerminatedValue(
		string value,
		string role
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentException.ThrowIfNullOrWhiteSpace( role );

		if ( value.IndexOf( '\0' ) >= 0 ) {
			throw new InvalidOperationException(
				$"The {role} contains an embedded NUL and cannot be represented by a NUL-terminated compiled field."
			);
		}

		foreach ( char character in value ) {
			if ( character > '\u00FF' ) {
				throw new InvalidOperationException(
					$"The {role} contains U+{(int)character:X4}, which cannot be represented exactly in Latin-1."
				);
			}
		}
	}
}