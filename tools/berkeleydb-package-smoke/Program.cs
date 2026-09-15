using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;

const string canonical = "hdb03-package";
const string alias = "hdb03-package-alias";
const string description = "HDB03 isolated package consumer";
string databasePath = Path.Combine(
	Path.GetTempPath(),
	"Icod.TermInfo.HDB03PackageSmoke." + Guid.NewGuid().ToString( "N" ) + ".db"
);

try {
	File.WriteAllBytes(
		databasePath,
		CreateStore(
			canonical,
			alias,
			description
		)
	);

	CompiledTermInfoParserOptions parserOptions =
		new( maximumEntrySize: 4096 );
	BerkeleyDbTerminalDescriptionProviderOptions options =
		new(
			parserOptions,
			maximumDatabaseSize: 1024 * 1024,
			maximumIndexHops: 4
		);

	if ( ReferenceEquals( parserOptions, options.ParserOptions ) ) {
		throw new InvalidOperationException(
			"The provider options did not snapshot parser limits."
		);
	}

	BerkeleyDbTerminalDescriptionProvider provider =
		new(
			databasePath,
			options
		);
	if (
		!provider.TryLoad(
			alias,
			out TerminalDescription? terminal
		)
	) {
		throw new InvalidOperationException(
			"The packaged provider did not resolve the alias."
		);
	}
	if (
		!string.Equals(
			canonical,
			terminal.Name,
			StringComparison.Ordinal
		)
		|| !string.Equals(
			description,
			terminal.Description,
			StringComparison.Ordinal
		)
		|| !terminal.Aliases.Contains(
			alias,
			StringComparer.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The packaged provider returned an unexpected terminal identity."
		);
	}

	AssertReferenceEqual(
		terminal,
		LoadRequired(
			provider,
			alias
		)
	);

	if (
		provider.TryLoad(
			"hdb03-missing",
			out TerminalDescription? missing
		)
		|| missing is not null
	) {
		throw new InvalidOperationException(
			"The packaged provider did not return a clean miss."
		);
	}

	Console.WriteLine(
		$"HDB03 package provider loaded {terminal.Name} through alias {alias}."
	);
} finally {
	File.Delete( databasePath );
}

static TerminalDescription LoadRequired(
	BerkeleyDbTerminalDescriptionProvider provider,
	string name
) {
	if (
		!provider.TryLoad(
			name,
			out TerminalDescription? terminal
		)
	) {
		throw new InvalidOperationException(
			$"Terminal '{name}' was not found."
		);
	}
	return terminal;
}

static void AssertReferenceEqual(
	TerminalDescription expected,
	TerminalDescription actual
) {
	if ( !ReferenceEquals( expected, actual ) ) {
		throw new InvalidOperationException(
			"The packaged provider did not cache the successful result."
		);
	}
}

static byte[] CreateStore(
	string canonical,
	string alias,
	string description
) {
	byte[] entry =
		CreateCompiledEntry(
			canonical,
			alias,
			description
		);
	byte[] target =
		Encoding.UTF8.GetBytes(
			canonical + "|" + alias
		);

	return CreateDatabase(
		(
			Encoding.UTF8.GetBytes( canonical ),
			PrependMarker( target, 2 )
		),
		(
			Encoding.UTF8.GetBytes( alias ),
			PrependMarker( target, 2 )
		),
		(
			target,
			PrependMarker( entry, 0 )
		)
	);
}

static byte[] CreateCompiledEntry(
	string canonical,
	string alias,
	string description
) {
	byte[] names =
		Encoding.Latin1.GetBytes(
			canonical + "|" + alias + "|" + description + "\0"
		);
	int length = 12 + names.Length;
	if ( ( length & 1 ) != 0 ) {
		length++;
	}

	byte[] entry = new byte[length];
	BinaryPrimitives.WriteUInt16LittleEndian(
		entry.AsSpan( 0, 2 ),
		0x011A
	);
	BinaryPrimitives.WriteUInt16LittleEndian(
		entry.AsSpan( 2, 2 ),
		checked( (ushort)names.Length )
	);
	names.CopyTo( entry.AsSpan( 12 ) );
	return entry;
}

static byte[] PrependMarker(
	byte[] bytes,
	byte marker
) {
	byte[] value = new byte[bytes.Length + 1];
	value[0] = marker;
	bytes.CopyTo( value.AsSpan( 1 ) );
	return value;
}

static byte[] CreateDatabase(
	params ( byte[] Key, byte[] Value )[] records
) {
	byte[] database = new byte[512 * ( records.Length + 1 )];
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 12, 4 ),
		0x00061561
	);
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 16, 4 ),
		9
	);
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 20, 4 ),
		512
	);
	database[25] = 8;
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 32, 4 ),
		(uint)records.Length
	);

	for ( int index = 0; index < records.Length; index++ ) {
		( byte[] key, byte[] value ) = records[index];
		Span<byte> page =
			database.AsSpan(
				512 * ( index + 1 ),
				512
			);
		BinaryPrimitives.WriteUInt32LittleEndian(
			page.Slice( 8, 4 ),
			(uint)( index + 1 )
		);
		page[25] = 13;
		ushort keyOffset =
			checked(
				(ushort)( 511 - key.Length )
			);
		ushort valueOffset =
			checked(
				(ushort)( keyOffset - value.Length - 1 )
			);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 20, 2 ),
			2
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 22, 2 ),
			valueOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 26, 2 ),
			keyOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 28, 2 ),
			valueOffset
		);
		page[keyOffset] = 1;
		key.CopyTo( page[( keyOffset + 1 )..] );
		page[valueOffset] = 1;
		value.CopyTo( page[( valueOffset + 1 )..] );
	}

	return database;
}
