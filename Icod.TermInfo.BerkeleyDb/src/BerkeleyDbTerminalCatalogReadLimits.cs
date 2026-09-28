/*
	Icod.TermInfo.BerkeleyDb
	Bounded acquisition of ncurses-compatible Berkeley DB Hash-v9 catalogs.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Configures inclusive publication and aggregate byte budgets for bounded catalog reads.</summary>
/// <remarks>These limits measure input work, not exact managed heap allocations.</remarks>
public sealed class BerkeleyDbTerminalCatalogReadLimits {
	/// <summary>Initializes immutable bounded-read limits.</summary>
	/// <param name="maximumPublicationCount">Maximum actual ncurses publication rows.</param>
	/// <param name="maximumDecodedBytes">Maximum sum of extracted key/value lengths, including repeated overflow references.</param>
	/// <param name="maximumParsedBytes">Maximum compiled payload bytes, charged once per storage key including orphans.</param>
	/// <exception cref="ArgumentOutOfRangeException">A limit is not positive.</exception>
	public BerkeleyDbTerminalCatalogReadLimits(
		int maximumPublicationCount = 65_536,
		long maximumDecodedBytes = 67_108_864,
		long maximumParsedBytes = 67_108_864
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumPublicationCount );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDecodedBytes );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumParsedBytes );
		MaximumPublicationCount = maximumPublicationCount;
		MaximumDecodedBytes = maximumDecodedBytes;
		MaximumParsedBytes = maximumParsedBytes;
	}
	/// <summary>Gets the inclusive maximum number of publication rows.</summary>
	public int MaximumPublicationCount { get; }
	/// <summary>Gets the inclusive aggregate decoded key/value byte maximum.</summary>
	public long MaximumDecodedBytes { get; }
	/// <summary>Gets the inclusive aggregate compiled payload byte maximum.</summary>
	public long MaximumParsedBytes { get; }
}
