/*
	Icod.TermInfo.Hw00.ManagedWriterProbe
	Research-only managed Berkeley DB Hash-v9 writer prototype.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

namespace Icod.TermInfo.Hw00.ManagedWriterProbe;

public static class Hw00HashV9Writer {
	public const int PageSize = 4096;

	public static void WriteNcursesCatalog(
		Stream destination,
		IEnumerable<ReadOnlyMemory<byte>> compiledEntries
	) {
		ArgumentNullException.ThrowIfNull( destination );
		ArgumentNullException.ThrowIfNull( compiledEntries );
		throw new NotSupportedException(
			"HW00 RED: the managed Hash-v9 writer is not implemented."
		);
	}
}
