using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C01WriterTests {
	[Fact]
	public void MinimalDescriptionWritesLegacyHeaderAndRoundTrips() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c01-minimal" )
				.SetDescription( "C01 minimal terminal" )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);

		Assert.Equal(
			0x011A,
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 0, 2 )
			)
		);
		Assert.Equal(
			0,
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 4, 2 )
			)
		);
		Assert.Equal(
			0,
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 6, 2 )
			)
		);
		Assert.Equal(
			0,
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 8, 2 )
			)
		);
		Assert.Equal(
			0,
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 10, 2 )
			)
		);
		ushort namesSize =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 2, 2 )
			);
		Assert.Equal(
			"c01-minimal|C01 minimal terminal\0",
			Encoding.Latin1.GetString(
				compiled.AsSpan(
					12,
					namesSize
				)
			)
		);

		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				compiled
			);
		Assert.Equal( description.Name, parsed.Name );
		Assert.Equal( description.Description, parsed.Description );
		Assert.Empty( parsed.BooleanCapabilities );
		Assert.Empty( parsed.NumericCapabilities );
		Assert.Empty( parsed.StringCapabilities );
		Assert.Empty( parsed.ExtendedCapabilities );
	}

	[Fact]
	public void AliasOrderAndHighLatinOneBytesRoundTrip() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c01-latin" )
				.AddAlias( "latin-alias" )
				.AddAlias( "latin-second" )
				.SetDescription( "Caf\u00e9 terminal" )
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
	}

	[Fact]
	public void OddNamesSectionReceivesZeroAlignmentByte() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "ab" )
				.SetDescription( "d" )
				.Build();

		byte[] compiled =
			CompiledTermInfoWriter.Write(
				description
			);
		ushort namesSize =
			BinaryPrimitives.ReadUInt16LittleEndian(
				compiled.AsSpan( 2, 2 )
			);

		Assert.Equal( 5, namesSize );
		Assert.Equal( 18, compiled.Length );
		Assert.Equal( 0, compiled[^1] );
		Assert.Equal(
			description.Name,
			CompiledTermInfoParser.Parse( compiled ).Name
		);
	}

	[Fact]
	public void RepeatedWritesAreByteForByteDeterministic() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "deterministic" )
				.AddAlias( "det" )
				.SetDescription( "Deterministic terminal" )
				.Build();

		byte[] first =
			CompiledTermInfoWriter.Write(
				description
			);
		byte[] second =
			CompiledTermInfoWriter.Write(
				description
			);

		Assert.Equal( first, second );
	}

	[Fact]
	public void MissingDescriptionIsRejectedRatherThanSynthesized() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "missing-description" )
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

	[Theory]
	[InlineData( "embedded\0nul" )]
	[InlineData( "contains|separator" )]
	[InlineData( "outside-\u0100-latin-one" )]
	public void UnrepresentableDescriptionsAreRejected(
		string text
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "unrepresentable" )
				.SetDescription( text )
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => CompiledTermInfoWriter.Write( description )
		);
	}

}