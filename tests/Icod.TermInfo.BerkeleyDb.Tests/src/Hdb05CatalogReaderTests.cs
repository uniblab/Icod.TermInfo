/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HDB05 logical catalog enumeration.
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

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb05CatalogReaderTests {
	[Fact]
	public void ConstructorCanonicalizesDatabasePath() {
		string relative = "catalog.db";

		BerkeleyDbTerminalCatalogReader reader = new( relative );

		Assert.Equal( Path.GetFullPath( relative ), reader.DatabasePath );
	}

	[Theory]
	[InlineData( null )]
	[InlineData( "" )]
	[InlineData( " " )]
	public void ConstructorRejectsInvalidDatabasePath( string? path ) {
		Assert.ThrowsAny<ArgumentException>(
			() => new BerkeleyDbTerminalCatalogReader( path! )
		);
	}

	[Fact]
	public void OptionsSnapshotAllLimits() {
		CompiledTermInfoParserOptions parserOptions = new( 1234 );
		BerkeleyDbTerminalCatalogReaderOptions options =
			new(
				parserOptions,
				maximumDatabaseSize: 4096,
				maximumRecordCount: 17,
				maximumIndexHops: 7
			);

		Assert.Equal( 1234, options.ParserOptions.MaximumEntrySize );
		Assert.NotSame( parserOptions, options.ParserOptions );
		Assert.Equal( 4096, options.MaximumDatabaseSize );
		Assert.Equal( 17, options.MaximumRecordCount );
		Assert.Equal( 7, options.MaximumIndexHops );
		Assert.Equal(
			65_536,
			BerkeleyDbTerminalCatalogReaderOptions.DefaultMaximumRecordCount
		);
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectNonpositiveDatabaseLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbTerminalCatalogReaderOptions(
				maximumDatabaseSize: value
			)
		);
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectNonpositiveRecordLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbTerminalCatalogReaderOptions(
				maximumRecordCount: value
			)
		);
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 1025 )]
	public void OptionsRejectUnsupportedHopLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbTerminalCatalogReaderOptions(
				maximumIndexHops: value
			)
		);
	}

	[Fact]
	public void ReadReturnsCanonicalAndAliasPublicationsInOrdinalOrder() {
		WithDatabase(
			CreateCatalogStore(
				"sample",
				"z-alias",
				"a-alias"
			),
			path => {
				BerkeleyDbTerminalCatalogReader reader = new( path );

				IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
					reader.Read();

				Assert.Equal(
					new[] { "a-alias", "sample", "z-alias" },
					entries.Select( entry => entry.Name )
				);
				Assert.Equal(
					new[] {
						BerkeleyDbTerminalCatalogEntryKind.Alias,
						BerkeleyDbTerminalCatalogEntryKind.Canonical,
						BerkeleyDbTerminalCatalogEntryKind.Alias,
					},
					entries.Select( entry => entry.Kind )
				);
				Assert.All(
					entries,
					entry => Assert.Same( entries[0].Terminal, entry.Terminal )
				);
				Assert.Equal( "sample", entries[0].Terminal.Name );
				Assert.Contains( "a-alias", entries[0].Terminal.Aliases );
				Assert.Contains( "z-alias", entries[0].Terminal.Aliases );
			}
		);
	}

	[Fact]
	public void ReturnedCatalogIsReadOnly() {
		WithDatabase(
			CreateCatalogStore( "sample" ),
			path => {
				IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
					new BerkeleyDbTerminalCatalogReader( path ).Read();
				IList<BerkeleyDbTerminalCatalogEntry> list =
					Assert.IsAssignableFrom<IList<BerkeleyDbTerminalCatalogEntry>>(
						entries
					);

				Assert.Throws<NotSupportedException>(
					() => list.Add( entries[0] )
				);
			}
		);
	}

	[Fact]
	public void ReadFollowsMarkerTwoChainsWithinTheHopLimit() {
		byte[] storageKey =
			Encoding.UTF8.GetBytes( "sample|sample-link|test terminal" );
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "sample" ),
					PrependMarker(
						Encoding.UTF8.GetBytes( "sample-link" ),
						2
					)
				),
				(
					Encoding.UTF8.GetBytes( "sample-link" ),
					PrependMarker( storageKey, 2 )
				),
				(
					storageKey,
					PrependMarker(
						CreateCompiledEntry(
							"sample",
							[ "sample-link" ]
						)
					)
				)
			),
			path => {
				IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
					new BerkeleyDbTerminalCatalogReader( path ).Read();

				Assert.Equal(
					new[] { "sample", "sample-link" },
					entries.Select( entry => entry.Name )
				);
				Assert.Same( entries[0].Terminal, entries[1].Terminal );
			}
		);
	}

	[Fact]
	public void OrphanMarkerZeroRecordIsValidatedButNotEmitted() {
		var records = new List<( byte[] Key, byte[] Value )>(
			CreateCatalogRecords( "sample" )
		) {
			(
				Encoding.UTF8.GetBytes( "orphan-storage" ),
				PrependMarker( CreateCompiledEntry( "orphan", [] ) )
			),
		};
		WithDatabase(
			CreateDatabase( records.ToArray() ),
			path => {
				BerkeleyDbTerminalCatalogEntry entry =
					Assert.Single(
						new BerkeleyDbTerminalCatalogReader( path ).Read()
					);

				Assert.Equal( "sample", entry.Name );
			}
		);
	}

	[Fact]
	public void MalformedOrphanMarkerZeroRecordIsRejected() {
		var records = new List<( byte[] Key, byte[] Value )>(
			CreateCatalogRecords( "sample" )
		) {
			(
				Encoding.UTF8.GetBytes( "bad-storage" ),
				new byte[] { 0, 1, 2 }
			),
		};
		WithDatabase(
			CreateDatabase( records.ToArray() ),
			path => Assert.Throws<CompiledTermInfoFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void InvalidUtf8PublicationKeyIsDatabaseFormatFailure() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "sample|test terminal" );
		WithDatabase(
			CreateDatabase(
				(
					new byte[] { 0xC3, 0x28 },
					PrependMarker( storageKey, 2 )
				),
				(
					storageKey,
					PrependMarker(
						CreateCompiledEntry( "sample", [] )
					)
				)
			),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void UnsafePublicationNameIsDatabaseFormatFailure() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "sample|test terminal" );
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "bad/name" ),
					PrependMarker( storageKey, 2 )
				),
				(
					storageKey,
					PrependMarker(
						CreateCompiledEntry( "sample", [] )
					)
				)
			),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void MissingMarkerTargetIsDatabaseFormatFailure() {
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "sample" ),
					PrependMarker(
						Encoding.UTF8.GetBytes( "missing" ),
						2
					)
				)
			),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void MarkerCycleIsDatabaseFormatFailure() {
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "a" ),
					PrependMarker( Encoding.UTF8.GetBytes( "b" ), 2 )
				),
				(
					Encoding.UTF8.GetBytes( "b" ),
					PrependMarker( Encoding.UTF8.GetBytes( "a" ), 2 )
				)
			),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void MarkerHopLimitIsInclusive() {
		BerkeleyDbTerminalCatalogReaderOptions options =
			new( maximumIndexHops: 0 );
		WithDatabase(
			CreateCatalogStore( "sample" ),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path, options ).Read()
			)
		);
	}

	[Fact]
	public void UnsupportedMarkerIsDatabaseFormatFailure() {
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "sample" ),
					new byte[] { 7 }
				)
			),
			path => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void MalformedDatabaseIsMappedToDatabaseFormat() {
		byte[] database = CreateCatalogStore( "sample" );
		database[12] = 0;

		WithDatabase(
			database,
			path => {
				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => new BerkeleyDbTerminalCatalogReader( path ).Read()
					);

				Assert.IsType<InvalidDataException>( error.InnerException );
			}
		);
	}

	[Fact]
	public void CompiledFailureRetainsCompiledFormatException() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "sample|test terminal" );
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "sample" ),
					PrependMarker( storageKey, 2 )
				),
				(
					storageKey,
					new byte[] { 0, 1, 2 }
				)
			),
			path => Assert.Throws<CompiledTermInfoFormatException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void UndeclaredPublicationRetainsIdentityFailure() {
		byte[] storageKey = Encoding.UTF8.GetBytes( "storage" );
		WithDatabase(
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( "sample" ),
					PrependMarker( storageKey, 2 )
				),
				(
					storageKey,
					PrependMarker(
						CreateCompiledEntry( "other", [] )
					)
				)
			),
			path => Assert.Throws<InvalidDataException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read()
			)
		);
	}

	[Fact]
	public void IoFailureRetainsFileNotFoundException() {
		string path = Path.Combine(
			Path.GetTempPath(),
			Guid.NewGuid().ToString( "N" ) + ".db"
		);
		Assert.Throws<FileNotFoundException>(
			() => new BerkeleyDbTerminalCatalogReader( path ).Read()
		);
	}

	[Fact]
	public void ReadHonorsPreCanceledToken() {
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		WithDatabase(
			CreateCatalogStore( "sample" ),
			path => Assert.Throws<OperationCanceledException>(
				() => new BerkeleyDbTerminalCatalogReader( path ).Read(
					cancellation.Token
				)
			)
		);
	}

	[Fact]
	public void RepeatedReadsAcquireFreshSnapshots() {
		WithDatabase(
			CreateCatalogStore( "first" ),
			path => {
				BerkeleyDbTerminalCatalogReader reader = new( path );
				BerkeleyDbTerminalCatalogEntry first =
					Assert.Single( reader.Read() );

				File.WriteAllBytes(
					path,
					CreateCatalogStore( "second" )
				);
				BerkeleyDbTerminalCatalogEntry second =
					Assert.Single( reader.Read() );

				Assert.Equal( "first", first.Name );
				Assert.Equal( "second", second.Name );
				Assert.NotSame( first.Terminal, second.Terminal );
			}
		);
	}

	[Fact]
	public void ConcurrentReadersKeepIndependentSnapshots() {
		WithTwoDatabases(
			CreateCatalogStore( "first" ),
			CreateCatalogStore( "second" ),
			( firstPath, secondPath ) => {
				BerkeleyDbTerminalCatalogReader firstReader =
					new( firstPath );
				BerkeleyDbTerminalCatalogReader secondReader =
					new( secondPath );

				Task<IReadOnlyList<BerkeleyDbTerminalCatalogEntry>> first =
					Task.Run( () => firstReader.Read() );
				Task<IReadOnlyList<BerkeleyDbTerminalCatalogEntry>> second =
					Task.Run( () => secondReader.Read() );
				Task.WaitAll( first, second );

				Assert.Equal( "first", Assert.Single( first.Result ).Name );
				Assert.Equal( "second", Assert.Single( second.Result ).Name );
			}
		);
	}

	[Fact]
	public void LogicalOrderingDoesNotDependOnPhysicalPageOrder() {
		( byte[] Key, byte[] Value )[] records =
			CreateCatalogRecords(
				"sample",
				"z-alias",
				"a-alias"
			);
		WithTwoDatabases(
			CreateDatabase( records ),
			CreateDatabase( records.Reverse().ToArray() ),
			( firstPath, secondPath ) => Assert.Equal(
				new BerkeleyDbTerminalCatalogReader( firstPath )
					.Read()
					.Select( entry => ( entry.Name, entry.Kind ) ),
				new BerkeleyDbTerminalCatalogReader( secondPath )
					.Read()
					.Select( entry => ( entry.Name, entry.Kind ) )
			)
		);
	}


	[Fact]
	public void LowestByteKeyLogicalFailureWinsAcrossPagePermutations() {
		( byte[] Key, byte[] Value )[] records = [
			(
				Encoding.UTF8.GetBytes( "z-bad" ),
				new byte[] { 7 }
			),
			(
				Encoding.UTF8.GetBytes( "a-bad" ),
				PrependMarker(
					Encoding.UTF8.GetBytes( "missing" ),
					2
				)
			),
		];
		WithTwoDatabases(
			CreateDatabase( records ),
			CreateDatabase( records.Reverse().ToArray() ),
			( firstPath, secondPath ) => {
				BerkeleyDbDatabaseFormatException first =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => new BerkeleyDbTerminalCatalogReader(
							firstPath
						).Read()
					);
				BerkeleyDbDatabaseFormatException second =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => new BerkeleyDbTerminalCatalogReader(
							secondPath
						).Read()
					);

				Assert.Contains(
					"missing key",
					first.Message,
					StringComparison.Ordinal
				);
				Assert.Equal( first.Message, second.Message );
			}
		);
	}

	[Fact]
	public void RecordLimitAcceptsExactBoundaryAndRejectsNextRecord() {
		byte[] database = CreateCatalogStore( "sample" );
		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalCatalogReaderOptions exact =
					new( maximumRecordCount: 2 );
				Assert.Single(
					new BerkeleyDbTerminalCatalogReader(
						path,
						exact
					).Read()
				);

				BerkeleyDbTerminalCatalogReaderOptions tooSmall =
					new( maximumRecordCount: 1 );
				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => new BerkeleyDbTerminalCatalogReader(
						path,
						tooSmall
					).Read()
				);
			}
		);
	}

	[Fact]
	public void DatabaseLimitAcceptsExactLengthAndRejectsNextByte() {
		byte[] database = CreateCatalogStore( "sample" );
		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalCatalogReaderOptions exact =
					new( maximumDatabaseSize: database.Length );
				Assert.Single(
					new BerkeleyDbTerminalCatalogReader(
						path,
						exact
					).Read()
				);

				BerkeleyDbTerminalCatalogReaderOptions tooSmall =
					new( maximumDatabaseSize: database.Length - 1 );
				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => new BerkeleyDbTerminalCatalogReader(
						path,
						tooSmall
					).Read()
				);
			}
		);
	}

	[Fact]
	public void ParserLimitPlusMarkerIsTheExactStoredItemBoundary() {
		byte[] entry = CreateCompiledEntry( "sample", [] );
		byte[] storageKey =
			Encoding.UTF8.GetBytes( "sample|test terminal" );
		byte[] database = CreateDatabase(
			(
				Encoding.UTF8.GetBytes( "sample" ),
				PrependMarker( storageKey, 2 )
			),
			(
				storageKey,
				PrependMarker( entry )
			)
		);
		WithDatabase(
			database,
			path => {
				BerkeleyDbTerminalCatalogReaderOptions exact =
					new(
						new CompiledTermInfoParserOptions(
							entry.Length
						)
					);
				Assert.Single(
					new BerkeleyDbTerminalCatalogReader(
						path,
						exact
					).Read()
				);

				BerkeleyDbTerminalCatalogReaderOptions tooSmall =
					new(
						new CompiledTermInfoParserOptions(
							entry.Length - 1
						)
					);
				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => new BerkeleyDbTerminalCatalogReader(
						path,
						tooSmall
					).Read()
				);
			}
		);
	}

	[Fact]
	public void MaximumSupportedHopLimitIsAccepted() {
		BerkeleyDbTerminalCatalogReaderOptions options =
			new(
				maximumIndexHops:
					BerkeleyDbTerminalDescriptionProviderOptions
						.MaximumSupportedIndexHops
			);

		Assert.Equal(
			BerkeleyDbTerminalDescriptionProviderOptions
				.MaximumSupportedIndexHops,
			options.MaximumIndexHops
		);
	}

	[Fact]
	public void EmptyDatabaseReturnsAnEmptyImmutableSnapshot() {
		WithDatabase(
			CreateDatabase(),
			path => {
				IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
					new BerkeleyDbTerminalCatalogReader( path ).Read();

				Assert.Empty( entries );
				IList<BerkeleyDbTerminalCatalogEntry> list =
					Assert.IsAssignableFrom<IList<BerkeleyDbTerminalCatalogEntry>>(
						entries
					);
				Assert.True( list.IsReadOnly );
			}
		);
	}

	[Fact]
	public void ConcurrentReadsUseIndependentLimitSnapshots() {
		WithDatabase(
			CreateCatalogStore( "sample", "sample-alias" ),
			path => {
				BerkeleyDbTerminalCatalogReader accepted =
					new(
						path,
						new BerkeleyDbTerminalCatalogReaderOptions(
							maximumRecordCount: 3
						)
					);
				BerkeleyDbTerminalCatalogReader rejected =
					new(
						path,
						new BerkeleyDbTerminalCatalogReaderOptions(
							maximumRecordCount: 2
						)
					);

				Task<IReadOnlyList<BerkeleyDbTerminalCatalogEntry>> success =
					Task.Run( () => accepted.Read() );
				Task<Exception?> failure =
					Task.Run(
						() => Record.Exception( () => rejected.Read() )
					);
				Task.WaitAll( success, failure );

				Assert.Equal( 2, success.Result.Count );
				Assert.IsType<BerkeleyDbDatabaseFormatException>(
					failure.Result
				);
			}
		);
	}

	[Fact]
	public void FileHandleIsReleasedAfterSuccessAndFailureFamilies() {
		WithDatabase(
			CreateCatalogStore( "sample" ),
			path => {
				new BerkeleyDbTerminalCatalogReader( path ).Read();
				AssertFileUnlocked( path );

				byte[] malformedDatabase =
					CreateCatalogStore( "sample" );
				malformedDatabase[12] = 0;
				File.WriteAllBytes( path, malformedDatabase );
				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => new BerkeleyDbTerminalCatalogReader( path ).Read()
				);
				AssertFileUnlocked( path );

				File.WriteAllBytes(
					path,
					CreateDatabase(
						(
							Encoding.UTF8.GetBytes( "orphan" ),
							new byte[] { 0, 1, 2 }
						)
					)
				);
				Assert.Throws<CompiledTermInfoFormatException>(
					() => new BerkeleyDbTerminalCatalogReader( path ).Read()
				);
				AssertFileUnlocked( path );

				byte[] storageKey =
					Encoding.UTF8.GetBytes( "other|test terminal" );
				File.WriteAllBytes(
					path,
					CreateDatabase(
						(
							Encoding.UTF8.GetBytes( "sample" ),
							PrependMarker( storageKey, 2 )
						),
						(
							storageKey,
							PrependMarker(
								CreateCompiledEntry( "other", [] )
							)
						)
					)
				);
				Assert.Throws<InvalidDataException>(
					() => new BerkeleyDbTerminalCatalogReader( path ).Read()
				);
				AssertFileUnlocked( path );
			}
		);
	}


	private static void AssertFileUnlocked( string path ) {
		using FileStream stream = new FileStream(
			path,
			FileMode.Open,
			FileAccess.ReadWrite,
			FileShare.None
		);
		Assert.True( stream.CanRead );
		Assert.True( stream.CanWrite );
	}


	[Fact]
	public void CatalogReadObservesCancellationBetweenLogicalRecords() {
		using CancellationTokenSource cancellation = new();
		BerkeleyDbHashRecord[] records = [
			new BerkeleyDbHashRecord(
				Encoding.UTF8.GetBytes( "a-storage" ),
				PrependMarker(
					CreateCompiledEntry( "a", [] )
				)
			),
			new BerkeleyDbHashRecord(
				Encoding.UTF8.GetBytes( "b-storage" ),
				PrependMarker(
					CreateCompiledEntry( "b", [] )
				)
			),
		];
		var cancelingRecords =
			new CancelingRecordList(
				records,
				cancellation
			);

		Assert.Throws<OperationCanceledException>(
			() => NcursesCatalogReader.Read(
				cancelingRecords,
				new CompiledTermInfoParserOptions(),
				maximumIndexHops: 16,
				cancellation.Token
			)
		);
	}


	private sealed class CancelingRecordList
		: IReadOnlyList<BerkeleyDbHashRecord> {
		private readonly CancellationTokenSource _cancellation;
		private readonly BerkeleyDbHashRecord[] _records;
		private int _enumerationCount;

		internal CancelingRecordList(
			BerkeleyDbHashRecord[] records,
			CancellationTokenSource cancellation
		) {
			_records = records;
			_cancellation = cancellation;
		}

		public int Count =>
			_records.Length;

		public BerkeleyDbHashRecord this[int index] =>
			_records[index];

		public IEnumerator<BerkeleyDbHashRecord> GetEnumerator() {
			int enumeration =
				Interlocked.Increment( ref _enumerationCount );
			return Enumerate( enumeration ).GetEnumerator();
		}

		System.Collections.IEnumerator
			System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		private IEnumerable<BerkeleyDbHashRecord> Enumerate(
			int enumeration
		) {
			for ( int index = 0; index < _records.Length; index++ ) {
				if ( enumeration == 2 && index == 1 ) {
					_cancellation.Cancel();
				}
				yield return _records[index];
			}
		}
	}

	private static byte[] CreateCatalogStore(
		string canonical,
		params string[] aliases
	) {
		return CreateDatabase(
			CreateCatalogRecords( canonical, aliases )
		);
	}

	private static ( byte[] Key, byte[] Value )[] CreateCatalogRecords(
		string canonical,
		params string[] aliases
	) {
		var names = new List<string> { canonical };
		names.AddRange( aliases );
		byte[] storageKey =
			Encoding.UTF8.GetBytes( string.Join( "|", names ) + "|test terminal" );
		var records =
			new List<( byte[] Key, byte[] Value )>();

		foreach ( string name in names ) {
			records.Add(
				(
					Encoding.UTF8.GetBytes( name ),
					PrependMarker( storageKey, 2 )
				)
			);
		}
		records.Add(
			(
				storageKey,
				PrependMarker(
					CreateCompiledEntry( canonical, aliases )
				)
			)
		);
		return records.ToArray();
	}

	private static byte[] CreateCompiledEntry(
		string canonical,
		string[] aliases,
		string description = "test terminal"
	) {
		string identity = ( aliases.Length == 0 )
			? canonical + "|" + description + "\0"
			: canonical
				+ "|"
				+ string.Join( "|", aliases )
				+ "|"
				+ description
				+ "\0"
		;
		byte[] names = Encoding.Latin1.GetBytes( identity );
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

	private static byte[] PrependMarker(
		byte[] bytes,
		byte marker = 0
	) {
		byte[] value = new byte[bytes.Length + 1];
		value[0] = marker;
		bytes.CopyTo( value.AsSpan( 1 ) );
		return value;
	}

	private static byte[] CreateDatabase(
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
			checked( (uint)records.Length )
		);

		for ( int index = 0; index < records.Length; index++ ) {
			( byte[] key, byte[] value ) = records[index];
			Span<byte> page =
				database.AsSpan( 512 * ( index + 1 ), 512 );
			BinaryPrimitives.WriteUInt32LittleEndian(
				page.Slice( 8, 4 ),
				checked( (uint)( index + 1 ) )
			);
			page[25] = 13;
			ushort keyOffset =
				checked( (ushort)( 511 - key.Length ) );
			ushort valueOffset =
				checked( (ushort)( keyOffset - value.Length - 1 ) );
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

	private static void WithDatabase(
		byte[] database,
		Action<string> assertion
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			assertion( path );
		} finally {
			File.Delete( path );
		}
	}

	private static void WithTwoDatabases(
		byte[] first,
		byte[] second,
		Action<string, string> assertion
	) {
		string firstPath = Path.GetTempFileName();
		string secondPath = Path.GetTempFileName();
		try {
			File.WriteAllBytes( firstPath, first );
			File.WriteAllBytes( secondPath, second );
			assertion( firstPath, secondPath );
		} finally {
			File.Delete( firstPath );
			File.Delete( secondPath );
		}
	}
}
