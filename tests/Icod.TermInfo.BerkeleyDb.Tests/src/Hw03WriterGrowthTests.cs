/*
	Icod.TermInfo.BerkeleyDb.Tests
	Defines the HW03 Hash-v9 bounded-growth and collision-chain contract.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Globalization;
using System.Text;
using Xunit;
using static Icod.TermInfo.BerkeleyDb.Tests.Hw03WriterTestSupport;

namespace Icod.TermInfo.BerkeleyDb.Tests;

[Collection( Hdb07CultureCollection.Name )]
public sealed class Hw03WriterGrowthTests {
	private static readonly ( string Key, uint Hash )[] GrowthKeys = [
		( "hw03-growth-003", 0x2B73EB10U ),
		( "hw03-growth-007", 0x2B73EB14U ),
		( "hw03-growth-001", 0x2B73EB12U ),
		( "hw03-growth-005", 0x2B73EB16U ),
	];

	private static readonly ( string Key, uint Hash )[] LongChainKeys = [
		( "hw03-chain-003", 0xDB6BFDF4U ),
		( "hw03-chain-007", 0xDB6BFDF0U ),
		( "hw03-chain-010", 0xDA6BFC04U ),
		( "hw03-chain-014", 0xDA6BFC00U ),
		( "hw03-chain-018", 0xDA6BFC0CU ),
		( "hw03-chain-021", 0xDD6C00DCU ),
		( "hw03-chain-025", 0xDD6C00D8U ),
		( "hw03-chain-029", 0xDD6C00D4U ),
		( "hw03-chain-032", 0xDC6BFF68U ),
		( "hw03-chain-036", 0xDC6BFF6CU ),
	];

	[Fact]
	public void ReducibleCollisionSelectsFourPrimaryBuckets() {
		AssertQualifiedHashes( GrowthKeys );

		byte[] image = BuildGrowthRecords(
			maximumDatabaseSize: 64 * 1024 * 1024
		);
		Assert.Equal( 5 * PageSize, image.Length );
		Assert.Equal( 3U, ReadUInt32( image, 72 ) );
		Assert.Equal( 3U, ReadUInt32( image, 76 ) );
		Assert.Equal( 1U, ReadUInt32( image, 80 ) );
		Assert.Equal( 1U, ReadUInt32( image, 96 ) );
		Assert.Equal( 1U, ReadUInt32( image, 100 ) );
		Assert.Equal( 1U, ReadUInt32( image, 104 ) );
		Assert.All(
			Enumerable.Range( 1, 4 ),
			pageNumber => Assert.Equal(
				(uint)pageNumber,
				ReadPageNumber( image, pageNumber )
			)
		);

		Assert.Equal( (byte)13, ReadPageType( image, 1 ) );
		Assert.Equal( (ushort)4, ReadPageItemCount( image, 1 ) );
		Assert.Equal( (byte)13, ReadPageType( image, 2 ) );
		Assert.Equal( (ushort)0, ReadPageItemCount( image, 2 ) );
		Assert.Equal( (byte)13, ReadPageType( image, 3 ) );
		Assert.Equal( (ushort)4, ReadPageItemCount( image, 3 ) );
		Assert.Equal( (byte)13, ReadPageType( image, 4 ) );
		Assert.Equal( (ushort)0, ReadPageItemCount( image, 4 ) );
	}

	[Fact]
	public void ExactHashCollisionUsesLinkedContinuationPage() {
		Assert.Equal(
			0xC071CA1DU,
			BerkeleyDbHashV9ImageBuilder.Hash(
				Encoding.ASCII.GetBytes( "hw03-0c5ny4k-da6" )
			)
		);
		Assert.Equal(
			0xC071CA1DU,
			BerkeleyDbHashV9ImageBuilder.Hash(
				Encoding.ASCII.GetBytes( "hw03-0fpxptj-1j8p" )
			)
		);
		BerkeleyDbHashRecord[] records = ExactCollisionRecords();
		Assert.Equal(
			0x6DBEE87DU,
			BerkeleyDbHashV9ImageBuilder.Hash( records[0].Key.Span )
		);
		Assert.Equal(
			0x6DBEE87DU,
			BerkeleyDbHashV9ImageBuilder.Hash( records[1].Key.Span )
		);

		byte[] image = Build( records );
		int bucketPage = checked( (int)( 0x6DBEE87DU & 1U ) + 1 );
		const int continuationPage = 3;
		Assert.Equal(
			(uint)continuationPage,
			ReadPageNext( image, bucketPage )
		);
		Assert.Equal(
			(uint)bucketPage,
			ReadPagePrevious( image, continuationPage )
		);
		Assert.Equal( 0U, ReadPageNext( image, continuationPage ) );

		foreach ( BerkeleyDbHashRecord record in records ) {
			Assert.True(
				BerkeleyDbHashReader.TryReadValue(
					image,
					record.Key.Span,
					out byte[] actual,
					maximumItemSize: 2048
				)
			);
			Assert.Equal( record.Value.ToArray(), actual );
		}
	}

	[Fact]
	public void GrowthCapUsesTwoBucketsAndOneContinuationPage() {
		byte[] image = BuildGrowthRecords(
			maximumDatabaseSize: 4 * PageSize
		);
		Assert.Equal( 4 * PageSize, image.Length );
		Assert.Equal( 1U, ReadUInt32( image, 72 ) );
		Assert.Equal( 1U, ReadUInt32( image, 76 ) );
		Assert.Equal( 0U, ReadUInt32( image, 80 ) );
		Assert.Equal( 3U, ReadPageNext( image, 1 ) );
	}

	[Fact]
	public void ExactPageLimitSucceedsAndOneByteBelowFailsBeforeAllocation() {
		Assert.Equal(
			4 * PageSize,
			BuildGrowthRecords( 4 * PageSize ).Length
		);
		Assert.Throws<InvalidOperationException>(
			() => BuildGrowthRecords( ( 4 * PageSize ) - 1 )
		);
	}

	[Fact]
	public void CappedGrowthBuildsMultipleContinuationPages() {
		AssertQualifiedHashes( LongChainKeys );

		byte[] image = Build( LongChainRecords(), 8 * PageSize );
		Assert.Equal( 8 * PageSize, image.Length );
		Assert.Equal( 3U, ReadUInt32( image, 72 ) );
		Assert.Equal( 5U, ReadPageNext( image, 1 ) );
		Assert.Equal( 6U, ReadPageNext( image, 5 ) );
		Assert.Equal( 7U, ReadPageNext( image, 6 ) );
		Assert.Equal( 0U, ReadPageNext( image, 7 ) );
	}

	[Fact]
	public void ContinuationPagesPrecedeEveryPayloadOverflowPage() {
		BerkeleyDbHashRecord[] records = LongChainRecords()
			.Append( Record( "hw03-overflow-order", 1025, 0x6A ) )
			.ToArray()
		;
		records = Sort( records );
		byte[] image = Build( records, 9 * PageSize );
		Assert.Equal( (byte)13, ReadPageType( image, 5 ) );
		Assert.Equal( (byte)13, ReadPageType( image, 6 ) );
		Assert.Equal( (byte)13, ReadPageType( image, 7 ) );
		Assert.Equal( (byte)7, ReadPageType( image, 8 ) );
	}

	[Fact]
	public void RepeatedAndReversedInputsProduceEqualBytes() {
		BerkeleyDbHashRecord[] forward = GrowthRecords();
		BerkeleyDbHashRecord[] reversed = Sort(
			forward.Reverse().ToArray()
		);
		Assert.Equal( Build( forward ), Build( forward ) );
		Assert.Equal( Build( forward ), Build( reversed ) );
	}

	[Fact]
	public void ContrastingCulturesProduceEqualBytes() {
		CultureInfo original = CultureInfo.CurrentCulture;
		CultureInfo originalUi = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = new CultureInfo( "en-US" );
			CultureInfo.CurrentUICulture = new CultureInfo( "en-US" );
			byte[] expected = Build( GrowthRecords() );
			CultureInfo.CurrentCulture = new CultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = new CultureInfo( "tr-TR" );
			Assert.Equal( expected, Build( GrowthRecords() ) );
		} finally {
			CultureInfo.CurrentCulture = original;
			CultureInfo.CurrentUICulture = originalUi;
		}
	}

	[Fact]
	public void LargerCeilingDoesNotChangeNaturalImage() {
		BerkeleyDbHashRecord[] records = GrowthRecords();
		Assert.Equal(
			Build( records, 5 * PageSize ),
			Build( records, 64 * 1024 * 1024 )
		);
	}

	[Fact]
	public void SameRecordsHaveSameFileIdAcrossNaturalAndCappedLayouts() {
		byte[] natural = Build( GrowthRecords(), 5 * PageSize );
		byte[] capped = Build( GrowthRecords(), 4 * PageSize );
		Assert.Equal(
			natural.AsSpan( 52, 20 ).ToArray(),
			capped.AsSpan( 52, 20 ).ToArray()
		);
	}

	[Fact]
	public void PreCancelledTokenEscapesWithoutAnImage() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => Build(
				GrowthRecords(),
				cancellationToken: source.Token
			)
		);
	}

	[Fact]
	public void CancellationDuringCandidateEvaluationEscapesWithoutAnImage() {
		using var source = new CancellationTokenSource();
		var records = new CancelAfterReadsList(
			GrowthRecords(),
			source,
			cancelOnRead: 2
		);
		Assert.Throws<OperationCanceledException>(
			() => Build( records, cancellationToken: source.Token )
		);
	}

	[Fact]
	public void IntMaxValueCeilingDoesNotCauseGratuitousAllocation() {
		Assert.Equal(
			5 * PageSize,
			Build( GrowthRecords(), int.MaxValue ).Length
		);
	}

	private static BerkeleyDbHashRecord[] LongChainRecords() => Sort(
		LongChainKeys.Select(
			( item, index ) => new BerkeleyDbHashRecord(
				Encoding.ASCII.GetBytes( item.Key ),
				Enumerable.Repeat(
					checked( (byte)( index + 1 ) ),
					1024
				).ToArray()
			)
		).ToArray()
	);

	private static void AssertQualifiedHashes(
		IEnumerable<( string Key, uint Hash )> items
	) {
		foreach ( ( string key, uint hash ) in items ) {
			Assert.Equal(
				hash,
				BerkeleyDbHashV9ImageBuilder.Hash(
					Encoding.ASCII.GetBytes( key )
				)
			);
		}
	}
}
