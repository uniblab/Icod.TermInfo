using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C03WriterTests {
	[Fact]
	public void StandardAndAllExtendedKindsRoundTripSemantically() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-roundtrip" )
				.AddAlias( "c03" )
				.SetDescription( "C03 extended terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString(
					StringCapability.ClearScreen,
					"\u001b[H\u001b[2J"
				)
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 12345 )
				.SetExtendedString(
					"XStr",
					"alpha\u001bbeta"
				)
				.Build();

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					description
				)
			);

		Assert.Equal( description.Name, parsed.Name );
		Assert.Equal( description.Description, parsed.Description );
		Assert.Equal( description.Aliases, parsed.Aliases );
		Assert.True( parsed.GetBoolean( BooleanCapability.AutoRightMargin ) );
		Assert.Equal(
			(int?)132,
			parsed.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			"\u001b[H\u001b[2J",
			parsed.GetString( StringCapability.ClearScreen )
		);
		Assert.Equal( 3, parsed.ExtendedCapabilities.Count );
		Assert.True( parsed.ExtendedCapabilities["XBool"].BooleanValue );
		Assert.Equal( 12345, parsed.ExtendedCapabilities["XNum"].NumberValue );
		Assert.Equal(
			"alpha\u001bbeta",
			parsed.ExtendedCapabilities["XStr"].StringValue
		);
	}

	[Fact]
	public void ExtendedBodyUsesOrdinalKindOrderingAndNcursesOffsets() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-layout" )
				.SetDescription( "C03 layout terminal" )
				.SetExtendedString( "zStr", "zulu" )
				.SetExtendedNumber( "zNum", 22 )
				.SetExtendedBoolean( "zBool" )
				.SetExtendedString( "AStr", "alpha" )
				.SetExtendedNumber( "ANum", 11 )
				.SetExtendedBoolean( "ABool" )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		int headerOffset =
			GetExtendedHeaderOffset( compiled );

		Assert.Equal(
			2,
			ReadUInt16( compiled, headerOffset )
		);
		Assert.Equal(
			2,
			ReadUInt16( compiled, headerOffset + 2 )
		);
		Assert.Equal(
			2,
			ReadUInt16( compiled, headerOffset + 4 )
		);
		Assert.Equal(
			8,
			ReadUInt16( compiled, headerOffset + 6 )
		);

		int booleanOffset =
			headerOffset + 10;
		Assert.Equal( 1, compiled[booleanOffset] );
		Assert.Equal( 1, compiled[booleanOffset + 1] );

		int numericOffset =
			booleanOffset + 2;
		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}
		Assert.Equal(
			11,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan( numericOffset, sizeof( short ) )
			)
		);
		Assert.Equal(
			22,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					numericOffset + sizeof( short ),
					sizeof( short )
				)
			)
		);

		int stringOffsetTableOffset =
			numericOffset + ( 2 * sizeof( short ) );
		Assert.Equal(
			0,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					stringOffsetTableOffset,
					sizeof( short )
				)
			)
		);
		Assert.Equal(
			6,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					stringOffsetTableOffset + sizeof( short ),
					sizeof( short )
				)
			)
		);

		int nameOffsetTableOffset =
			stringOffsetTableOffset + ( 2 * sizeof( short ) );
		short[] expectedNameOffsets = [
			0,
			6,
			12,
			17,
			22,
			27,
		];
		for ( int index = 0; index < expectedNameOffsets.Length; index++ ) {
			Assert.Equal(
				expectedNameOffsets[index],
				BinaryPrimitives.ReadInt16LittleEndian(
					compiled.AsSpan(
						nameOffsetTableOffset
							+ ( index * sizeof( short ) ),
						sizeof( short )
					)
				)
			);
		}

		int stringTableOffset =
			nameOffsetTableOffset
			+ ( expectedNameOffsets.Length * sizeof( short ) );
		int stringTableSize =
			ReadUInt16(
				compiled,
				headerOffset + 8
			);
		Assert.Equal(
			"alpha\0zulu\0ABool\0zBool\0ANum\0zNum\0AStr\0zStr\0",
			Encoding.Latin1.GetString(
				compiled.AsSpan(
					stringTableOffset,
					stringTableSize
				)
			)
		);
	}

	[Fact]
	public void ExtendedOutputIsIndependentOfInsertionOrder() {
		TerminalDescription first =
			new TerminalDescriptionBuilder( "c03-deterministic" )
				.SetDescription( "C03 deterministic terminal" )
				.SetExtendedBoolean( "ZB" )
				.SetExtendedNumber( "AN", 2 )
				.SetExtendedString( "ZS", "z" )
				.SetExtendedBoolean( "AB" )
				.SetExtendedNumber( "ZN", 3 )
				.SetExtendedString( "AS", "a" )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "c03-deterministic" )
				.SetDescription( "C03 deterministic terminal" )
				.SetExtendedString( "AS", "a" )
				.SetExtendedNumber( "ZN", 3 )
				.SetExtendedBoolean( "AB" )
				.SetExtendedString( "ZS", "z" )
				.SetExtendedNumber( "AN", 2 )
				.SetExtendedBoolean( "ZB" )
				.Build();

		Assert.Equal(
			CompiledTermInfoWriter.Write( first ),
			CompiledTermInfoWriter.Write( second )
		);
	}

	[Fact]
	public void ExtendedHeaderAndNumericTableReceiveRequiredAlignment() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-align" )
				.SetDescription( "C03 alignment terminal" )
				.SetString(
					StandardCapabilityCatalog.StringCapabilities[0].Capability,
					"ab"
				)
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 7 )
				.Build();
		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		int conventionalEnd =
			GetConventionalEnd( compiled );

		Assert.True( ( conventionalEnd & 1 ) != 0 );
		Assert.Equal( 0, compiled[conventionalEnd] );

		int headerOffset =
			conventionalEnd + 1;
		int unalignedNumericOffset =
			headerOffset + 10 + 1;
		Assert.True( ( unalignedNumericOffset & 1 ) != 0 );
		Assert.Equal( 0, compiled[unalignedNumericOffset] );
		Assert.Equal(
			7,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					unalignedNumericOffset + 1,
					sizeof( short )
				)
			)
		);
	}

	[Fact]
	public void LegacyMaximumExtendedNumericValueRoundTrips() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-max-number" )
				.SetDescription( "C03 maximum extended number" )
				.SetExtendedNumber(
					"XNum",
					short.MaxValue
				)
				.Build();

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					description
				)
			);

		Assert.Equal(
			(int)short.MaxValue,
			parsed.ExtendedCapabilities["XNum"].NumberValue
		);
	}

	[Theory]
	[InlineData( -3 )]
	[InlineData( -2 )]
	[InlineData( -1 )]
	[InlineData( 32768 )]
	[InlineData( int.MaxValue )]
	public void ExtendedNumericsOutsideLegacyPresentRangeAreRejected(
		int value
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-invalid-number" )
				.SetDescription( "C03 invalid extended number" )
				.SetExtendedNumber(
					"XNum",
					value
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write(
				description,
				new CompiledTermInfoWriterOptions(
					CompiledTermInfoFormat.Legacy
				)
			)
		);
	}

	[Theory]
	[InlineData( "name\0nul" )]
	[InlineData( "name-\u0100" )]
	public void UnrepresentableExtendedNamesAreRejected(
		string name
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-invalid-name" )
				.SetDescription( "C03 invalid extended name" )
				.SetExtendedBoolean( name )
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Theory]
	[InlineData( "value\0nul" )]
	[InlineData( "value-\u0100" )]
	public void UnrepresentableExtendedStringsAreRejected(
		string value
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-invalid-string" )
				.SetDescription( "C03 invalid extended string" )
				.SetExtendedString(
					"XStr",
					value
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Fact]
	public void HighLatinOneExtendedNamesAndValuesRoundTrip() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-latin-one" )
				.SetDescription( "C03 Latin-1 terminal" )
				.SetExtendedString(
					"X\u00e9",
					"caf\u00e9"
				)
				.Build();

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					description
				)
			);

		Assert.Equal(
			"caf\u00e9",
			parsed.ExtendedCapabilities["X\u00e9"].StringValue
		);
	}

	[Fact]
	public void SignedExtendedStringOffsetOverflowIsRejectedBeforeNarrowing() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-value-offset" )
				.SetDescription( "C03 extended value offset overflow" )
				.SetExtendedString(
					"AFirst",
					new string(
						'a',
						32768
					)
				)
				.SetExtendedString(
					"BSecond",
					"second"
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Fact]
	public void SignedExtendedNameOffsetOverflowIsRejectedBeforeNarrowing() {
		string firstName =
			new string(
				'a',
				32768
			);
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-name-offset" )
				.SetDescription( "C03 extended name offset overflow" )
				.SetExtendedBoolean( firstName )
				.SetExtendedBoolean( "b" )
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Fact]
	public void UnsignedExtendedStringTableSizeOverflowIsRejectedBeforeNarrowing() {
		string longName =
			new string(
				'x',
				30000
			);
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c03-table-size" )
				.SetDescription( "C03 extended string table overflow" )
				.SetExtendedString(
					longName,
					new string(
						'y',
						40000
					)
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	private static int GetExtendedHeaderOffset(
		byte[] compiled
	) {
		ArgumentNullException.ThrowIfNull( compiled );

		int conventionalEnd =
			GetConventionalEnd( compiled );
		return ( ( conventionalEnd & 1 ) == 0 )
			? conventionalEnd
			: conventionalEnd + 1
		;
	}

	private static int GetConventionalEnd(
		byte[] compiled
	) {
		ArgumentNullException.ThrowIfNull( compiled );

		int namesSize =
			ReadUInt16( compiled, 2 );
		int booleanCount =
			ReadUInt16( compiled, 4 );
		int numericCount =
			ReadUInt16( compiled, 6 );
		int stringCount =
			ReadUInt16( compiled, 8 );
		int stringTableSize =
			ReadUInt16( compiled, 10 );

		int numericOffset =
			12 + namesSize + booleanCount;
		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}

		return numericOffset
			+ ( numericCount * sizeof( short ) )
			+ ( stringCount * sizeof( short ) )
			+ stringTableSize;
	}

	private static int ReadUInt16(
		byte[] bytes,
		int offset
	) {
		ArgumentNullException.ThrowIfNull( bytes );

		return BinaryPrimitives.ReadUInt16LittleEndian(
			bytes.AsSpan(
				offset,
				sizeof( ushort )
			)
		);
	}
}