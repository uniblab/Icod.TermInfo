/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates deterministic HW02 Hash-v9 inline image construction.
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
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw02WriterImageTests {
	[Fact]
	public void PreparedPublicationPreservesExactCompiledNamesSection() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"hw02-primary",
			"HW02 caf\u00E9 description",
			"hw02-alias"
		);
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw02-primary",
			[ "hw02-alias" ],
			compiled
		);

		Array prepared = InvokePreparePublications( [ entry ] );
		object publication = Assert.Single( prepared.Cast<object>() );
		PropertyInfo? property = publication.GetType().GetProperty(
			"StorageKey",
			BindingFlags.Instance
				| BindingFlags.Public
				| BindingFlags.NonPublic
		);

		Assert.NotNull( property );
		Assert.Equal(
			Encoding.Latin1.GetBytes(
				"hw02-primary|hw02-alias|HW02 caf\u00E9 description"
			),
			Assert.IsType<byte[]>( property!.GetValue( publication ) )
		);
	}

	[Fact]
	public void RecordPlannerEmitsExactNcursesEnvelopes() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"hw02-primary",
			"HW02 record planner",
			"hw02-alias"
		);
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw02-primary",
			[ "hw02-alias" ],
			compiled
		);
		BerkeleyDbTerminalDatabaseWriter.PreparedPublication[] prepared =
			BerkeleyDbTerminalDatabaseWriter.PreparePublications(
				[ entry ],
				new BerkeleyDbTerminalDatabaseWriterOptions(),
				CancellationToken.None
			);

		IReadOnlyList<BerkeleyDbHashRecord> records =
			InvokeCreateRecords( prepared, CancellationToken.None );
		byte[] storageKey = Encoding.Latin1.GetBytes(
			"hw02-primary|hw02-alias|HW02 record planner"
		);

		Assert.Equal( 3, records.Count );
		AssertRecord(
			records,
			Encoding.UTF8.GetBytes( "hw02-primary" ),
			PrependExpectedMarker( storageKey, 2 )
		);
		AssertRecord(
			records,
			Encoding.UTF8.GetBytes( "hw02-alias" ),
			PrependExpectedMarker( storageKey, 2 )
		);
		AssertRecord(
			records,
			storageKey,
			PrependExpectedMarker( compiled, 0 )
		);
	}

	[Fact]
	public void WriterComparerPlacesLongerKeyBeforeItsExactPrefix() {
		IComparer<byte[]> comparer = InvokeWriterComparer();
		byte[][] keys = [
			Encoding.ASCII.GetBytes( "b" ),
			Encoding.ASCII.GetBytes( "a" ),
			Encoding.ASCII.GetBytes( "aa" ),
		];

		Array.Sort( keys, comparer );

		Assert.Equal(
			new[] { "aa", "a", "b" },
			keys.Select( Encoding.ASCII.GetString ).ToArray()
		);
	}

	[Fact]
	public void RecordPlannerRejectsDuplicateExactByteKeys() {
		var first = new BerkeleyDbTerminalDatabaseWriter.PreparedPublication(
			new BerkeleyDbTerminalDatabaseWriter.PreparedIdentity(
				"first",
				[ 0x61 ]
			),
			Array.Empty<BerkeleyDbTerminalDatabaseWriter.PreparedIdentity>(),
			[ 0x70, 0x31 ],
			[ 0x01 ]
		);
		var second = new BerkeleyDbTerminalDatabaseWriter.PreparedPublication(
			new BerkeleyDbTerminalDatabaseWriter.PreparedIdentity(
				"second",
				[ 0x61 ]
			),
			Array.Empty<BerkeleyDbTerminalDatabaseWriter.PreparedIdentity>(),
			[ 0x70, 0x32 ],
			[ 0x02 ]
		);

		Assert.Throws<InvalidOperationException>(
			() => InvokeCreateRecords(
				[ first, second ],
				CancellationToken.None
			)
		);
	}

	[Fact]
	public void RecordPlannerObservesPreCancelledToken() {
		var publication =
			new BerkeleyDbTerminalDatabaseWriter.PreparedPublication(
				new BerkeleyDbTerminalDatabaseWriter.PreparedIdentity(
					"cancelled",
					[ 0x63 ]
				),
				Array.Empty<BerkeleyDbTerminalDatabaseWriter.PreparedIdentity>(),
				[ 0x73 ],
				[ 0x01 ]
			);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => InvokeCreateRecords(
				[ publication ],
				cancellation.Token
			)
		);
	}

	[Fact]
	public void ImageBuilderWritesExactMetadataAndInlineBuckets() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"hw02-primary",
			"HW02 inline layout",
			"hw02-alias"
		);
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw02-primary",
			[ "hw02-alias" ],
			compiled
		);
		BerkeleyDbTerminalDatabaseWriter.PreparedPublication[] prepared =
			BerkeleyDbTerminalDatabaseWriter.PreparePublications(
				[ entry ],
				new BerkeleyDbTerminalDatabaseWriterOptions(),
				CancellationToken.None
			);
		IReadOnlyList<BerkeleyDbHashRecord> records =
			InvokeCreateRecords( prepared, CancellationToken.None );

		byte[] image = BerkeleyDbHashV9ImageBuilder.Build(
			records,
			maximumDatabaseSize: 3 * 4096,
			CancellationToken.None
		);

		Assert.Equal( 3 * 4096, image.Length );
		Assert.Equal( 1U, ReadUInt32( image, 4 ) );
		Assert.Equal( 0x00061561U, ReadUInt32( image, 12 ) );
		Assert.Equal( 9U, ReadUInt32( image, 16 ) );
		Assert.Equal( 4096U, ReadUInt32( image, 20 ) );
		Assert.Equal( (byte)8, image[25] );
		Assert.Equal( 2U, ReadUInt32( image, 32 ) );
		Assert.Equal( 1U, ReadUInt32( image, 72 ) );
		Assert.Equal( 1U, ReadUInt32( image, 76 ) );
		Assert.Equal( 0U, ReadUInt32( image, 80 ) );
		Assert.Equal( 0U, ReadUInt32( image, 84 ) );
		Assert.Equal( 3U, ReadUInt32( image, 88 ) );
		Assert.Equal( 0x5E688DD1U, ReadUInt32( image, 92 ) );
		Assert.Equal( 1U, ReadUInt32( image, 96 ) );
		Assert.Equal( 1U, ReadUInt32( image, 100 ) );

		AssertInlineBucket( image, 1, records );
		AssertInlineBucket( image, 2, records );
	}

	private static Array InvokePreparePublications(
		BerkeleyDbTerminalDatabaseEntry[] entries
	) {
		MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(
			typeof( BerkeleyDbTerminalDatabaseWriter ).GetMethod(
				"PreparePublications",
				BindingFlags.Static
					| BindingFlags.Public
					| BindingFlags.NonPublic
			)
		);

		try {
			return Assert.IsAssignableFrom<Array>(
				method.Invoke(
					null,
					new object[] {
						entries,
						new BerkeleyDbTerminalDatabaseWriterOptions(),
						CancellationToken.None,
					}
				)
			);
		} catch (
			TargetInvocationException exception
		) when ( exception.InnerException is not null ) {
			ExceptionDispatchInfo.Capture( exception.InnerException ).Throw();
			throw;
		}
	}

	private static IReadOnlyList<BerkeleyDbHashRecord> InvokeCreateRecords(
		IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
		CancellationToken cancellationToken
	) {
		Type? plannerType = typeof( BerkeleyDbTerminalDatabaseWriter )
			.Assembly
			.GetType(
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbNcursesRecordPlanner",
				throwOnError: false,
				ignoreCase: false
			);
		Assert.NotNull( plannerType );
		MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(
			plannerType!.GetMethod(
				"CreateRecords",
				BindingFlags.Static
					| BindingFlags.Public
					| BindingFlags.NonPublic
			)
		);

		try {
			return Assert.IsAssignableFrom<IReadOnlyList<BerkeleyDbHashRecord>>(
				method.Invoke(
					null,
					new object[] { publications, cancellationToken }
				)
			);
		} catch (
			TargetInvocationException exception
		) when ( exception.InnerException is not null ) {
			ExceptionDispatchInfo.Capture( exception.InnerException ).Throw();
			throw;
		}
	}

	private static IComparer<byte[]> InvokeWriterComparer() {
		Type? comparerType = typeof( BerkeleyDbTerminalDatabaseWriter )
			.Assembly
			.GetType(
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbHashV9WriterKeyComparer",
				throwOnError: false,
				ignoreCase: false
			);
		Assert.NotNull( comparerType );
		PropertyInfo property = Assert.IsAssignableFrom<PropertyInfo>(
			comparerType!.GetProperty(
				"Instance",
				BindingFlags.Static
					| BindingFlags.Public
					| BindingFlags.NonPublic
			)
		);
		return Assert.IsAssignableFrom<IComparer<byte[]>>(
			property.GetValue( null )
		);
	}

	private static void AssertInlineBucket(
		byte[] image,
		int pageNumber,
		IReadOnlyList<BerkeleyDbHashRecord> records
	) {
		const int pageSize = 4096;
		int pageStart = checked( pageNumber * pageSize );
		BerkeleyDbHashRecord[] expected = records
			.Where(
				record =>
					( ExpectedHash( record.Key.Span ) & 1 )
						== (uint)( pageNumber - 1 )
			)
			.ToArray();

		Assert.Equal( 1U, ReadUInt32( image, pageStart + 4 ) );
		Assert.Equal(
			checked( (uint)pageNumber ),
			ReadUInt32( image, pageStart + 8 )
		);
		Assert.Equal( (byte)13, image[pageStart + 25] );
		Assert.Equal(
			checked( (ushort)( expected.Length * 2 ) ),
			ReadUInt16( image, pageStart + 20 )
		);

		int expectedHighFree = pageSize - expected.Sum(
			static record =>
				checked( record.Key.Length + record.Value.Length + 2 )
		);
		Assert.Equal(
			checked( (ushort)expectedHighFree ),
			ReadUInt16( image, pageStart + 22 )
		);

		int itemIndex = 0;
		foreach ( BerkeleyDbHashRecord record in expected ) {
			AssertInlineItem(
				image,
				pageStart,
				itemIndex++,
				record.Key.Span
			);
			AssertInlineItem(
				image,
				pageStart,
				itemIndex++,
				record.Value.Span
			);
		}
	}

	private static void AssertInlineItem(
		byte[] image,
		int pageStart,
		int itemIndex,
		ReadOnlySpan<byte> expectedPayload
	) {
		ushort itemOffset = ReadUInt16(
			image,
			checked( pageStart + 26 + ( itemIndex * 2 ) )
		);
		Assert.Equal( (byte)1, image[pageStart + itemOffset] );
		Assert.True(
			image.AsSpan(
				checked( pageStart + itemOffset + 1 ),
				expectedPayload.Length
			).SequenceEqual( expectedPayload )
		);
	}

	private static uint ExpectedHash( ReadOnlySpan<byte> key ) {
		uint result = 0;
		foreach ( byte value in key ) {
			result = unchecked( result * 16777619 );
			result ^= value;
		}
		return result;
	}

	private static ushort ReadUInt16( byte[] bytes, int offset ) =>
		BinaryPrimitives.ReadUInt16LittleEndian(
			bytes.AsSpan( offset, sizeof( ushort ) )
		)
	;

	private static uint ReadUInt32( byte[] bytes, int offset ) =>
		BinaryPrimitives.ReadUInt32LittleEndian(
			bytes.AsSpan( offset, sizeof( uint ) )
		)
	;

	private static void AssertRecord(
		IEnumerable<BerkeleyDbHashRecord> records,
		byte[] key,
		byte[] value
	) {
		BerkeleyDbHashRecord record = Assert.Single(
			records,
			item => item.Key.Span.SequenceEqual( key )
		);
		Assert.True( record.Value.Span.SequenceEqual( value ) );
	}

	private static byte[] PrependExpectedMarker(
		byte[] payload,
		byte marker
	) {
		byte[] result = new byte[checked( payload.Length + 1 )];
		result[0] = marker;
		payload.CopyTo( result.AsSpan( 1 ) );
		return result;
	}
}
