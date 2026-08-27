using System.Buffers.Binary;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C04WriterTests {
	[Fact]
	public void AutomaticPrefersLegacyWhenAllPresentNumericsFit() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-auto-legacy" )
				.SetDescription( "C04 automatic legacy terminal" )
				.SetNumber( NumericCapability.Columns, short.MaxValue )
				.SetExtendedNumber( "XNum", short.MaxValue )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);

		Assert.Equal( 0x011A, ReadUInt16( compiled, 0 ) );
	}

	[Fact]
	public void AutomaticSelectsWideForStandardNumericBeyondLegacyRange() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-auto-standard-wide" )
				.SetDescription( "C04 automatic standard wide terminal" )
				.SetNumber( NumericCapability.Columns, short.MaxValue + 1 )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		TerminalDescription parsed =
			CompiledTermInfoParser.Parse( compiled );

		Assert.Equal( 0x021E, ReadUInt16( compiled, 0 ) );
		Assert.Equal(
			(int?)( short.MaxValue + 1 ),
			parsed.GetNumber( NumericCapability.Columns )
		);
	}

	[Fact]
	public void AutomaticSelectsWideForExtendedNumericBeyondLegacyRange() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-auto-extended-wide" )
				.SetDescription( "C04 automatic extended wide terminal" )
				.SetExtendedNumber( "XNum", short.MaxValue + 1 )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		TerminalDescription parsed =
			CompiledTermInfoParser.Parse( compiled );

		Assert.Equal( 0x021E, ReadUInt16( compiled, 0 ) );
		Assert.Equal(
			short.MaxValue + 1,
			parsed.ExtendedCapabilities["XNum"].NumberValue
		);
	}

	[Fact]
	public void AutomaticWideRoundTripsMaximumStandardAndExtendedNumerics() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-max-wide" )
				.SetDescription( "C04 maximum wide terminal" )
				.SetNumber( NumericCapability.Columns, int.MaxValue )
				.SetExtendedNumber( "XNum", int.MaxValue )
				.Build();

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					description
				)
			);

		Assert.Equal(
			(int?)int.MaxValue,
			parsed.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			int.MaxValue,
			parsed.ExtendedCapabilities["XNum"].NumberValue
		);
	}

	[Fact]
	public void ExplicitLegacyFailsRatherThanUpgrading() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-explicit-legacy" )
				.SetDescription( "C04 explicit legacy terminal" )
				.SetNumber( NumericCapability.Columns, 100000 )
				.SetExtendedNumber( "XNum", 200000 )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Legacy
			);

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write(
				description,
				options
			)
		);
	}

	[Fact]
	public void ExplicitWideStaysWideWhenLegacyWouldSuffice() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-explicit-wide" )
				.SetDescription( "C04 explicit wide terminal" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Wide
			);

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description,
				options
			);

		Assert.Equal( 0x021E, ReadUInt16( compiled, 0 ) );
		Assert.Equal(
			(int?)80,
			CompiledTermInfoParser.Parse( compiled )
				.GetNumber( NumericCapability.Columns )
		);
	}

	[Fact]
	public void WideUsesFourByteStandardAndExtendedNumericTables() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-wide-layout" )
				.SetDescription( "C04 wide layout terminal" )
				.SetNumber( NumericCapability.Columns, 100000 )
				.SetExtendedNumber( "XWide", 200000 )
				.Build();
		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description,
				new CompiledTermInfoWriterOptions(
					CompiledTermInfoFormat.Wide
				)
			);

		Assert.Equal( 0x021E, ReadUInt16( compiled, 0 ) );

		int numericOffset =
			GetConventionalNumericOffset( compiled );
		Assert.Equal(
			100000,
			BinaryPrimitives.ReadInt32LittleEndian(
				compiled.AsSpan(
					numericOffset,
					sizeof( int )
				)
			)
		);

		int conventionalEnd =
			GetConventionalEnd(
				compiled,
				sizeof( int )
			);
		int extendedHeaderOffset =
			( ( conventionalEnd & 1 ) == 0 )
				? conventionalEnd
				: conventionalEnd + 1
		;
		Assert.Equal( 0, ReadUInt16( compiled, extendedHeaderOffset ) );
		Assert.Equal( 1, ReadUInt16( compiled, extendedHeaderOffset + 2 ) );
		Assert.Equal( 0, ReadUInt16( compiled, extendedHeaderOffset + 4 ) );

		int extendedNumericOffset =
			extendedHeaderOffset + 10;
		Assert.Equal(
			200000,
			BinaryPrimitives.ReadInt32LittleEndian(
				compiled.AsSpan(
					extendedNumericOffset,
					sizeof( int )
				)
			)
		);
	}

	[Theory]
	[InlineData( -3 )]
	[InlineData( -2 )]
	[InlineData( -1 )]
	public void NegativePresentNumericsRemainUnrepresentableInWideFormat(
		int value
	) {
		TerminalDescription standard =
			new TerminalDescriptionBuilder( "c04-negative-standard" )
				.SetDescription( "C04 negative standard terminal" )
				.SetNumber( NumericCapability.Columns, value )
				.Build();
		TerminalDescription extended =
			new TerminalDescriptionBuilder( "c04-negative-extended" )
				.SetDescription( "C04 negative extended terminal" )
				.SetExtendedNumber( "XNum", value )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Wide
			);

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write(
				standard,
				options
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write(
				extended,
				options
			)
		);
	}

	[Fact]
	public void ExcludingExtendedCapabilitiesFailsRatherThanDroppingThem() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-no-extended" )
				.SetDescription( "C04 excluded extended terminal" )
				.SetExtendedBoolean( "XBool" )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Automatic,
				includeExtendedCapabilities: false
			);

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write(
				description,
				options
			)
		);
	}

	[Fact]
	public void ExcludingExtendedCapabilitiesAllowsStandardOnlyEntry() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-standard-only" )
				.SetDescription( "C04 standard-only terminal" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Automatic,
				includeExtendedCapabilities: false
			);

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description,
				options
			);
		TerminalDescription parsed =
			CompiledTermInfoParser.Parse( compiled );

		Assert.Equal( 0x011A, ReadUInt16( compiled, 0 ) );
		Assert.Empty( parsed.ExtendedCapabilities );
		Assert.Equal(
			(int?)80,
			parsed.GetNumber( NumericCapability.Columns )
		);
	}

	[Fact]
	public void WriterOptionsSnapshotRequestedPolicy() {
		CompiledTermInfoWriterOptions defaults =
			new();
		CompiledTermInfoWriterOptions explicitPolicy =
			new(
				CompiledTermInfoFormat.Wide,
				includeExtendedCapabilities: false
			);

		Assert.Equal( CompiledTermInfoFormat.Automatic, defaults.Format );
		Assert.True( defaults.IncludeExtendedCapabilities );
		Assert.Equal( CompiledTermInfoFormat.Wide, explicitPolicy.Format );
		Assert.False( explicitPolicy.IncludeExtendedCapabilities );
	}

	[Fact]
	public void InvalidFormatValueIsRejectedAsAnArgument() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new CompiledTermInfoWriterOptions(
				(CompiledTermInfoFormat)12345
			)
		);
	}

	[Fact]
	public void NullOptionsAreRejectedAsAnArgument() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-null-options" )
				.SetDescription( "C04 null options terminal" )
				.Build();

		Assert.Throws<ArgumentNullException>(
			() => CompiledTermInfoWriter.Write(
				description,
				null!
			)
		);
	}

	[Fact]
	public void SameDescriptionAndOptionsProduceIdenticalBytes() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c04-deterministic" )
				.SetDescription( "C04 deterministic terminal" )
				.SetNumber( NumericCapability.Columns, 100000 )
				.SetExtendedNumber( "XNum", 200000 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		CompiledTermInfoWriterOptions options =
			new(
				CompiledTermInfoFormat.Wide
			);

		Assert.Equal(
			CompiledTermInfoWriter.Write(
				description,
				options
			),
			CompiledTermInfoWriter.Write(
				description,
				options
			)
		);
	}

	private static int GetConventionalNumericOffset(
		byte[] compiled
	) {
		ArgumentNullException.ThrowIfNull( compiled );

		int namesSize = ReadUInt16( compiled, 2 );
		int booleanCount = ReadUInt16( compiled, 4 );
		int numericOffset =
			12
			+ namesSize
			+ booleanCount;
		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}

		return numericOffset;
	}

	private static int GetConventionalEnd(
		byte[] compiled,
		int numericWidth
	) {
		ArgumentNullException.ThrowIfNull( compiled );

		int numericCount = ReadUInt16( compiled, 6 );
		int stringCount = ReadUInt16( compiled, 8 );
		int stringTableSize = ReadUInt16( compiled, 10 );
		return GetConventionalNumericOffset( compiled )
			+ ( numericCount * numericWidth )
			+ ( stringCount * sizeof( short ) )
			+ stringTableSize;
	}

	private static ushort ReadUInt16(
		byte[] compiled,
		int offset
	) {
		ArgumentNullException.ThrowIfNull( compiled );

		return BinaryPrimitives.ReadUInt16LittleEndian(
			compiled.AsSpan(
				offset,
				sizeof( ushort )
			)
		);
	}
}