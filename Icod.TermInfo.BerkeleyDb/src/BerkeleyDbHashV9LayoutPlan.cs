/*
	Icod.TermInfo.BerkeleyDb
	Describes a preflighted deterministic Berkeley DB Hash-v9 image layout.
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

internal readonly record struct BerkeleyDbHashV9ItemPlan(
	ReadOnlyMemory<byte> Payload,
	int FirstOverflowPageNumber,
	int OverflowPageCount
) {
	internal bool IsOffPage =>
		OverflowPageCount != 0;

	internal int EncodedLength => ( IsOffPage )
		? 12
		: checked( Payload.Length + 1 )
	;
}

internal sealed record BerkeleyDbHashV9RecordPlan(
	uint Hash,
	BerkeleyDbHashV9ItemPlan Key,
	BerkeleyDbHashV9ItemPlan Value
);

internal sealed record BerkeleyDbHashV9HashPagePlan(
	int PageNumber,
	int BucketNumber,
	int PreviousPageNumber,
	int NextPageNumber,
	IReadOnlyList<BerkeleyDbHashV9RecordPlan> Records
);

internal sealed record BerkeleyDbHashV9OverflowPagePlan(
	int PageNumber,
	int PreviousPageNumber,
	int NextPageNumber,
	ReadOnlyMemory<byte> Payload
);

internal sealed class BerkeleyDbHashV9LayoutPlan {
	internal required int BucketCount { get; init; }
	internal required int PageCount { get; init; }
	internal required int ImageSize { get; init; }
	internal required IReadOnlyList<BerkeleyDbHashV9HashPagePlan> HashPages {
		get;
		init;
	}
	internal required IReadOnlyList<BerkeleyDbHashV9OverflowPagePlan> OverflowPages {
		get;
		init;
	}
}
