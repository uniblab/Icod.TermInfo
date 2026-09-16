/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the bounded HDB07C terminal-name encoding policy.
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

using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb07cEncodingCompatibilityTests {
	private const string Canonical = "hdb07c-caf\u00E9";
	private const string Alias = "hdb07c-ali\u00E9";
	private const int PageSize = 512;

	[Theory]
	[InlineData( Canonical )]
	[InlineData( Alias )]
	public void ProviderLoadsExactLatin1CanonicalAndAliasAfterUtf8Miss(
		string requestedName
	) {
		WithDatabase(
			CreateLatin1Store(),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );

				Assert.True(
					provider.TryLoad(
						requestedName,
						out TerminalDescription? terminal
					)
				);
				Assert.Equal( Canonical, terminal!.Name );
				Assert.Contains( Alias, terminal.Aliases );
			}
		);
	}

	[Theory]
	[InlineData( Canonical )]
	[InlineData( Alias )]
	public void SystemProviderLoadsExactLatin1CanonicalAndAlias(
		string requestedName
	) {
		WithDatabase(
			CreateLatin1Store(),
			path => {
				BerkeleyDbSystemTerminalDescriptionProvider provider =
					CreateSystemProvider( path );

				Assert.True(
					provider.TryLoad(
						requestedName,
						out TerminalDescription? terminal
					)
				);
				Assert.Equal( Canonical, terminal!.Name );
				Assert.Contains( Alias, terminal.Aliases );
			}
		);
	}

	[Fact]
	public void ProviderDoesNotUseLatin1AfterFoundUtf8EnvelopeFailure() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			Canonical,
			"HDB07C encoding fixture",
			Alias
		);
		WithDatabase(
			CreateDatabase(
				CreateRecord(
					Encoding.UTF8.GetBytes( Canonical ),
					[ 7 ]
				),
				CreateRecord(
					Encoding.Latin1.GetBytes( Canonical ),
					Hdb07HashV9FixtureBuilder.NcursesData( compiled )
				)
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );

				BerkeleyDbDatabaseFormatException error =
					Assert.Throws<BerkeleyDbDatabaseFormatException>(
						() => provider.TryLoad( Canonical, out _ )
					);
				InvalidDataException inner =
					Assert.IsType<InvalidDataException>( error.InnerException );
				Assert.Equal(
					"Ncurses hashed-term marker 7 is not supported.",
					inner.Message
				);
			}
		);
	}

	[Fact]
	public void ProviderReturnsCleanMissWhenBothPermittedCandidatesAreAbsent() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"hdb07c-other",
			"HDB07C other fixture"
		);
		WithDatabase(
			CreateDatabase(
				CreateRecord(
					Encoding.UTF8.GetBytes( "hdb07c-other" ),
					Hdb07HashV9FixtureBuilder.NcursesData( compiled )
				)
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );

				Assert.False( provider.TryLoad( Canonical, out TerminalDescription? terminal ) );
				Assert.Null( terminal );
			}
		);
	}

	[Fact]
	public void NonLatin1NameUsesOnlyUtf8Candidate() {
		const string name = "hdb07c-\u0100";
		WithDatabase(
			CreateDatabase(
				CreateRecord(
					Encoding.ASCII.GetBytes( "hdb07c-?" ),
					[ 7 ]
				)
			),
			path => {
				BerkeleyDbTerminalDescriptionProvider provider = new( path );

				Assert.False( provider.TryLoad( name, out TerminalDescription? terminal ) );
				Assert.Null( terminal );
			}
		);
	}

	private static byte[] CreateLatin1Store() {
		byte[] canonical = Encoding.Latin1.GetBytes( Canonical );
		byte[] alias = Encoding.Latin1.GetBytes( Alias );
		byte[] storageKey = Encoding.Latin1.GetBytes(
			Canonical + "|" + Alias
		);
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			Canonical,
			"HDB07C encoding fixture",
			Alias
		);

		return CreateDatabase(
			CreateRecord(
				canonical,
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateRecord(
				alias,
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateRecord(
				storageKey,
				Hdb07HashV9FixtureBuilder.NcursesData( compiled )
			)
		);
	}

	private static BerkeleyDbSystemTerminalDescriptionProvider
		CreateSystemProvider( string path ) {
		SystemTerminalDiscoverySnapshot snapshot = new(
			termInfo: path,
			termInfoDirs: null,
			homeDirectory: null,
			currentDirectory: Path.GetDirectoryName( path )!,
			platform: TerminalHostPlatform.Linux
		);
		return new BerkeleyDbSystemTerminalDescriptionProvider(
			new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false
			),
			snapshot,
			Array.Empty<string>()
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
}
