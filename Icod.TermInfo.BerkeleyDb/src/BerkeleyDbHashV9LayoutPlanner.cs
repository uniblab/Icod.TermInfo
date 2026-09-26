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
	private const int InitialBucketCount = 2;
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

		try {
			return CreateChecked(
				records,
				maximumDatabaseSize,
				cancellationToken
			);
		} catch ( OverflowException exception ) {
			throw new InvalidOperationException(
				"The Berkeley DB records exceed the Hash-v9 numeric limits.",
				exception
			);
		}
	}

	private static BerkeleyDbHashV9LayoutPlan CreateChecked(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize,
		CancellationToken cancellationToken
	) {
		( RecordShape[] shapes, int overflowPageCount ) = CreateShapes(
			records,
			cancellationToken
		);
		int pageBudget = Math.Min(
			maximumDatabaseSize / PageSize,
			Array.MaxLength / PageSize
		);
		CandidateLayout? greatestFeasible = null;
		for (
			int bucketCount = InitialBucketCount;
			;
			bucketCount = checked( bucketCount * 2 )
		) {
			cancellationToken.ThrowIfCancellationRequested();
			CandidateLayout candidate = Evaluate(
				shapes,
				bucketCount,
				overflowPageCount,
				pageBudget,
				cancellationToken
			);
			if ( candidate.IsFeasible ) {
				greatestFeasible = candidate;
				if ( !candidate.HasReducibleCollision ) {
					return Finalize(
						candidate,
						cancellationToken
					);
				}
			} else if ( !candidate.HasReducibleCollision ) {
				break;
			}

			if (
				!CanEvaluateNextBucketCount(
					bucketCount,
					pageBudget,
					overflowPageCount
				)
			) {
				break;
			}
		}

		if ( greatestFeasible is null ) {
			throw new InvalidOperationException(
				$"The Berkeley DB records do not fit the configured {maximumDatabaseSize}-byte database limit."
			);
		}
		return Finalize(
			greatestFeasible,
			cancellationToken
		);
	}

	private static (
		RecordShape[] Shapes,
		int OverflowPageCount
	) CreateShapes(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		CancellationToken cancellationToken
	) {
		var shapes = new RecordShape[records.Count];
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
			var shape = new RecordShape(
				Hash: BerkeleyDbHashV9ImageBuilder.Hash( record.Key.Span ),
				KeyPayload: record.Key,
				KeyEncodedLength: GetEncodedLength(
					record.Key.Length,
					keyOverflowPageCount
				),
				KeyOverflowPageCount: keyOverflowPageCount,
				ValuePayload: record.Value,
				ValueEncodedLength: GetEncodedLength(
					record.Value.Length,
					valueOverflowPageCount
				),
				ValueOverflowPageCount: valueOverflowPageCount
			);
			if ( !Fits( 0, 0, shape ) ) {
				throw new InvalidOperationException(
					"A Berkeley DB record pair does not fit on an empty Hash-v9 page."
				);
			}
			shapes[index] = shape;
		}
		return ( shapes, overflowPageCount );
	}

	private static CandidateLayout Evaluate(
		IReadOnlyList<RecordShape> shapes,
		int bucketCount,
		int overflowPageCount,
		int pageBudget,
		CancellationToken cancellationToken
	) {
		var buckets = new List<RecordShape>[bucketCount];
		for ( int index = 0; index < buckets.Length; index++ ) {
			buckets[index] = [];
		}
		uint bucketMask = checked( (uint)( bucketCount - 1 ) );
		foreach ( RecordShape shape in shapes ) {
			cancellationToken.ThrowIfCancellationRequested();
			int bucketNumber = checked( (int)( shape.Hash & bucketMask ) );
			buckets[bucketNumber].Add( shape );
		}

		var bucketPages =
			new List<IReadOnlyList<IReadOnlyList<RecordShape>>>(
				bucketCount
			);
		int continuationPageCount = 0;
		bool hasReducibleCollision = false;
		foreach ( List<RecordShape> bucket in buckets ) {
			cancellationToken.ThrowIfCancellationRequested();
			IReadOnlyList<IReadOnlyList<RecordShape>> pages = PackBucket(
				bucket,
				cancellationToken
			);
			bucketPages.Add( pages );
			continuationPageCount = checked(
				continuationPageCount + pages.Count - 1
			);
			if (
				pages.Count > 1
				&& bucket.Select( static record => record.Hash )
					.Distinct()
					.Skip( 1 )
					.Any()
			) {
				hasReducibleCollision = true;
			}
		}

		int totalPageCount = checked(
			1
				+ bucketCount
				+ continuationPageCount
				+ overflowPageCount
		);
		return new CandidateLayout(
			BucketCount: bucketCount,
			BucketPages: bucketPages.ToArray(),
			ContinuationPageCount: continuationPageCount,
			TotalPageCount: totalPageCount,
			HasReducibleCollision: hasReducibleCollision,
			IsFeasible: totalPageCount <= pageBudget
		);
	}

	private static IReadOnlyList<IReadOnlyList<RecordShape>> PackBucket(
		IReadOnlyList<RecordShape> records,
		CancellationToken cancellationToken
	) {
		var pages = new List<IReadOnlyList<RecordShape>>();
		var current = new List<RecordShape>();
		int currentItemBytes = 0;
		foreach ( RecordShape record in records ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !Fits( current.Count, currentItemBytes, record ) ) {
				if ( current.Count == 0 ) {
					throw new InvalidOperationException(
						"A Berkeley DB record pair does not fit on an empty Hash-v9 page."
					);
				}
				pages.Add( current.ToArray() );
				current = [];
				currentItemBytes = 0;
			}
			current.Add( record );
			currentItemBytes = checked(
				currentItemBytes
					+ record.KeyEncodedLength
					+ record.ValueEncodedLength
			);
		}
		pages.Add( current.ToArray() );
		return pages.ToArray();
	}

	private static BerkeleyDbHashV9LayoutPlan Finalize(
		CandidateLayout candidate,
		CancellationToken cancellationToken
	) {
		int nextContinuationPageNumber = checked(
			candidate.BucketCount + 1
		);
		var pageNumbers = new int[candidate.BucketCount][];
		for (
			int bucketNumber = 0;
			bucketNumber < candidate.BucketCount;
			bucketNumber++
		) {
			cancellationToken.ThrowIfCancellationRequested();
			int pageCount = candidate.BucketPages[bucketNumber].Count;
			var numbers = new int[pageCount];
			numbers[0] = checked( bucketNumber + 1 );
			for ( int pageIndex = 1; pageIndex < pageCount; pageIndex++ ) {
				numbers[pageIndex] = nextContinuationPageNumber;
				nextContinuationPageNumber = checked(
					nextContinuationPageNumber + 1
				);
			}
			pageNumbers[bucketNumber] = numbers;
		}

		int firstOverflowPageNumber = checked(
			1
				+ candidate.BucketCount
				+ candidate.ContinuationPageCount
		);
		if ( nextContinuationPageNumber != firstOverflowPageNumber ) {
			throw new InvalidOperationException(
				"The Hash-v9 layout did not assign its declared continuation pages."
			);
		}

		var hashPages = new List<BerkeleyDbHashV9HashPagePlan>(
			checked(
				candidate.BucketCount + candidate.ContinuationPageCount
			)
		);
		var overflowPages = new List<BerkeleyDbHashV9OverflowPagePlan>();
		int nextOverflowPageNumber = firstOverflowPageNumber;
		for (
			int bucketNumber = 0;
			bucketNumber < candidate.BucketCount;
			bucketNumber++
		) {
			cancellationToken.ThrowIfCancellationRequested();
			IReadOnlyList<IReadOnlyList<RecordShape>> pages =
				candidate.BucketPages[bucketNumber];
			int[] numbers = pageNumbers[bucketNumber];
			for ( int pageIndex = 0; pageIndex < pages.Count; pageIndex++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				IReadOnlyList<RecordShape> sourceRecords = pages[pageIndex];
				var records = new List<BerkeleyDbHashV9RecordPlan>(
					sourceRecords.Count
				);
				foreach ( RecordShape record in sourceRecords ) {
					cancellationToken.ThrowIfCancellationRequested();
					BerkeleyDbHashV9ItemPlan key = CreateItemPlan(
						record.KeyPayload,
						record.KeyOverflowPageCount,
						ref nextOverflowPageNumber,
						overflowPages,
						cancellationToken
					);
					BerkeleyDbHashV9ItemPlan value = CreateItemPlan(
						record.ValuePayload,
						record.ValueOverflowPageCount,
						ref nextOverflowPageNumber,
						overflowPages,
						cancellationToken
					);
					records.Add(
						new BerkeleyDbHashV9RecordPlan(
							record.Hash,
							key,
							value
						)
					);
				}

				hashPages.Add(
					new BerkeleyDbHashV9HashPagePlan(
						PageNumber: numbers[pageIndex],
						BucketNumber: bucketNumber,
						PreviousPageNumber: ( pageIndex == 0 )
							? 0
							: numbers[pageIndex - 1],
						NextPageNumber: ( pageIndex + 1 == pages.Count )
							? 0
							: numbers[pageIndex + 1],
						Records: records.ToArray()
					)
				);
			}
		}

		if ( nextOverflowPageNumber != candidate.TotalPageCount ) {
			throw new InvalidOperationException(
				"The Hash-v9 layout did not assign its declared overflow pages."
			);
		}
		int imageSize = checked( candidate.TotalPageCount * PageSize );
		return new BerkeleyDbHashV9LayoutPlan {
			BucketCount = candidate.BucketCount,
			PageCount = candidate.TotalPageCount,
			ImageSize = imageSize,
			HashPages = hashPages
				.OrderBy( static page => page.PageNumber )
				.ToArray(),
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
			int pageNumber = nextOverflowPageNumber;
			nextOverflowPageNumber = checked(
				nextOverflowPageNumber + 1
			);
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

	private static bool Fits(
		int currentRecordCount,
		int currentItemBytes,
		RecordShape next
	) => checked(
		PageHeaderSize
			+ ( ( currentRecordCount + 1 ) * 2 * sizeof( ushort ) )
			+ currentItemBytes
			+ next.KeyEncodedLength
			+ next.ValueEncodedLength
	) <= PageSize;

	private static bool CanEvaluateNextBucketCount(
		int bucketCount,
		int pageBudget,
		int overflowPageCount
	) {
		if ( bucketCount > int.MaxValue / 2 ) {
			return false;
		}
		int nextBucketCount = checked( bucketCount * 2 );
		long minimumPageCount = 1L
			+ nextBucketCount
			+ overflowPageCount
		;
		return minimumPageCount <= pageBudget;
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

	private sealed record RecordShape(
		uint Hash,
		ReadOnlyMemory<byte> KeyPayload,
		int KeyEncodedLength,
		int KeyOverflowPageCount,
		ReadOnlyMemory<byte> ValuePayload,
		int ValueEncodedLength,
		int ValueOverflowPageCount
	);

	private sealed record CandidateLayout(
		int BucketCount,
		IReadOnlyList<IReadOnlyList<IReadOnlyList<RecordShape>>> BucketPages,
		int ContinuationPageCount,
		int TotalPageCount,
		bool HasReducibleCollision,
		bool IsFeasible
	);
}
