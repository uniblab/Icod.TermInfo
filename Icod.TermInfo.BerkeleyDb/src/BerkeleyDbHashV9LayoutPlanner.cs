/*
	Icod.TermInfo.BerkeleyDb
	Preflights deterministic Berkeley DB Hash-v9 image layouts.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo.BerkeleyDb;

internal static class BerkeleyDbHashV9LayoutPlanner {
	private const int PageSize = 4096;
	private const int PageHeaderSize = 26;
	private const int BucketCount = 2;
	private const int FirstOverflowPageNumber = BucketCount + 1;
	private const int BigItemThreshold = PageSize / 4;
	private const int OverflowPayloadSize = PageSize - PageHeaderSize;

	internal static BerkeleyDbHashV9LayoutPlan Create(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( records );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
			maximumDatabaseSize
		);
		cancellationToken.ThrowIfCancellationRequested();
		if ( records.Count == 0 ) {
			throw new ArgumentException(
				"At least one Berkeley DB record is required.",
				nameof( records )
			);
		}

		var buckets = new[] {
			new List<ClassifiedRecord>(),
			new List<ClassifiedRecord>(),
		};
		int overflowPageCount = 0;
		for ( int index = 0; index < records.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			BerkeleyDbHashRecord record = records[index];
			cancellationToken.ThrowIfCancellationRequested();
			ArgumentNullException.ThrowIfNull( record );

			int keyOverflowPageCount = GetOverflowPageCount(
				record.Key.Length
			);
			int valueOverflowPageCount = GetOverflowPageCount(
				record.Value.Length
			);
			overflowPageCount = checked(
				overflowPageCount
					+ keyOverflowPageCount
					+ valueOverflowPageCount
			);
			uint hash = BerkeleyDbHashV9ImageBuilder.Hash( record.Key.Span );
			buckets[checked( (int)( hash & 1 ) )].Add(
				new ClassifiedRecord(
					hash,
					record.Key,
					keyOverflowPageCount,
					record.Value,
					valueOverflowPageCount
				)
			);
		}

		foreach ( List<ClassifiedRecord> bucket in buckets ) {
			cancellationToken.ThrowIfCancellationRequested();
			ValidateBucketFits( bucket );
		}

		int pageCount = checked(
			FirstOverflowPageNumber + overflowPageCount
		);
		int imageSize = ValidateImageSize(
			pageCount,
			maximumDatabaseSize
		);
		var hashPages = new List<BerkeleyDbHashV9HashPagePlan>(
			BucketCount
		);
		var overflowPages = new List<BerkeleyDbHashV9OverflowPagePlan>(
			overflowPageCount
		);
		int nextOverflowPageNumber = FirstOverflowPageNumber;
		for ( int bucketNumber = 0; bucketNumber < BucketCount; bucketNumber++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			List<ClassifiedRecord> bucket = buckets[bucketNumber];
			var plannedRecords = new List<BerkeleyDbHashV9RecordPlan>(
				bucket.Count
			);
			foreach ( ClassifiedRecord record in bucket ) {
				cancellationToken.ThrowIfCancellationRequested();
				BerkeleyDbHashV9ItemPlan key = CreateItemPlan(
					record.Key,
					record.KeyOverflowPageCount,
					ref nextOverflowPageNumber,
					overflowPages,
					cancellationToken
				);
				BerkeleyDbHashV9ItemPlan value = CreateItemPlan(
					record.Value,
					record.ValueOverflowPageCount,
					ref nextOverflowPageNumber,
					overflowPages,
					cancellationToken
				);
				plannedRecords.Add(
					new BerkeleyDbHashV9RecordPlan(
						record.Hash,
						key,
						value
					)
				);
			}

			hashPages.Add(
				new BerkeleyDbHashV9HashPagePlan(
					PageNumber: checked( bucketNumber + 1 ),
					BucketNumber: bucketNumber,
					PreviousPageNumber: 0,
					NextPageNumber: 0,
					Records: plannedRecords.ToArray()
				)
			);
		}

		if ( nextOverflowPageNumber != pageCount ) {
			throw new InvalidOperationException(
				"The Hash-v9 layout did not assign its declared overflow pages."
			);
		}
		return new BerkeleyDbHashV9LayoutPlan {
			BucketCount = BucketCount,
			PageCount = pageCount,
			ImageSize = imageSize,
			HashPages = hashPages.ToArray(),
			OverflowPages = overflowPages.ToArray(),
		};
	}

	private static BerkeleyDbHashV9ItemPlan CreateItemPlan(
		ReadOnlyMemory<byte> payload,
		int overflowPageCount,
		ref int nextOverflowPageNumber,
		ICollection<BerkeleyDbHashV9OverflowPagePlan> overflowPages,
		CancellationToken cancellationToken
	) {
		if ( overflowPageCount == 0 ) {
			return new BerkeleyDbHashV9ItemPlan( payload, 0, 0 );
		}

		int firstPageNumber = nextOverflowPageNumber;
		int payloadOffset = 0;
		for ( int index = 0; index < overflowPageCount; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			int pageNumber = nextOverflowPageNumber++;
			int chunkLength = Math.Min(
				OverflowPayloadSize,
				checked( payload.Length - payloadOffset )
			);
			if ( chunkLength <= 0 ) {
				throw new InvalidOperationException(
					"The Hash-v9 overflow plan contains an empty payload page."
				);
			}
			overflowPages.Add(
				new BerkeleyDbHashV9OverflowPagePlan(
					PageNumber: pageNumber,
					PreviousPageNumber: ( index == 0 )
						? 0
						: checked( pageNumber - 1 ),
					NextPageNumber: ( index + 1 == overflowPageCount )
						? 0
						: checked( pageNumber + 1 ),
					Payload: payload.Slice( payloadOffset, chunkLength )
				)
			);
			payloadOffset = checked( payloadOffset + chunkLength );
		}

		if ( payloadOffset != payload.Length ) {
			throw new InvalidOperationException(
				"The Hash-v9 overflow plan did not consume its complete payload."
			);
		}
		return new BerkeleyDbHashV9ItemPlan(
			payload,
			firstPageNumber,
			overflowPageCount
		);
	}

	private static void ValidateBucketFits(
		IReadOnlyCollection<ClassifiedRecord> records
	) {
		int tableEnd = checked(
			PageHeaderSize
				+ checked( records.Count * 2 * sizeof( ushort ) )
		);
		int highFreeOffset = PageSize;
		foreach ( ClassifiedRecord record in records ) {
			highFreeOffset = checked(
				highFreeOffset
					- GetEncodedLength(
						record.Key.Length,
						record.KeyOverflowPageCount
					)
					- GetEncodedLength(
						record.Value.Length,
						record.ValueOverflowPageCount
					)
			);
		}
		if ( highFreeOffset < tableEnd ) {
			throw new InvalidOperationException(
				"A Berkeley DB hash bucket does not fit in its canonical two-bucket HW03 page."
			);
		}
	}

	private static int ValidateImageSize(
		int pageCount,
		int maximumDatabaseSize
	) {
		if ( pageCount > maximumDatabaseSize / PageSize ) {
			long requiredSize = pageCount * (long)PageSize;
			throw new InvalidOperationException(
				$"The Berkeley DB records require a {requiredSize}-byte Hash-v9 image, which exceeds the {maximumDatabaseSize}-byte database limit."
			);
		}
		if ( pageCount > Array.MaxLength / PageSize ) {
			throw new InvalidOperationException(
				"The Berkeley DB records require a Hash-v9 image larger than the runtime array limit."
			);
		}
		return checked( pageCount * PageSize );
	}

	private static int GetEncodedLength(
		int payloadLength,
		int overflowPageCount
	) => ( overflowPageCount == 0 )
		? checked( payloadLength + 1 )
		: 12
	;

	private static int GetOverflowPageCount( int payloadLength ) =>
		( payloadLength <= BigItemThreshold )
			? 0
			: checked(
				1 + ( ( payloadLength - 1 ) / OverflowPayloadSize )
			)
	;

	private sealed record ClassifiedRecord(
		uint Hash,
		ReadOnlyMemory<byte> Key,
		int KeyOverflowPageCount,
		ReadOnlyMemory<byte> Value,
		int ValueOverflowPageCount
	);
}
