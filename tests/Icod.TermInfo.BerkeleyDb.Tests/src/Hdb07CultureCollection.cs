/*
	Icod.TermInfo.BerkeleyDb.Tests
	Isolates HDB07 tests which temporarily change process culture.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

[CollectionDefinition( Name, DisableParallelization = true )]
public sealed class Hdb07CultureCollection {
	public const string Name = "HDB07 culture-sensitive process state";
}
