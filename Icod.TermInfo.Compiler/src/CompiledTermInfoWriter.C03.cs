using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Compiler;

public static partial class CompiledTermInfoWriter {
	private const int ExtendedHeaderSize = 10;

	private static byte[] AppendExtendedSection(
		byte[] conventionalEntry,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( conventionalEntry );
		ArgumentNullException.ThrowIfNull( description );

		KeyValuePair<string, TermInfoCapabilityValue>[] booleans =
			description.ExtendedCapabilities
				.Where( pair => pair.Value.IsBoolean )
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.ToArray();
		KeyValuePair<string, TermInfoCapabilityValue>[] numerics =
			description.ExtendedCapabilities
				.Where( pair => pair.Value.IsNumber )
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.ToArray();
		KeyValuePair<string, TermInfoCapabilityValue>[] strings =
			description.ExtendedCapabilities
				.Where( pair => pair.Value.IsString )
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.ToArray();

		ValidateExtendedBooleans( booleans );
		ValidateExtendedNumerics( numerics );

		short[] stringOffsets =
			new short[strings.Length];
		byte[][] stringBytes =
			new byte[strings.Length][];
		int valueTableSize = 0;
		for ( int index = 0; index < strings.Length; index++ ) {
			KeyValuePair<string, TermInfoCapabilityValue> pair =
				strings[index];
			ValidateExtendedName( pair.Key );
			string role =
				$"extended string capability '{pair.Key}'";
			string value =
				pair.Value.StringValue;
			ValidateLatinOneTerminatedValue(
				value,
				role
			);

			if ( valueTableSize > short.MaxValue ) {
				throw new InvalidOperationException(
					$"The {role} would begin at extended string-table offset {valueTableSize}, exceeding the signed 16-bit offset field."
				);
			}

			byte[] encoded =
				Encoding.Latin1.GetBytes( value );
			stringOffsets[index] =
				(short)valueTableSize;
			stringBytes[index] = encoded;
			valueTableSize =
				checked(
					valueTableSize
					+ encoded.Length
					+ 1
				);
		}

		string[] names =
			booleans.Select( pair => pair.Key )
				.Concat( numerics.Select( pair => pair.Key ) )
				.Concat( strings.Select( pair => pair.Key ) )
				.ToArray();
		short[] nameOffsets =
			new short[names.Length];
		byte[][] nameBytes =
			new byte[names.Length][];
		int nameTableSize = 0;
		for ( int index = 0; index < names.Length; index++ ) {
			string name = names[index];
			ValidateExtendedName( name );

			if ( nameTableSize > short.MaxValue ) {
				throw new InvalidOperationException(
					$"Extended capability name '{name}' would begin at name-table offset {nameTableSize}, exceeding the signed 16-bit offset field."
				);
			}

			byte[] encoded =
				Encoding.Latin1.GetBytes( name );
			nameOffsets[index] =
				(short)nameTableSize;
			nameBytes[index] = encoded;
			nameTableSize =
				checked(
					nameTableSize
					+ encoded.Length
					+ 1
				);
		}

		int stringTableSize =
			checked(
				valueTableSize
				+ nameTableSize
			);
		if ( stringTableSize > ushort.MaxValue ) {
			throw new InvalidOperationException(
				$"The extended string table requires {stringTableSize} bytes, exceeding the unsigned 16-bit size field."
			);
		}

		int nameCount =
			checked(
				booleans.Length
				+ numerics.Length
				+ strings.Length
			);
		int stringTableItemCount =
			checked(
				nameCount
				+ strings.Length
			);

		ushort booleanCountField =
			ToUnsignedShort(
				booleans.Length,
				"extended Boolean count"
			);
		ushort numericCountField =
			ToUnsignedShort(
				numerics.Length,
				"extended numeric count"
			);
		ushort stringCountField =
			ToUnsignedShort(
				strings.Length,
				"extended string count"
			);
		ushort itemCountField =
			ToUnsignedShort(
				stringTableItemCount,
				"extended string-table item count"
			);
		ushort stringTableSizeField =
			ToUnsignedShort(
				stringTableSize,
				"extended string-table size"
			);

		int headerAlignmentSize =
			( ( conventionalEntry.Length & 1 ) == 0 )
				? 0
				: 1
		;
		int headerOffset =
			checked(
				conventionalEntry.Length
				+ headerAlignmentSize
			);
		int booleanOffset =
			checked(
				headerOffset
				+ ExtendedHeaderSize
			);
		int unalignedNumericOffset =
			checked(
				booleanOffset
				+ booleans.Length
			);
		int numericAlignmentSize =
			( ( unalignedNumericOffset & 1 ) == 0 )
				? 0
				: 1
		;
		int numericOffset =
			checked(
				unalignedNumericOffset
				+ numericAlignmentSize
			);
		int stringOffsetTableOffset =
			checked(
				numericOffset
				+ checked( numerics.Length * sizeof( short ) )
			);
		int nameOffsetTableOffset =
			checked(
				stringOffsetTableOffset
				+ checked( strings.Length * sizeof( short ) )
			);
		int stringTableOffset =
			checked(
				nameOffsetTableOffset
				+ checked( nameCount * sizeof( short ) )
			);
		int entrySize =
			checked(
				stringTableOffset
				+ stringTableSize
			);

		byte[] entry =
			new byte[entrySize];
		conventionalEntry.CopyTo(
			entry.AsSpan()
		);

		WriteExtendedHeader(
			entry.AsSpan( headerOffset, ExtendedHeaderSize ),
			booleanCountField,
			numericCountField,
			stringCountField,
			itemCountField,
			stringTableSizeField
		);

		for ( int index = 0; index < booleans.Length; index++ ) {
			entry[booleanOffset + index] = BooleanPresent;
		}

		for ( int index = 0; index < numerics.Length; index++ ) {
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.AsSpan(
					numericOffset + ( index * sizeof( short ) ),
					sizeof( short )
				),
				(short)numerics[index].Value.NumberValue
			);
		}

		WriteStringOffsets(
			entry,
			stringOffsetTableOffset,
			stringOffsets
		);
		WriteStringOffsets(
			entry,
			nameOffsetTableOffset,
			nameOffsets
		);

		int tableCursor =
			stringTableOffset;
		foreach ( byte[] encoded in stringBytes ) {
			encoded.CopyTo(
				entry.AsSpan( tableCursor )
			);
			tableCursor =
				checked(
					tableCursor
					+ encoded.Length
					+ 1
				);
		}
		foreach ( byte[] encoded in nameBytes ) {
			encoded.CopyTo(
				entry.AsSpan( tableCursor )
			);
			tableCursor =
				checked(
					tableCursor
					+ encoded.Length
					+ 1
				);
		}

		return entry;
	}

	private static void WriteExtendedHeader(
		Span<byte> header,
		ushort booleanCount,
		ushort numericCount,
		ushort stringCount,
		ushort stringTableItemCount,
		ushort stringTableSize
	) {
		if ( header.Length != ExtendedHeaderSize ) {
			throw new ArgumentException(
				$"The extended header span must contain exactly {ExtendedHeaderSize} bytes.",
				nameof( header )
			);
		}

		BinaryPrimitives.WriteUInt16LittleEndian(
			header[0..2],
			booleanCount
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			header[2..4],
			numericCount
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			header[4..6],
			stringCount
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			header[6..8],
			stringTableItemCount
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			header[8..10],
			stringTableSize
		);
	}

	private static void ValidateExtendedBooleans(
		IReadOnlyList<KeyValuePair<string, TermInfoCapabilityValue>> booleans
	) {
		ArgumentNullException.ThrowIfNull( booleans );

		foreach ( KeyValuePair<string, TermInfoCapabilityValue> pair in booleans ) {
			ValidateExtendedName( pair.Key );
			if ( !pair.Value.BooleanValue ) {
				throw new InvalidOperationException(
					$"Extended Boolean capability '{pair.Key}' has a false effective value and therefore cannot be emitted as a present compiled capability."
				);
			}
		}
	}

	private static void ValidateExtendedNumerics(
		IReadOnlyList<KeyValuePair<string, TermInfoCapabilityValue>> numerics
	) {
		ArgumentNullException.ThrowIfNull( numerics );

		foreach ( KeyValuePair<string, TermInfoCapabilityValue> pair in numerics ) {
			ValidateExtendedName( pair.Key );
			int value =
				pair.Value.NumberValue;
			if ( value < 0 || value > short.MaxValue ) {
				throw new InvalidOperationException(
					$"Extended numeric capability '{pair.Key}' has value {value}, which cannot be represented by legacy 0432 without colliding with sentinels or exceeding the signed 16-bit range."
				);
			}
		}
	}

	private static void ValidateExtendedName(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		ValidateLatinOneTerminatedValue(
			name,
			$"extended capability name '{name}'"
		);
	}
}