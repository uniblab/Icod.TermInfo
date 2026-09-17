using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;

const string canonical = "hdb03-package";
const string alias = "hdb03-package-alias";
const string description = "HDB03 isolated package consumer";
const string latin1Canonical = "hdb07c-package-caf\u00E9";
const string latin1Alias = "hdb07c-package-ali\u00E9";
string databasePath = Path.Combine(
	Path.GetTempPath(),
	"Icod.TermInfo.HDB03PackageSmoke." + Guid.NewGuid().ToString( "N" ) + ".db"
);
string latin1DatabasePath = Path.Combine(
	Path.GetTempPath(),
	"Icod.TermInfo.HDB07CPackageSmoke." + Guid.NewGuid().ToString( "N" ) + ".db"
);
string? previousTermInfo =
	Environment.GetEnvironmentVariable( "TERMINFO" );
string? previousTermInfoDirs =
	Environment.GetEnvironmentVariable( "TERMINFO_DIRS" );

try {
	File.WriteAllBytes(
		databasePath,
		CreateStore(
			canonical,
			alias,
			description,
			Encoding.UTF8
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


	CompiledTermInfoParserOptions catalogParserOptions =
		new( maximumEntrySize: 4096 );
	BerkeleyDbTerminalCatalogReaderOptions catalogOptions =
		new(
			catalogParserOptions,
			maximumDatabaseSize: 1024 * 1024,
			maximumRecordCount: 3,
			maximumIndexHops: 4
		);
	if (
		ReferenceEquals(
			catalogParserOptions,
			catalogOptions.ParserOptions
		)
		|| catalogOptions.ParserOptions.MaximumEntrySize != 4096
	) {
		throw new InvalidOperationException(
			"The catalog options did not snapshot parser limits."
		);
	}

	BerkeleyDbTerminalCatalogReader catalogReader =
		new(
			databasePath,
			catalogOptions
		);
	if (
		!string.Equals(
			Path.GetFullPath( databasePath ),
			catalogReader.DatabasePath,
			StringComparison.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The catalog reader did not canonicalize its database path."
		);
	}

	IReadOnlyList<BerkeleyDbTerminalCatalogEntry> catalog =
		catalogReader.Read();
	if (
		catalog.Count != 2
		|| !string.Equals(
			catalog[0].Name,
			canonical,
			StringComparison.Ordinal
		)
		|| catalog[0].Kind
			!= BerkeleyDbTerminalCatalogEntryKind.Canonical
		|| !string.Equals(
			catalog[1].Name,
			alias,
			StringComparison.Ordinal
		)
		|| catalog[1].Kind
			!= BerkeleyDbTerminalCatalogEntryKind.Alias
		|| !ReferenceEquals(
			catalog[0].Terminal,
			catalog[1].Terminal
		)
		|| !string.Equals(
			catalog[0].Terminal.Name,
			canonical,
			StringComparison.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The packaged catalog reader returned an unexpected snapshot."
		);
	}

	const string replacementCanonical = "hdb05-replacement";
	const string replacementAlias = "hdb05-replacement-alias";
	File.WriteAllBytes(
		databasePath,
		CreateStore(
			replacementCanonical,
			replacementAlias,
			"HDB05 fresh package snapshot",
			Encoding.UTF8
		)
	);
	IReadOnlyList<BerkeleyDbTerminalCatalogEntry> replacement =
		catalogReader.Read();
	if (
		replacement.Count != 2
		|| !replacement.Any(
			entry => string.Equals(
				entry.Name,
				replacementCanonical,
				StringComparison.Ordinal
			)
		)
		|| replacement.Any(
			entry => string.Equals(
				entry.Name,
				canonical,
				StringComparison.Ordinal
			)
		)
	) {
		throw new InvalidOperationException(
			"The packaged catalog reader did not acquire a fresh snapshot."
		);
	}

	File.WriteAllBytes(
		databasePath,
		CreateStore(
			canonical,
			alias,
			description,
			Encoding.UTF8
		)
	);

	Environment.SetEnvironmentVariable(
		"TERMINFO",
		databasePath
	);
	Environment.SetEnvironmentVariable(
		"TERMINFO_DIRS",
		null
	);
	BerkeleyDbSystemTerminalDescriptionProvider systemProvider =
		new(
			new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false,
				parserOptions: parserOptions,
				maximumDatabaseSize: 1024 * 1024,
				maximumIndexHops: 4
			)
		);
	Environment.SetEnvironmentVariable(
		"TERMINFO",
		databasePath + ".missing"
	);

	TerminalDescription systemTerminal =
		LoadRequired(
			systemProvider,
			alias
		);
	if (
		!string.Equals(
			canonical,
			systemTerminal.Name,
			StringComparison.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The packaged system provider returned an unexpected terminal identity."
		);
	}

	File.WriteAllBytes(
		latin1DatabasePath,
		CreateStore(
			latin1Canonical,
			latin1Alias,
			"HDB07C Latin-1 package fixture",
			Encoding.Latin1
		)
	);
	BerkeleyDbTerminalDescriptionProvider latin1Provider =
		new( latin1DatabasePath );
	TerminalDescription latin1CanonicalTerminal =
		LoadRequired( latin1Provider, latin1Canonical );
	TerminalDescription latin1AliasTerminal =
		LoadRequired( latin1Provider, latin1Alias );
	if (
		!string.Equals(
			latin1Canonical,
			latin1CanonicalTerminal.Name,
			StringComparison.Ordinal
		)
		|| !string.Equals(
			latin1Canonical,
			latin1AliasTerminal.Name,
			StringComparison.Ordinal
		)
		|| !latin1AliasTerminal.Aliases.Contains(
			latin1Alias,
			StringComparer.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The packaged provider did not preserve the Latin-1 terminal identity."
		);
	}

	IReadOnlyList<BerkeleyDbTerminalCatalogEntry> latin1Catalog =
		new BerkeleyDbTerminalCatalogReader(
			latin1DatabasePath
		).Read();
	BerkeleyDbTerminalCatalogEntry latin1CanonicalEntry =
		latin1Catalog.Single(
			entry => string.Equals(
				entry.Name,
				latin1Canonical,
				StringComparison.Ordinal
			)
		);
	BerkeleyDbTerminalCatalogEntry latin1AliasEntry =
		latin1Catalog.Single(
			entry => string.Equals(
				entry.Name,
				latin1Alias,
				StringComparison.Ordinal
			)
		);
	if (
		latin1Catalog.Count != 2
		|| latin1CanonicalEntry.Kind
			!= BerkeleyDbTerminalCatalogEntryKind.Canonical
		|| latin1AliasEntry.Kind
			!= BerkeleyDbTerminalCatalogEntryKind.Alias
		|| !ReferenceEquals(
			latin1CanonicalEntry.Terminal,
			latin1AliasEntry.Terminal
		)
	) {
		throw new InvalidOperationException(
			"The packaged catalog did not preserve the Latin-1 publications."
		);
	}

	Environment.SetEnvironmentVariable(
		"TERMINFO",
		latin1DatabasePath
	);
	BerkeleyDbSystemTerminalDescriptionProvider latin1SystemProvider =
		new(
			new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false
			)
		);
	TerminalDescription latin1SystemTerminal =
		LoadRequired( latin1SystemProvider, latin1Alias );
	if (
		!string.Equals(
			latin1Canonical,
			latin1SystemTerminal.Name,
			StringComparison.Ordinal
		)
	) {
		throw new InvalidOperationException(
			"The packaged system provider did not preserve the Latin-1 terminal identity."
		);
	}

	Console.WriteLine(
		$"HDB07C package provider, catalog, and system provider loaded {latin1SystemTerminal.Name} through alias {latin1Alias}."
	);
} finally {
	Environment.SetEnvironmentVariable(
		"TERMINFO",
		previousTermInfo
	);
	Environment.SetEnvironmentVariable(
		"TERMINFO_DIRS",
		previousTermInfoDirs
	);
	File.Delete( databasePath );
	File.Delete( latin1DatabasePath );
}

static TerminalDescription LoadRequired(
	ITerminalDescriptionProvider provider,
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
	string description,
	Encoding keyEncoding
) {
	ArgumentNullException.ThrowIfNull( keyEncoding );

	byte[] entry =
		CreateCompiledEntry(
			canonical,
			alias,
			description
		);
	byte[] target =
		keyEncoding.GetBytes(
			canonical + "|" + alias
		);

	return CreateDatabase(
		(
			keyEncoding.GetBytes( canonical ),
			PrependMarker( target, 2 )
		),
		(
			keyEncoding.GetBytes( alias ),
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
