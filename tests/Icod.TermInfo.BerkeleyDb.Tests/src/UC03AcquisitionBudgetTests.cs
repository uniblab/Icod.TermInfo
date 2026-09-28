/*
	Icod.TermInfo.BerkeleyDb.Tests
	Exercises bounded physical catalog acquisition.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: GPL-3.0-or-later
*/

using System.Buffers.Binary;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class UC03AcquisitionBudgetTests {
	private static readonly string Source = Path.GetFullPath( "uc03.db" );

	[Fact]
	public void CurrentSurfaceAddsExactlyTwoSealedTypes() {
		Type[] types = typeof( BerkeleyDbTerminalCatalogReader ).Assembly.GetExportedTypes();
		Assert.Equal( 14, types.Length );
		Assert.Contains( typeof( BerkeleyDbCatalogLimitException ), types );
		Assert.Contains( typeof( BerkeleyDbTerminalCatalogReadLimits ), types );
		Assert.True( typeof( BerkeleyDbCatalogLimitException ).IsSealed );
		Assert.True( typeof( BerkeleyDbTerminalCatalogReadLimits ).IsSealed );
	}

	[Fact]
	public void LimitsValidateInOrderWithoutCrossBudgetRestrictions() {
		var defaults = new BerkeleyDbTerminalCatalogReadLimits();
		Assert.Equal( 65_536, defaults.MaximumPublicationCount );
		Assert.Equal( 67_108_864L, defaults.MaximumDecodedBytes );
		Assert.Equal( 67_108_864L, defaults.MaximumParsedBytes );
		Assert.Equal( "maximumPublicationCount", Assert.Throws<ArgumentOutOfRangeException>( () => new BerkeleyDbTerminalCatalogReadLimits( 0, 0, 0 ) ).ParamName );
		Assert.Equal( "maximumDecodedBytes", Assert.Throws<ArgumentOutOfRangeException>( () => new BerkeleyDbTerminalCatalogReadLimits( 1, -1, 0 ) ).ParamName );
		Assert.Equal( "maximumParsedBytes", Assert.Throws<ArgumentOutOfRangeException>( () => new BerkeleyDbTerminalCatalogReadLimits( 1, 1, 0 ) ).ParamName );
		var limits = new BerkeleyDbTerminalCatalogReadLimits( int.MaxValue, 1, long.MaxValue );
		Assert.Equal( long.MaxValue, limits.MaximumParsedBytes );
		Assert.All( typeof( BerkeleyDbTerminalCatalogReadLimits ).GetProperties(), property => Assert.Null( property.SetMethod ) );
	}

	[Theory]
	[InlineData( "MaximumDatabaseSize" )]
	[InlineData( "MaximumRecordCount" )]
	[InlineData( "MaximumIndexHops" )]
	[InlineData( "MaximumDecodedBytes" )]
	[InlineData( "MaximumParsedBytes" )]
	[InlineData( "MaximumEntrySize" )]
	[InlineData( "MaximumStoredItemSize" )]
	[InlineData( "MaximumPublicationCount" )]
	public void TypedFailureRetainsValidatedIdentity( string name ) {
		var inner = new IOException( "cause" );
		var error = new BerkeleyDbCatalogLimitException( Source, name, 1, inner );
		Assert.Equal( Source, error.SourcePath );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( 1, error.Limit );
		Assert.Same( inner, error.InnerException );
		Assert.Throws<ArgumentOutOfRangeException>( () => new BerkeleyDbCatalogLimitException( Source, name, -1 ) );
		if ( name == "MaximumIndexHops" ) {
			Assert.Equal( 0, new BerkeleyDbCatalogLimitException( Source, name, 0 ).Limit );
		} else {
			Assert.Throws<ArgumentOutOfRangeException>( () => new BerkeleyDbCatalogLimitException( Source, name, 0 ) );
		}
	}

	[Fact]
	public void TypedFailureRejectsInvalidSourceAndUnknownLimit() {
		Assert.Throws<ArgumentNullException>( () => new BerkeleyDbCatalogLimitException( null!, "MaximumRecordCount", 1 ) );
		Assert.Throws<ArgumentException>( () => new BerkeleyDbCatalogLimitException( " ", "MaximumRecordCount", 1 ) );
		Assert.Throws<ArgumentException>( () => new BerkeleyDbCatalogLimitException( "relative", "MaximumRecordCount", 1 ) );
		Assert.Throws<ArgumentException>( () => new BerkeleyDbCatalogLimitException( Source, "MaximumEntryCount", 1 ) );
	}

	[Fact]
	public void ImageLimitPrecedesAllocation() {
		using var stream = new ObservedStream( new byte[512] );
		AssertLimit( "MaximumDatabaseSize", 511, () => BerkeleyDbHashReader.ReadDatabase( stream, 511, Budget( image: 511 ) ) );
		Assert.Equal( 0, stream.ReadCount );
		Assert.Equal( 512, BerkeleyDbHashReader.ReadStableDatabase( stream, 512, Budget( image: 512 ) ).Length );
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( "before" )]
	[InlineData( "length" )]
	[InlineData( "read" )]
	[InlineData( "verify-length" )]
	[InlineData( "verify-read" )]
	[InlineData( "final-length" )]
	public void CancellationCoversImageAndStabilityReads( string point ) {
		using var cancellation = new CancellationTokenSource();
		using var stream = new ObservedStream( new byte[200_000] ) {
			Observe = stage => {
				if ( stage == point ) {
					cancellation.Cancel();
				}
			},
		};
		if ( point == "before" ) {
			cancellation.Cancel();
		}
		Assert.Throws<OperationCanceledException>( () => BerkeleyDbHashReader.ReadStableDatabase( stream, 200_000, Budget( image: 200_000, token: cancellation.Token ) ) );
		if ( point is "before" or "length" ) {
			Assert.Equal( 0, stream.ReadCount );
		}
		Assert.InRange( stream.MaximumReadSize, 0, 81_920 );
		Assert.True( stream.CanRead );
	}

	[Theory]
	[InlineData( "mutate" )]
	[InlineData( "truncate" )]
	[InlineData( "grow" )]
	public void BoundedAcquisitionRetainsStabilityFailures( string change ) {
		byte[] bytes = new byte[512];
		using var stream = new ObservedStream( bytes );
		stream.Observe = stage => {
			if ( stage == "verify-length" ) {
				if ( change == "mutate" ) {
					bytes[100] = 1;
				} else {
					stream.ReportedLength = change == "grow" ? 513 : 511;
				}
			}
		};
		Assert.Throws<IOException>( () => BerkeleyDbHashReader.ReadStableDatabase( stream, 512, Budget( image: 512 ) ) );
		Assert.True( stream.CanRead );
	}

	[Fact]
	public void OwnedFileIsReleasedOnBudgetFailure() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid() + ".db" );
		try {
			File.WriteAllBytes( path, new byte[512] );
			Assert.Throws<BerkeleyDbCatalogLimitException>( () => BerkeleyDbHashReader.ReadDatabase( path, 511, Budget( image: 511 ) ) );
			using var exclusive = new FileStream( path, FileMode.Open, FileAccess.ReadWrite, FileShare.None );
			Assert.Equal( 512, exclusive.Length );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( true, false )]
	[InlineData( false, true )]
	[InlineData( true, true )]
	public void PhysicalRecordsAndDecodedItemsUseInclusiveBudgets( bool bigEndian, bool overflow ) {
		Hdb07ItemSpec Item( byte[] bytes ) => overflow ? Hdb07ItemSpec.OffPage( bytes, [bytes.Length] ) : Hdb07ItemSpec.Inline( bytes );
		byte[] image = Hdb07HashV9FixtureBuilder.CreateDatabase(
			bigEndian ? Hdb07ByteOrder.BigEndian : Hdb07ByteOrder.LittleEndian, 512,
			new Hdb07RecordSpec( Item( [1, 2] ), Item( [0, 3, 4] ) ),
			new Hdb07RecordSpec( Item( [5, 6] ), Item( [2, 7, 8] ) )
		);
		Assert.Equal( 2, Read( image, Budget( records: 2, decoded: 10 ) ).Count );
		AssertLimit( "MaximumDecodedBytes", 9, () => Read( image, Budget( decoded: 9 ) ) );
		AssertLimit( "MaximumRecordCount", 1, () => Read( image, Budget( records: 1 ) ) );
		AssertLimit( "MaximumStoredItemSize", 2, () => Read( image, Budget( entry: 1 ) ) );
		Assert.Equal( 2, Read( image, Budget( entry: 2 ) ).Count );
	}

	[Fact]
	public void SharedOverflowPayloadIsChargedForEachExtraction() {
		byte[] image = Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.LittleEndian, 512,
			new( Hdb07ItemSpec.Inline( [0x61] ), Hdb07ItemSpec.OffPage( [0x42], [1] ) ),
			new( Hdb07ItemSpec.Inline( [0x62] ), Hdb07ItemSpec.OffPage( [0x42], [1] ) )
		);
		int offset = BinaryPrimitives.ReadUInt16LittleEndian( image.AsSpan( 1024 + 28 ) );
		BinaryPrimitives.WriteUInt32LittleEndian( image.AsSpan( 1024 + offset + 4 ), 3 );
		Assert.Equal( 2, Read( image, Budget( decoded: 4 ) ).Count );
		AssertLimit( "MaximumDecodedBytes", 3, () => Read( image, Budget( decoded: 3 ) ) );
	}

	[Fact]
	public void AggregateArithmeticDoesNotOverflowOrRefund() {
		var budget = Budget( decoded: long.MaxValue, parsed: long.MaxValue );
		budget.ReserveDecoded( long.MaxValue - 1 );
		budget.ReserveDecoded( 1 );
		AssertLimit( "MaximumDecodedBytes", long.MaxValue, () => budget.ReserveDecoded( 1 ) );
		budget.ReserveParsed( long.MaxValue );
		AssertLimit( "MaximumParsedBytes", long.MaxValue, () => budget.ReserveParsed( 1 ) );
	}

	private static BerkeleyDbCatalogReadBudget Budget(
		int image = 67_108_864, int records = 65_536, int entry = 1_048_576,
		long decoded = 67_108_864, long parsed = 67_108_864, CancellationToken token = default
	) => new( Source, new( new( entry ), image, records ), new( maximumDecodedBytes: decoded, maximumParsedBytes: parsed ), token );

	private static IReadOnlyList<BerkeleyDbHashRecord> Read( byte[] image, BerkeleyDbCatalogReadBudget budget ) =>
		BerkeleyDbHashReader.ReadRecords( image, 1_048_577, 65_536, CancellationToken.None, budget );

	private static void AssertLimit( string name, long value, Action action ) {
		var error = Assert.Throws<BerkeleyDbCatalogLimitException>( action );
		Assert.Equal( Source, error.SourcePath );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( value, error.Limit );
	}

	private sealed class ObservedStream( byte[] bytes ) : MemoryStream( bytes ) {
		private bool _verifying;
		private int _verificationLengths;
		internal Action<string>? Observe { get; set; }
		internal long? ReportedLength { get; set; }
		internal int ReadCount { get; private set; }
		internal int MaximumReadSize { get; private set; }
		public override long Length {
			get {
				Observe?.Invoke( _verifying ? (++_verificationLengths == 1 ? "verify-length" : "final-length") : "length" );
				return ReportedLength ?? base.Length;
			}
		}
		public override long Position {
			get => base.Position;
			set {
				_verifying = true;
				base.Position = value;
			}
		}
		public override int Read( Span<byte> buffer ) {
			ObserveRead( buffer.Length );
			return base.Read( buffer );
		}
		public override int Read( byte[] buffer, int offset, int count ) {
			ObserveRead( count );
			return base.Read( buffer, offset, count );
		}
		private void ObserveRead( int count ) {
			ReadCount++;
			MaximumReadSize = Math.Max( MaximumReadSize, count );
			Observe?.Invoke( _verifying ? "verify-read" : "read" );
		}
	}
}
