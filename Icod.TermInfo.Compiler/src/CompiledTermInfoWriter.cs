using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Serializes immutable terminal descriptions into conventional compiled
/// terminfo entries.
/// </summary>
/// <remarks>
/// C04 supports deterministic legacy <c>0432</c> and wide-numeric <c>01036</c>
/// entries, including the supported ncurses extended-capability section.
/// Automatic policy prefers the narrow legacy representation whenever it is
/// sufficient. Writing is pure with respect to filesystem, environment, and
/// native ncurses state.
/// </remarks>
public static partial class CompiledTermInfoWriter {
	private const ushort LegacyMagic = 0x011A;
	private const ushort WideMagic = 0x021E;
	private const int HeaderSize = 12;
	private const byte BooleanPresent = 0x01;
	private const int ValueAbsent = -1;

	/// <summary>
	/// Writes one terminal description using automatic format selection.
	/// </summary>
	/// <param name="description">
	/// The immutable terminal description to serialize.
	/// </param>
	/// <returns>A newly allocated compiled terminfo entry.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The description cannot be represented exactly by a supported compiled
	/// format.
	/// </exception>
	public static byte[] Write(
		TerminalDescription description
	) {
		return Write(
			description,
			new CompiledTermInfoWriterOptions()
		);
	}

	/// <summary>
	/// Writes one terminal description using explicit format policy.
	/// </summary>
	/// <param name="description">
	/// The immutable terminal description to serialize.
	/// </param>
	/// <param name="options">
	/// Immutable format-selection options.
	/// </param>
	/// <returns>A newly allocated compiled terminfo entry.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The description cannot be represented exactly by the requested compiled
	/// format and extended-section policy.
	/// </exception>
	public static byte[] Write(
		TerminalDescription description,
		CompiledTermInfoWriterOptions options
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );

		try {
			return WriteCore(
				description,
				options
			);
		} catch ( OverflowException exception ) {
			throw new InvalidOperationException(
				"The compiled terminfo entry cannot be represented because size arithmetic overflowed.",
				exception
			);
		}
	}

	private static byte[] WriteCore(
		TerminalDescription description,
		CompiledTermInfoWriterOptions options
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );

		if ( !options.IncludeExtendedCapabilities
			&& description.ExtendedCapabilities.Count != 0 ) {
			throw new InvalidOperationException(
				"The requested writer options exclude the ncurses extended section, but the terminal description contains extended capabilities."
			);
		}

		CompiledTermInfoFormat format =
			ResolveFormat(
				description,
				options.Format
			);
		int numericWidth =
			GetNumericWidth( format );

		string identity =
			CreateIdentity( description );
		byte[] identityBytes =
			Encoding.Latin1.GetBytes( identity );
		int namesSize =
			checked( identityBytes.Length + 1 );

		if ( namesSize > ushort.MaxValue ) {
			throw new InvalidOperationException(
				$"The compiled names section requires {namesSize} bytes, exceeding the unsigned 16-bit section-size field."
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
				+ checked( numericCount * numericWidth )
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
			GetMagic( format ),
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
			numericWidth,
			format,
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
			description,
			numericWidth,
			format
		);
	}

	private static void WriteHeader(
		Span<byte> entry,
		ushort magic,
		int namesSize,
		int booleanCount,
		int numericCount,
		int stringCount,
		int stringTableSize
	) {
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry[0..2],
			magic
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
		int numericWidth,
		CompiledTermInfoFormat format,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		for ( int index = 0; index < numericCount; index++ ) {
			WriteNumericValue(
				entry,
				numericOffset + ( index * numericWidth ),
				numericWidth,
				ValueAbsent
			);
		}

		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			StandardCapabilityMetadata<NumericCapability> metadata =
				StandardCapabilityCatalog.GetMetadata( pair.Key );
			ValidateNumericValueForFormat(
				pair.Value,
				$"Standard numeric capability '{metadata.ShortName}'",
				format
			);
			WriteNumericValue(
				entry,
				numericOffset
					+ ( metadata.BinaryIndex * numericWidth ),
				numericWidth,
				pair.Value
			);
		}
	}

	private static void WriteNumericValue(
		Span<byte> entry,
		int offset,
		int numericWidth,
		int value
	) {
		if ( numericWidth == sizeof( short ) ) {
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.Slice(
					offset,
					sizeof( short )
				),
				(short)value
			);
			return;
		}

		if ( numericWidth == sizeof( int ) ) {
			BinaryPrimitives.WriteInt32LittleEndian(
				entry.Slice(
					offset,
					sizeof( int )
				),
				value
			);
			return;
		}

		throw new ArgumentOutOfRangeException(
			nameof( numericWidth ),
			numericWidth,
			"Compiled numeric width must be two or four bytes."
		);
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
			(short)ValueAbsent
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

	private static CompiledTermInfoFormat ResolveFormat(
		TerminalDescription description,
		CompiledTermInfoFormat requestedFormat
	) {
		ArgumentNullException.ThrowIfNull( description );

		bool requiresWide =
			RequiresWideNumericRepresentation( description );

		if ( requestedFormat == CompiledTermInfoFormat.Automatic ) {
			return requiresWide
				? CompiledTermInfoFormat.Wide
				: CompiledTermInfoFormat.Legacy
			;
		}

		if ( requestedFormat == CompiledTermInfoFormat.Legacy ) {
			if ( requiresWide ) {
				throw new InvalidOperationException(
					"The requested legacy 0432 representation cannot encode one or more present numeric values without truncation."
				);
			}

			return CompiledTermInfoFormat.Legacy;
		}

		if ( requestedFormat == CompiledTermInfoFormat.Wide ) {
			return CompiledTermInfoFormat.Wide;
		}

		throw new ArgumentOutOfRangeException(
			nameof( requestedFormat ),
			requestedFormat,
			"Compiled terminfo format must be Automatic, Legacy, or Wide."
		);
	}

	private static bool RequiresWideNumericRepresentation(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		bool requiresWide = false;
		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			StandardCapabilityMetadata<NumericCapability> metadata =
				StandardCapabilityCatalog.GetMetadata( pair.Key );
			ValidatePresentNumericValue(
				pair.Value,
				$"Standard numeric capability '{metadata.ShortName}'"
			);
			requiresWide |= pair.Value > short.MaxValue;
		}

		foreach (
			KeyValuePair<string, TermInfoCapabilityValue> pair
			in description.ExtendedCapabilities
		) {
			if ( !pair.Value.IsNumber ) {
				continue;
			}

			ValidatePresentNumericValue(
				pair.Value.NumberValue,
				$"Extended numeric capability '{pair.Key}'"
			);
			requiresWide |= pair.Value.NumberValue > short.MaxValue;
		}

		return requiresWide;
	}

	private static void ValidateNumericValueForFormat(
		int value,
		string role,
		CompiledTermInfoFormat format
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( role );
		ValidatePresentNumericValue(
			value,
			role
		);

		if ( format == CompiledTermInfoFormat.Legacy
			&& value > short.MaxValue ) {
			throw new InvalidOperationException(
				$"{role} has value {value}, which cannot be represented by legacy 0432 without exceeding the signed 16-bit range."
			);
		}
	}

	private static void ValidatePresentNumericValue(
		int value,
		string role
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( role );

		if ( value < 0 ) {
			throw new InvalidOperationException(
				$"{role} has value {value}, which collides with compiled absent/canceled sentinel semantics."
			);
		}
	}

	private static ushort GetMagic(
		CompiledTermInfoFormat format
	) {
		return format switch {
			CompiledTermInfoFormat.Legacy => LegacyMagic,
			CompiledTermInfoFormat.Wide => WideMagic,
			_ => throw new ArgumentOutOfRangeException(
				nameof( format ),
				format,
				"Resolved compiled terminfo format must be Legacy or Wide."
			),
		};
	}

	private static int GetNumericWidth(
		CompiledTermInfoFormat format
	) {
		return format switch {
			CompiledTermInfoFormat.Legacy => sizeof( short ),
			CompiledTermInfoFormat.Wide => sizeof( int ),
			_ => throw new ArgumentOutOfRangeException(
				nameof( format ),
				format,
				"Resolved compiled terminfo format must be Legacy or Wide."
			),
		};
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