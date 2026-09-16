/*
	Icod.TermInfo.BerkeleyDb.Tests
	Characterizes HDB07 ncurses envelopes, identity, parsing, and culture invariance.
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
using Icod.TermInfo;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

[Collection( Hdb07CultureCollection.Name )]
public sealed class Hdb07LogicalHardeningTests {
	private const int PageSize = 512;

	public static TheoryData<string, string> EnvelopeFailureCases => new() {
		{
			"empty-record",
			"The ncurses hashed-term record is empty."
		},
		{
			"empty-index-target",
			"The ncurses index record has an empty target."
		},
		{
			"unsupported-marker",
			"Ncurses hashed-term marker 7 is not supported."
		},
		{
			"dangling-target",
			"The ncurses index record references a missing key."
		},
		{
			"repeated-key-cycle",
			"The ncurses index chain contains a cycle."
		},
		{
			"multi-key-cycle",
			"The ncurses index chain contains a cycle."
		},
	};

	public static TheoryData<string> UnsafePublicationNames => new() {
		"",
		" ",
		".",
		"..",
		"bad/name",
		"bad\\name",
		"bad\0name",
		"bad\u0001name",
	};

	public static TheoryData<string, string, string[], bool> IdentityCases =>
		new() {
			{ "sample", "sample", [], true },
			{ "sample-alias", "sample", [ "sample-alias" ], true },
			{ "missing", "sample", [], false },
			{ "other", "sample", [ "sample-alias" ], false },
		};

	[Fact]
	public void ProviderLoadsDirectMarkerZeroRecord() {
		byte[] database = CreateDatabase(
			CreateStorageRecord(
				Encoding.UTF8.GetBytes( "sample" ),
				"sample"
			)
		);

		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );

				Assert.True(
					provider.TryLoad(
						"sample",
						out TerminalDescription? terminal
					)
				);
				Assert.Equal( "sample", terminal!.Name );
			}
		);
	}

	[Fact]
	public void MarkerTwoChainAcceptsInclusiveHopLimitAndRejectsNext() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		byte[] database = CreateDatabase(
			CreateIndexRecord( "sample", Encoding.UTF8.GetBytes( "link" ) ),
			CreateRecord(
				Encoding.UTF8.GetBytes( "link" ),
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateStorageRecord( storageKey, "sample" )
		);

		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalDescriptionProvider accepted =
					new(
						path,
						new BerkeleyDbTerminalDescriptionProviderOptions(
							maximumIndexHops: 2
						)
					);
				Assert.True(
					accepted.TryLoad(
						"sample",
						out TerminalDescription? terminal
					)
				);
				Assert.Equal( "sample", terminal!.Name );

				BerkeleyDbTerminalDescriptionProvider rejected =
					new(
						path,
						new BerkeleyDbTerminalDescriptionProviderOptions(
							maximumIndexHops: 1
						)
					);
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => rejected.TryLoad( "sample", out _ )
					);
				Assert.Equal(
					"The ncurses index chain exceeds the configured hop limit.",
					error.InnerException?.Message
				);
			}
		);
	}

	[Theory]
	[MemberData( nameof( EnvelopeFailureCases ) )]
	public void CatalogRejectsEnvelopeFailuresDeterministically(
		string caseName,
		string expectedMessage
	) {
		byte[] database = CreateEnvelopeFailureDatabase( caseName );

		WithDatabase(
			database,
			path => {
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => new BerkeleyDbTerminalCatalogReader(
							path
						).Read()
					);
				Assert.Equal( expectedMessage, error.Message );
			}
		);
	}

	[Fact]
	public void CatalogResolvesEmbeddedNulIndexTargetAsBinaryKey() {
		byte[] storageKey = [ 0x73, 0x00, 0x74 ];
		byte[] database = CreateDatabase(
			CreateIndexRecord( "sample", storageKey ),
			CreateStorageRecord( storageKey, "sample" )
		);

		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalCatalogEntry entry = Assert.Single(
					new BerkeleyDbTerminalCatalogReader( path ).Read()
				);
				Assert.Equal( "sample", entry.Name );
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Canonical,
					entry.Kind
				);
			}
		);
	}

	[Fact]
	public void CatalogRejectsUnsafeLatin1FallbackPublicationKey() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		byte[] database = CreateDatabase(
			CreateRecord(
				[ 0xE9, 0x00 ],
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateStorageRecord( storageKey, "sample" )
		);

		AssertPublicationNameFailure( database );
	}

	[Theory]
	[MemberData( nameof( UnsafePublicationNames ) )]
	public void CatalogRejectsUnsafePublicationName(
		string unsafeName
	) {
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		byte[] database = CreateDatabase(
			CreateRecord(
				Encoding.UTF8.GetBytes( unsafeName ),
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateStorageRecord( storageKey, "sample" )
		);

		AssertPublicationNameFailure( database );
	}

	[Fact]
	public void ProviderRejectsSurrogateLookupBeforeOpeningDatabase() {
		string path = Path.Combine(
			Path.GetTempPath(),
			Guid.NewGuid().ToString( "N" ) + ".db"
		);
		BerkeleyDbTerminalDescriptionProvider provider = new( path );

		ArgumentException error = Assert.Throws<ArgumentException>(
			() => provider.TryLoad( "bad\uD800name", out _ )
		);
		Assert.StartsWith(
			"The terminal name contains unsafe key syntax.",
			error.Message,
			StringComparison.Ordinal
		);
	}

	[Theory]
	[MemberData( nameof( IdentityCases ) )]
	public void ProviderRequiresExactCanonicalOrAliasIdentity(
		string requestedName,
		string canonicalName,
		string[] aliases,
		bool shouldSucceed
	) {
		byte[] database = CreateDatabase(
			CreateStorageRecord(
				Encoding.UTF8.GetBytes( requestedName ),
				canonicalName,
				aliases
			)
		);

		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );
				if ( shouldSucceed ) {
					Assert.True(
						provider.TryLoad(
							requestedName,
							out TerminalDescription? terminal
						)
					);
					Assert.Equal( canonicalName, terminal!.Name );
				} else {
					InvalidDataException error =
						Assert.Throws<InvalidDataException>(
							() => provider.TryLoad(
								requestedName,
								out _
							)
						);
					Assert.Equal(
						$"Compiled terminfo entry in the Berkeley DB identifies terminal '{canonicalName}' and does not declare requested name '{requestedName}'.",
						error.Message
					);
				}
			}
		);
	}

	[Fact]
	public void ParserEntrySizeLimitIsInclusiveAtExactBoundary() {
		byte[] entry = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"sample",
			"test terminal"
		);
		byte[] database = CreateDatabase(
			CreateRecord(
				Encoding.UTF8.GetBytes( "sample" ),
				Hdb07HashV9FixtureBuilder.NcursesData( entry )
			)
		);

		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalDescriptionProvider exact =
					CreateProviderWithParserLimit( path, entry.Length );
				Assert.True( exact.TryLoad( "sample", out _ ) );

				BerkeleyDbTerminalDescriptionProvider exceeded =
					CreateProviderWithParserLimit(
						path,
						entry.Length - 1
					);
				CompiledTermInfoFormatException error =
					Assert.Throws<CompiledTermInfoFormatException>(
						() => exceeded.TryLoad( "sample", out _ )
					);
				Assert.Contains(
					$"exceeding the configured maximum of {entry.Length - 1} bytes",
					error.Message,
					StringComparison.Ordinal
				);
			}
		);
	}

	[Fact]
	public void ValidOrphanStorageRecordIsValidatedButNotPublished() {
		byte[] database = CreateDatabase(
			CreateStorageRecord(
				Encoding.UTF8.GetBytes( "orphan-storage" ),
				"orphan"
			)
		);

		WithDatabase(
			database,
			path => Assert.Empty(
				new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void MalformedOrphanStorageRecordIsRejected() {
		byte[] database = CreateDatabase(
			CreateRecord(
				Encoding.UTF8.GetBytes( "orphan-storage" ),
				[ 0x00, 0x01, 0x02 ]
			)
		);

		WithDatabase(
			database,
			path => Assert.Throws<CompiledTermInfoFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void PublicationsForOneStorageRecordShareParsedIdentity() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		byte[] database = CreateDatabase(
			CreateIndexRecord( "sample", storageKey ),
			CreateIndexRecord( "sample-alias", storageKey ),
			CreateStorageRecord(
				storageKey,
				"sample",
				"sample-alias"
			)
		);

		WithDatabase(
			database,
			path => {
				IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
					new BerkeleyDbTerminalCatalogReader( path ).Read();
				Assert.Equal(
					new[] { "sample", "sample-alias" },
					entries.Select( entry => entry.Name ).ToArray()
				);
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Canonical,
					entries[0].Kind
				);
				Assert.Equal(
					BerkeleyDbTerminalCatalogEntryKind.Alias,
					entries[1].Kind
				);
				Assert.Same( entries[0].Terminal, entries[1].Terminal );
			}
		);
	}

	[Theory]
	[InlineData( "tr-TR" )]
	[InlineData( "ar-SA" )]
	[InlineData( "ja-JP" )]
	public void StrictUtf8ByteKeyOrderingIsOrdinalUnderCulture(
		string cultureName
	) {
		string[] names = [ "\u0131", "i", "\u0130", "I" ];
		Hdb07RecordSpec[] records = names
			.Select(
				name => CreateRecord(
					Encoding.UTF8.GetBytes( name ),
					[ 0x00 ]
				)
			)
			.ToArray();
		byte[] database = CreateDatabase( records );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		using ( new CultureScope( cultureName ) ) {
			IReadOnlyList<BerkeleyDbHashRecord> ordered =
				BerkeleyDbHashReader.ReadRecords(
					database,
					maximumItemSize: 16,
					maximumRecordCount: names.Length,
					CancellationToken.None
				);
			Assert.Equal(
				new[] { "I", "i", "\u0130", "\u0131" },
				ordered
					.Select( record => Encoding.UTF8.GetString( record.Key.Span ) )
					.ToArray()
			);
		}

		Assert.Same( originalCulture, CultureInfo.CurrentCulture );
		Assert.Same( originalUiCulture, CultureInfo.CurrentUICulture );
	}

	[Theory]
	[InlineData( "tr-TR" )]
	[InlineData( "ar-SA" )]
	[InlineData( "ja-JP" )]
	public void PublicationOrderingIsOrdinalUnderCulture(
		string cultureName
	) {
		string[] names = [ "I", "i", "\u00D0", "\u00F0" ];
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		var records = new List<Hdb07RecordSpec>();
		foreach ( string name in names.Reverse() ) {
			records.Add( CreateIndexRecord( name, storageKey ) );
		}
		records.Add(
			CreateStorageRecord(
				storageKey,
				"I",
				"i",
				"\u00D0",
				"\u00F0"
			)
		);
		byte[] database = CreateDatabase( records.ToArray() );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		WithDatabase(
			database,
			path => {
				using ( new CultureScope( cultureName ) ) {
					IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
						new BerkeleyDbTerminalCatalogReader( path ).Read();
					Assert.Equal(
						new[] { "I", "i", "\u00D0", "\u00F0" },
						entries.Select( entry => entry.Name ).ToArray()
					);
				}
			}
		);

		Assert.Same( originalCulture, CultureInfo.CurrentCulture );
		Assert.Same( originalUiCulture, CultureInfo.CurrentUICulture );
	}

	private static byte[] CreateEnvelopeFailureDatabase(
		string caseName
	) {
		return caseName switch {
			"empty-record" => CreateDatabase(
				CreateRecord( Encoding.UTF8.GetBytes( "sample" ), [] )
			),
			"empty-index-target" => CreateDatabase(
				CreateRecord( Encoding.UTF8.GetBytes( "sample" ), [ 0x02 ] )
			),
			"unsupported-marker" => CreateDatabase(
				CreateRecord( Encoding.UTF8.GetBytes( "sample" ), [ 0x07 ] )
			),
			"dangling-target" => CreateDatabase(
				CreateIndexRecord(
					"sample",
					Encoding.UTF8.GetBytes( "missing" )
				)
			),
			"repeated-key-cycle" => CreateDatabase(
				CreateIndexRecord(
					"sample",
					Encoding.UTF8.GetBytes( "sample" )
				)
			),
			"multi-key-cycle" => CreateDatabase(
				CreateIndexRecord( "a", Encoding.UTF8.GetBytes( "b" ) ),
				CreateIndexRecord( "b", Encoding.UTF8.GetBytes( "a" ) )
			),
			_ => throw new InvalidOperationException(
				$"Unknown envelope-failure case '{caseName}'."
			),
		};
	}

	private static BerkeleyDbTerminalDescriptionProvider
		CreateProviderWithParserLimit(
			string path,
			int maximumEntrySize
		) {
		return new BerkeleyDbTerminalDescriptionProvider(
			path,
			new BerkeleyDbTerminalDescriptionProviderOptions(
				new CompiledTermInfoParserOptions( maximumEntrySize )
			)
		);
	}

	private static void AssertPublicationNameFailure(
		byte[] database
	) {
		WithDatabase(
			database,
			path => {
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => new BerkeleyDbTerminalCatalogReader(
							path
						).Read()
					);
				Assert.Equal(
					"The ncurses publication key is not a safe exact UTF-8 or Latin-1 terminal name.",
					error.Message
				);
			}
		);
	}

	private static byte[] CreateDatabase(
		params Hdb07RecordSpec[] records
	) {
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			records
		);
	}

	private static Hdb07RecordSpec CreateRecord(
		byte[] key,
		byte[] value
	) {
		return new Hdb07RecordSpec(
			Hdb07ItemSpec.Inline( key ),
			Hdb07ItemSpec.Inline( value )
		);
	}

	private static Hdb07RecordSpec CreateIndexRecord(
		string publicationName,
		byte[] targetKey
	) {
		return CreateRecord(
			Encoding.UTF8.GetBytes( publicationName ),
			Hdb07HashV9FixtureBuilder.NcursesIndex( targetKey )
		);
	}

	private static Hdb07RecordSpec CreateStorageRecord(
		byte[] storageKey,
		string canonicalName,
		params string[] aliases
	) {
		byte[] compiledEntry =
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				canonicalName,
				"test terminal",
				aliases
			);
		return CreateRecord(
			storageKey,
			Hdb07HashV9FixtureBuilder.NcursesData( compiledEntry )
		);
	}

	private static void WithDatabase(
		byte[] database,
		Action<string> action
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			action( path );
		} finally {
			File.Delete( path );
		}
	}

	private sealed class CultureScope : IDisposable {
		private readonly CultureInfo _culture =
			CultureInfo.CurrentCulture;
		private readonly CultureInfo _uiCulture =
			CultureInfo.CurrentUICulture;

		internal CultureScope( string name ) {
			CultureInfo selected = CultureInfo.GetCultureInfo( name );
			CultureInfo.CurrentCulture = selected;
			CultureInfo.CurrentUICulture = selected;
		}

		public void Dispose() {
			CultureInfo.CurrentCulture = _culture;
			CultureInfo.CurrentUICulture = _uiCulture;
		}
	}
}
