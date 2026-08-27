using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C02WriterTests {
	[Fact]
	public void CompleteStandardCatalogRoundTripsSemantically() {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( "c02-complete" )
				.AddAlias( "c02-full" )
				.SetDescription( "C02 complete standard terminal" );

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			builder.SetBoolean( metadata.Capability );
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			builder.SetNumber(
				metadata.Capability,
				metadata.BinaryIndex
			);
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			builder.SetString(
				metadata.Capability,
				$"c02-string-{metadata.BinaryIndex}"
			);
		}

		TerminalDescription description =
			builder.Build();
		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					description
				)
			);

		Assert.Equal( description.Name, parsed.Name );
		Assert.Equal( description.Description, parsed.Description );
		Assert.Equal( description.Aliases, parsed.Aliases );
		Assert.Equal<BooleanCapability>(
			description.BooleanCapabilities,
			parsed.BooleanCapabilities
		);
		Assert.Equal<KeyValuePair<NumericCapability, int>>(
			description.NumericCapabilities,
			parsed.NumericCapabilities
		);
		Assert.Equal<KeyValuePair<StringCapability, string>>(
			description.StringCapabilities,
			parsed.StringCapabilities
		);
		Assert.Empty( parsed.ExtendedCapabilities );
	}

	[Fact]
	public void SparseTablesUseCanonicalBinaryIndicesAndAbsentSentinels() {
		StandardCapabilityMetadata<BooleanCapability> booleanMetadata =
			StandardCapabilityCatalog.BooleanCapabilities[4];
		StandardCapabilityMetadata<NumericCapability> numericMetadata =
			StandardCapabilityCatalog.NumericCapabilities[6];
		StandardCapabilityMetadata<StringCapability> stringMetadata =
			StandardCapabilityCatalog.StringCapabilities[8];
		const int numericValue = 1234;
		const string stringValue = "c02-\u00e9";

		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-sparse" )
				.SetDescription( "C02 sparse terminal" )
				.SetBoolean( booleanMetadata.Capability )
				.SetNumber(
					numericMetadata.Capability,
					numericValue
				)
				.SetString(
					stringMetadata.Capability,
					stringValue
				)
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		int namesSize =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 2, 2 )
			);
		int booleanCount =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 4, 2 )
			);
		int numericCount =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 6, 2 )
			);
		int stringCount =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 8, 2 )
			);
		int stringTableSize =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 10, 2 )
			);

		Assert.Equal(
			booleanMetadata.BinaryIndex + 1,
			booleanCount
		);
		Assert.Equal(
			numericMetadata.BinaryIndex + 1,
			numericCount
		);
		Assert.Equal(
			stringMetadata.BinaryIndex + 1,
			stringCount
		);
		Assert.True(
			booleanCount
				< StandardCapabilityCatalog.BooleanCapabilities.Count
		);
		Assert.True(
			numericCount
				< StandardCapabilityCatalog.NumericCapabilities.Count
		);
		Assert.True(
			stringCount
				< StandardCapabilityCatalog.StringCapabilities.Count
		);

		int booleanOffset =
			12 + namesSize;
		for ( int index = 0; index < booleanMetadata.BinaryIndex; index++ ) {
			Assert.Equal(
				0,
				compiled[booleanOffset + index]
			);
		}
		Assert.Equal(
			1,
			compiled[booleanOffset + booleanMetadata.BinaryIndex]
		);

		int numericOffset =
			booleanOffset + booleanCount;
		if ( ( numericOffset & 1 ) != 0 ) {
			Assert.Equal( 0, compiled[numericOffset] );
			numericOffset++;
		}

		for ( int index = 0; index < numericMetadata.BinaryIndex; index++ ) {
			Assert.Equal(
				-1,
				BinaryPrimitives.ReadInt16LittleEndian(
					compiled.AsSpan(
						numericOffset + ( index * sizeof( short ) ),
						sizeof( short )
					)
				)
			);
		}
		Assert.Equal(
			(short)numericValue,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					numericOffset
						+ ( numericMetadata.BinaryIndex * sizeof( short ) ),
					sizeof( short )
				)
			)
		);

		int stringOffsetTableOffset =
			numericOffset
			+ ( numericCount * sizeof( short ) );
		for ( int index = 0; index < stringMetadata.BinaryIndex; index++ ) {
			Assert.Equal(
				-1,
				BinaryPrimitives.ReadInt16LittleEndian(
					compiled.AsSpan(
						stringOffsetTableOffset
							+ ( index * sizeof( short ) ),
						sizeof( short )
					)
				)
			);
		}
		Assert.Equal(
			0,
			BinaryPrimitives.ReadInt16LittleEndian(
				compiled.AsSpan(
					stringOffsetTableOffset
						+ ( stringMetadata.BinaryIndex * sizeof( short ) ),
					sizeof( short )
				)
			)
		);

		int stringTableOffset =
			stringOffsetTableOffset
			+ ( stringCount * sizeof( short ) );
		Assert.Equal(
			Encoding.Latin1.GetByteCount( stringValue ) + 1,
			stringTableSize
		);
		Assert.Equal(
			stringValue + "\0",
			Encoding.Latin1.GetString(
				compiled.AsSpan(
					stringTableOffset,
					stringTableSize
				)
			)
		);

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse( compiled );
		Assert.True(
			parsed.GetBoolean( booleanMetadata.Capability )
		);
		Assert.Equal(
			(int?)numericValue,
			parsed.GetNumber( numericMetadata.Capability )
		);
		Assert.Equal(
			stringValue,
			parsed.GetString( stringMetadata.Capability )
		);
	}

	[Fact]
	public void LegacyMaximumNumericValueRoundTrips() {
		NumericCapability capability =
			StandardCapabilityCatalog.NumericCapabilities[0].Capability;
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-max-number" )
				.SetDescription( "C02 maximum legacy number" )
				.SetNumber(
					capability,
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
			(int?)short.MaxValue,
			parsed.GetNumber( capability )
		);
	}

	[Theory]
	[InlineData( -3 )]
	[InlineData( -2 )]
	[InlineData( -1 )]
	[InlineData( 32768 )]
	[InlineData( int.MaxValue )]
	public void NumericsOutsideLegacyPresentRangeAreRejected(
		int value
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-invalid-number" )
				.SetDescription( "C02 invalid legacy number" )
				.SetNumber(
					StandardCapabilityCatalog
						.NumericCapabilities[0]
						.Capability,
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
	[InlineData( "embedded\0nul" )]
	[InlineData( "outside-\u0100-latin-one" )]
	public void UnrepresentableStandardStringsAreRejected(
		string value
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-invalid-string" )
				.SetDescription( "C02 invalid string terminal" )
				.SetString(
					StandardCapabilityCatalog
						.StringCapabilities[0]
						.Capability,
					value
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Fact]
	public void SignedStringOffsetOverflowIsRejectedBeforeNarrowing() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-offset-overflow" )
				.SetDescription( "C02 string offset overflow" )
				.SetString(
					StandardCapabilityCatalog
						.StringCapabilities[0]
						.Capability,
					new string(
						'a',
						32768
					)
				)
				.SetString(
					StandardCapabilityCatalog
						.StringCapabilities[1]
						.Capability,
					"second"
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Fact]
	public void UnsignedStringTableSizeOverflowIsRejectedBeforeNarrowing() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c02-table-overflow" )
				.SetDescription( "C02 string table overflow" )
				.SetString(
					StandardCapabilityCatalog
						.StringCapabilities[0]
						.Capability,
					new string(
						'a',
						ushort.MaxValue
					)
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}
}