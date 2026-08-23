using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T35ParserHardeningTests {
	private const ushort LegacyMagic = 0x011A;
	private const ushort ExtendedNumberMagic = 0x021E;
	private const int HeaderSize = 12;
	private const int ExtendedHeaderSize = 10;

	[Fact]
	public void AssemblyIdentifiesT35DevelopmentVersion() {
		Assembly assembly = typeof( CompiledTermInfoParser ).Assembly;
		Version? assemblyVersion = assembly.GetName().Version;
		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;

		Assert.NotNull( assemblyVersion );
		Assert.Equal( new Version( 0, 9, 0, 0 ), assemblyVersion );
		Assert.NotNull( informationalVersion );
		Assert.True(
			informationalVersion!.StartsWith(
				"0.9.0-alpha.4",
				StringComparison.Ordinal ),
			$"Unexpected informational version '{informationalVersion}'." );
	}

	[Theory]
	[InlineData( "compiled/t29-legacy-minimal.bin" )]
	[InlineData( "compiled/t29-legacy-alignment.bin" )]
	[InlineData( "compiled/t29-legacy-edge.bin" )]
	[InlineData( "compiled/t29-extended.bin" )]
	[InlineData( "compiled/t29-extended32.bin" )]
	public void EveryTruncatedPrefixHasDeterministicParserOutcome(
		string relativePath ) {
		byte[] entry =
			ReadFixture( relativePath );
		int conventionalEnd =
			GetConventionalEnd( entry );

		for ( int length = 0;
			length < entry.Length;
			length++ ) {
			byte[] prefix =
				entry[ ..length ];

			if ( length == conventionalEnd ) {
				TerminalDescription terminal =
					CompiledTermInfoParser.Parse( prefix );

				Assert.False(
					string.IsNullOrWhiteSpace(
						terminal.Name ) );
				continue;
			}

			Exception? exception =
				Record.Exception(
					() => CompiledTermInfoParser.Parse( prefix ) );

			Assert.NotNull( exception );
			Assert.IsType<CompiledTermInfoFormatException>(
				exception );
		}
	}

	[Theory]
	[InlineData( 4, "booleans" )]
	[InlineData( 6, "numerics" )]
	[InlineData( 8, "string-offsets" )]
	public void ImpossibleStandardCountsFailBeforeSectionWalking(
		int headerFieldOffset,
		string expectedSection ) {
		byte[] entry =
			CreateHeaderOnlyEntry(
				LegacyMagic );

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerFieldOffset,
				sizeof( ushort ) ),
			ushort.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			expectedSection,
			exception.Section );
		Assert.Equal(
			headerFieldOffset,
			exception.Offset );
	}

	[Fact]
	public void MaximumDeclaredNamesSizeFailsBeforeDecodingAbsentBytes() {
		byte[] entry =
			CreateHeaderOnlyEntry(
				LegacyMagic );

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				2,
				sizeof( ushort ) ),
			ushort.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"names",
			exception.Section );
		Assert.Equal(
			entry.Length,
			exception.Offset );
	}

	[Fact]
	public void MaximumDeclaredStringTableSizeFailsBeforeStringExtraction() {
		byte[] entry =
			CreateMinimalConventionalEntry();

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				10,
				sizeof( ushort ) ),
			ushort.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"string-table",
			exception.Section );
		Assert.Equal(
			entry.Length,
			exception.Offset );
	}

	[Fact]
	public void ImpossibleExtendedNamePopulationFailsBeforeBodyWalking() {
		byte[] entry =
			AddExtendedHeader(
				CreateMinimalConventionalEntry(),
				booleanCount: 2,
				numericCount: 0,
				stringCount: 0,
				stringTableItemCount: 2,
				stringTableSize: 1 );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"extended-header",
			exception.Section );
		Assert.Equal(
			-1,
			exception.Offset );
	}

	[Fact]
	public void MaximumExtendedCountsRemainInsideCompiledFormatExceptionContract() {
		byte[] entry =
			AddExtendedHeader(
				CreateMinimalConventionalEntry(),
				booleanCount: ushort.MaxValue,
				numericCount: ushort.MaxValue,
				stringCount: ushort.MaxValue,
				stringTableItemCount: ushort.MaxValue,
				stringTableSize: ushort.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"extended-header",
			exception.Section );
	}

	[Fact]
	public void StandardStringOffsetOutsideTableHasStableOffsetDiagnostic() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin" );
		int offsetTable =
			GetStringOffsetTableOffset( entry );

		BinaryPrimitives.WriteInt16LittleEndian(
			entry.AsSpan(
				offsetTable,
				sizeof( short ) ),
			short.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"string-offsets",
			exception.Section );
		Assert.Equal(
			offsetTable,
			exception.Offset );
	}

	[Fact]
	public void ExtendedStringOffsetOutsideTableHasStableOffsetDiagnostic() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-extended.bin" );
		ExtendedOffsets offsets =
			GetExtendedOffsets( entry );

		BinaryPrimitives.WriteInt16LittleEndian(
			entry.AsSpan(
				offsets.StringOffsetTable,
				sizeof( short ) ),
			short.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"extended-string-offsets",
			exception.Section );
		Assert.Equal(
			offsets.StringOffsetTable,
			exception.Offset );
	}

	[Fact]
	public void ExtendedNameOffsetOutsideTableHasStableOffsetDiagnostic() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-extended.bin" );
		ExtendedOffsets offsets =
			GetExtendedOffsets( entry );

		BinaryPrimitives.WriteInt16LittleEndian(
			entry.AsSpan(
				offsets.NameOffsetTable,
				sizeof( short ) ),
			short.MaxValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"extended-name-offsets",
			exception.Section );
		Assert.Equal(
			offsets.NameOffsetTable,
			exception.Offset );
	}

	[Fact]
	public void ThirtyTwoBitInvalidNegativeStandardNumericIsRejected() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-extended32.bin" );
		int numericOffset =
			GetNumericOffset( entry );

		BinaryPrimitives.WriteInt32LittleEndian(
			entry.AsSpan(
				numericOffset,
				sizeof( int ) ),
			int.MinValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"numerics",
			exception.Section );
		Assert.Equal(
			numericOffset,
			exception.Offset );
	}

	[Fact]
	public void ThirtyTwoBitInvalidNegativeExtendedNumericIsRejected() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-extended32.bin" );
		ExtendedOffsets offsets =
			GetExtendedOffsets( entry );

		BinaryPrimitives.WriteInt32LittleEndian(
			entry.AsSpan(
				offsets.NumericTable,
				sizeof( int ) ),
			int.MinValue );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			"extended-numerics",
			exception.Section );
		Assert.Equal(
			offsets.NumericTable,
			exception.Offset );
	}

	[Fact]
	public void DiagnosticsAreStableAcrossRepeatedMalformedParses() {
		byte[] entry =
			ReadFixture(
				"malformed/illegal-extended-string-offset.bin" );

		CompiledTermInfoFormatException first =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );
		CompiledTermInfoFormatException second =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse( entry ) );

		Assert.Equal(
			first.Section,
			second.Section );
		Assert.Equal(
			first.Offset,
			second.Offset );
		Assert.Equal(
			first.Message,
			second.Message );
	}

	[Fact]
	public void FailedParseDoesNotMutateInputOrContaminateLaterSuccess() {
		byte[] malformed =
			ReadFixture(
				"malformed/extended-standard-name-collision.bin" );
		byte[] snapshot =
			(byte[])malformed.Clone();

		Assert.Throws<CompiledTermInfoFormatException>(
			() => CompiledTermInfoParser.Parse( malformed ) );
		Assert.Equal(
			snapshot,
			malformed );

		TerminalDescription terminal =
			ParseFixture(
				"compiled/t29-extended32.bin" );

		Assert.Equal(
			"t29-extended32",
			terminal.Name );
		Assert.Equal<int?>(
			16_777_216,
			terminal.GetNumber(
				NumericCapability.Colors ) );
		Assert.True(
			terminal.TryGetExtendedNumber(
				"XNum",
				out int value ) );
		Assert.Equal(
			2_147_483_640,
			value );
	}

	[Fact]
	public void DeterministicRandomBytesNeverEscapeParserExceptionBoundary() {
		Random random =
			new( 0x0035_0900 );

		for ( int iteration = 0;
			iteration < 512;
			iteration++ ) {
			byte[] entry =
				new byte[
					random.Next(
						0,
						1_025 ) ];
			random.NextBytes( entry );

			if ( entry.Length >= sizeof( ushort )
				&& ( iteration & 1 ) == 0 ) {
				ushort magic =
					( ( iteration & 2 ) == 0 )
						? LegacyMagic
						: ExtendedNumberMagic
				;
				BinaryPrimitives.WriteUInt16LittleEndian(
					entry.AsSpan(
						0,
						sizeof( ushort ) ),
					magic );
			}

			Exception? exception =
				Record.Exception(
					() => CompiledTermInfoParser.Parse( entry ) );

			if ( exception is not null ) {
				Assert.IsType<CompiledTermInfoFormatException>(
					exception );
			}
		}
	}

	[Theory]
	[InlineData( "compiled/t29-legacy-minimal.bin" )]
	[InlineData( "compiled/t29-legacy-alignment.bin" )]
	[InlineData( "compiled/t29-legacy-edge.bin" )]
	[InlineData( "compiled/t29-extended.bin" )]
	[InlineData( "compiled/t29-extended32.bin" )]
	public void DeterministicMutationsNeverEscapeParserExceptionBoundary(
		string relativePath ) {
		byte[] seed =
			ReadFixture( relativePath );
		Random random =
			new(
				StringComparer.Ordinal.GetHashCode(
					relativePath )
				^ 0x0035_0009 );

		for ( int iteration = 0;
			iteration < 128;
			iteration++ ) {
			byte[] entry =
				(byte[])seed.Clone();
			int editCount =
				random.Next(
					1,
					5 );

			for ( int edit = 0;
				edit < editCount;
				edit++ ) {
				int offset =
					random.Next(
						entry.Length );
				int bit =
					random.Next(
						0,
						8 );
				entry[ offset ] ^=
					(byte)( 1 << bit );
			}

			Exception? exception =
				Record.Exception(
					() => CompiledTermInfoParser.Parse( entry ) );

			if ( exception is not null ) {
				Assert.IsType<CompiledTermInfoFormatException>(
					exception );
			}
		}
	}

	[Fact]
	public void ParserDeclaresNoNativeEntryPoints() {
		MethodInfo[] methods =
			typeof( CompiledTermInfoParser )
				.GetMethods(
					BindingFlags.Public
					| BindingFlags.NonPublic
					| BindingFlags.Static
					| BindingFlags.DeclaredOnly );

		Assert.NotEmpty( methods );

		Assert.All(
			methods,
			method => {
				Assert.Null(
					method.GetCustomAttribute<DllImportAttribute>() );
				Assert.True(
					( method.Attributes
						& MethodAttributes.PinvokeImpl )
					== 0,
					$"Parser method '{method.Name}' is marked as a native P/Invoke." );
			} );
	}

	[Fact]
	public void ConfiguredEntryLimitIsCheckedBeforeFormatWalking() {
		byte[] entry =
			ReadFixture(
				"compiled/t29-extended32.bin" );
		CompiledTermInfoParserOptions options =
			new( entry.Length - 1 );

		CompiledTermInfoFormatException exception =
			Assert.Throws<CompiledTermInfoFormatException>(
				() => CompiledTermInfoParser.Parse(
					entry,
					options ) );

		Assert.Equal(
			"entry",
			exception.Section );
		Assert.Equal(
			-1,
			exception.Offset );
	}

	private static TerminalDescription ParseFixture(
		string relativePath ) {
		return CompiledTermInfoParser.Parse(
			ReadFixture( relativePath ) );
	}

	private static byte[] ReadFixture(
		string relativePath ) {
		return File.ReadAllBytes(
			Path.Combine(
				AppContext.BaseDirectory,
				"fixtures",
				"compiled-terminfo",
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar ) ) );
	}

	private static byte[] CreateHeaderOnlyEntry(
		ushort magic ) {
		byte[] entry =
			new byte[ HeaderSize ];

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				0,
				sizeof( ushort ) ),
			magic );
		return entry;
	}

	private static byte[] CreateMinimalConventionalEntry() {
		byte[] entry =
			new byte[ HeaderSize + 4 ];

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				0,
				sizeof( ushort ) ),
			LegacyMagic );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				2,
				sizeof( ushort ) ),
			4 );
		"n|d\0"u8.CopyTo(
			entry.AsSpan(
				HeaderSize ) );

		return entry;
	}

	private static byte[] AddExtendedHeader(
		byte[] conventional,
		ushort booleanCount,
		ushort numericCount,
		ushort stringCount,
		ushort stringTableItemCount,
		ushort stringTableSize ) {
		ArgumentNullException.ThrowIfNull( conventional );

		int headerOffset =
			( ( conventional.Length & 1 ) == 0 )
				? conventional.Length
				: conventional.Length + 1
		;
		byte[] entry =
			new byte[
				headerOffset
				+ ExtendedHeaderSize ];

		conventional.CopyTo(
			entry,
			0 );

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerOffset,
				sizeof( ushort ) ),
			booleanCount );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerOffset + 2,
				sizeof( ushort ) ),
			numericCount );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerOffset + 4,
				sizeof( ushort ) ),
			stringCount );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerOffset + 6,
				sizeof( ushort ) ),
			stringTableItemCount );
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				headerOffset + 8,
				sizeof( ushort ) ),
			stringTableSize );

		return entry;
	}

	private static int GetConventionalEnd(
		ReadOnlySpan<byte> entry ) {
		ushort magic =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 0..2 ] );
		int names =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 2..4 ] );
		int booleans =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 4..6 ] );
		int numbers =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 6..8 ] );
		int strings =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 8..10 ] );
		int table =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 10..12 ] );

		int numericOffset =
			HeaderSize
			+ names
			+ booleans;
		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}

		int numericWidth =
			( magic == ExtendedNumberMagic )
				? sizeof( int )
				: sizeof( short )
		;

		return numericOffset
			+ ( numbers * numericWidth )
			+ ( strings * sizeof( short ) )
			+ table;
	}

	private static int GetNumericOffset(
		ReadOnlySpan<byte> entry ) {
		int names =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 2..4 ] );
		int booleans =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 4..6 ] );

		int offset =
			HeaderSize
			+ names
			+ booleans;

		return ( ( offset & 1 ) == 0 )
			? offset
			: offset + 1
		;
	}

	private static int GetStringOffsetTableOffset(
		ReadOnlySpan<byte> entry ) {
		ushort magic =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 0..2 ] );
		int numbers =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 6..8 ] );
		int numericWidth =
			( magic == ExtendedNumberMagic )
				? sizeof( int )
				: sizeof( short )
		;

		return GetNumericOffset( entry )
			+ ( numbers * numericWidth );
	}

	private static ExtendedOffsets GetExtendedOffsets(
		ReadOnlySpan<byte> entry ) {
		ushort magic =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry[ 0..2 ] );
		int numericWidth =
			( magic == ExtendedNumberMagic )
				? sizeof( int )
				: sizeof( short )
		;
		int headerOffset =
			GetConventionalEnd( entry );

		if ( ( headerOffset & 1 ) != 0 ) {
			headerOffset++;
		}

		int booleans =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.Slice(
					headerOffset,
					sizeof( ushort ) ) );
		int numbers =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.Slice(
					headerOffset + 2,
					sizeof( ushort ) ) );
		int strings =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.Slice(
					headerOffset + 4,
					sizeof( ushort ) ) );

		int numericOffset =
			headerOffset
			+ ExtendedHeaderSize
			+ booleans;
		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}

		int stringOffsetTable =
			numericOffset
			+ ( numbers * numericWidth );
		int nameOffsetTable =
			stringOffsetTable
			+ ( strings * sizeof( short ) );

		return new ExtendedOffsets(
			numericOffset,
			stringOffsetTable,
			nameOffsetTable );
	}

	private readonly record struct ExtendedOffsets(
		int NumericTable,
		int StringOffsetTable,
		int NameOffsetTable );
}