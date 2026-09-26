/*
	Icod.TermInfo.Hw03.ManagedWriterProbe
	Emits deterministic HW03 Berkeley DB Hash-v9 qualification fixtures.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.TermInfo.BerkeleyDb;

if ( args.Length != 1 ) {
	Console.Error.WriteLine( "Usage: Hw03.ManagedWriterProbe OUTPUT_DIRECTORY" );
	return 64;
}

string root = Path.GetFullPath( args[0] );
Directory.CreateDirectory( root );

byte[] overflowKey = Encoding.ASCII.GetBytes( "hw03-overflow" );
byte[] overflowValue = Enumerable.Range( 0, 9000 )
	.Select( static index => (byte)( index % 251 ) )
	.ToArray()
;
WriteFixture(
	root,
	"hw03-overflow",
	[
		new BerkeleyDbHashRecord( overflowKey, overflowValue ),
	],
	[ new LookupFile( "hw03-overflow", overflowKey, overflowValue ) ]
);

BerkeleyDbHashRecord[] growth = GrowthRecords();
BerkeleyDbHashRecord growthLookup = growth.Single(
	static record => Encoding.ASCII.GetString( record.Key.Span )
		== "hw03-growth-003"
);
WriteFixture(
	root,
	"hw03-growth",
	growth,
	[
		new LookupFile(
			"hw03-growth-003",
			growthLookup.Key.ToArray(),
			growthLookup.Value.ToArray()
		),
	]
);

byte[] firstCollisionKey = FirstExactCollisionKey();
byte[] secondCollisionKey = SecondExactCollisionKey();
BerkeleyDbHashRecord[] collisions = ExactCollisionRecords(
	firstCollisionKey,
	secondCollisionKey
);
BerkeleyDbHashRecord firstCollision = collisions.Single(
	record => record.Key.Span.SequenceEqual( firstCollisionKey )
);
BerkeleyDbHashRecord secondCollision = collisions.Single(
	record => record.Key.Span.SequenceEqual( secondCollisionKey )
);
WriteFixture(
	root,
	"hw03-collision",
	collisions,
	[
		new LookupFile(
			"hw03-collision-first",
			firstCollision.Key.ToArray(),
			firstCollision.Value.ToArray()
		),
		new LookupFile(
			"hw03-collision-second",
			secondCollision.Key.ToArray(),
			secondCollision.Value.ToArray()
		),
	]
);

return 0;

static BerkeleyDbHashRecord[] GrowthRecords() => [
	Record( "hw03-growth-003", 0x03 ),
	Record( "hw03-growth-007", 0x07 ),
	Record( "hw03-growth-001", 0x01 ),
	Record( "hw03-growth-005", 0x05 ),
];

static BerkeleyDbHashRecord[] ExactCollisionRecords(
	byte[] firstKey,
	byte[] secondKey
) => [
	new BerkeleyDbHashRecord(
		firstKey,
		Enumerable.Repeat( (byte)0x31, 1024 ).ToArray()
	),
	new BerkeleyDbHashRecord(
		secondKey,
		Enumerable.Repeat( (byte)0x32, 1024 ).ToArray()
	),
];

static byte[] FirstExactCollisionKey() =>
	Encoding.ASCII.GetBytes(
		"hw03-0c5ny4k-da6" + new string( 'x', 1000 )
	)
;

static byte[] SecondExactCollisionKey() =>
	Encoding.ASCII.GetBytes(
		"hw03-0fpxptj-1j8p" + new string( 'x', 1000 )
	)
;

static BerkeleyDbHashRecord Record( string key, byte fill ) =>
	new(
		Encoding.ASCII.GetBytes( key ),
		Enumerable.Repeat( fill, 1024 ).ToArray()
	)
;

static void WriteFixture(
	string root,
	string databaseName,
	BerkeleyDbHashRecord[] records,
	IReadOnlyList<LookupFile> lookups
) {
	Array.Sort(
		records,
		static ( left, right ) =>
			BerkeleyDbHashV9WriterKeyComparer.Instance.Compare(
				left.Key.Span,
				right.Key.Span
			)
	);
	byte[] database = BerkeleyDbHashV9ImageBuilder.Build(
		records,
		BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		CancellationToken.None
	);
	File.WriteAllBytes( Path.Combine( root, databaseName + ".db" ), database );
	foreach ( LookupFile lookup in lookups ) {
		File.WriteAllBytes(
			Path.Combine( root, lookup.Stem + ".key" ),
			lookup.Key
		);
		File.WriteAllBytes(
			Path.Combine( root, lookup.Stem + ".value" ),
			lookup.Value
		);
	}
}

internal sealed record LookupFile(
	string Stem,
	byte[] Key,
	byte[] Value
);
