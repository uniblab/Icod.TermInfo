/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HW04 public Hash-v9 publication engine.
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
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw04PublicWriterTests {
	[Fact]
	public void WriterCreatesCompleteReaderVisibleDatabaseAtCallerPath() {
		const string canonical = "hw04-primary";
		const string alias = "hw04-alias";
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			canonical,
			"HW04 public writer",
			alias
		);

		WithDirectory(
			directory => {
				string destination = Path.Combine( directory, "terminfo.db" );
				BerkeleyDbTerminalDatabaseWriter.Write(
					destination,
					[ new BerkeleyDbTerminalDatabaseEntry( canonical, [ alias ], compiled ) ]
				);

				byte[] image = File.ReadAllBytes( destination );
				Assert.Equal( 3 * BerkeleyDbHashV9ImageBuilder.PageSize, image.Length );
				Assert.True(
					NcursesRecordReader.TryReadCompiledEntry(
						destination,
						Encoding.UTF8.GetBytes( canonical ),
						out byte[] canonicalData
					)
				);
				Assert.Equal( compiled, canonicalData );
				Assert.True(
					NcursesRecordReader.TryReadCompiledEntry(
						destination,
						Encoding.UTF8.GetBytes( alias ),
						out byte[] aliasData
					)
				);
				Assert.Equal( compiled, aliasData );

				var provider = new BerkeleyDbTerminalDescriptionProvider( destination );
				Assert.True( provider.TryLoad( alias, out TerminalDescription? terminal ) );
				Assert.NotNull( terminal );
				Assert.Equal( canonical, terminal!.Name );
				Assert.Equal( alias, Assert.Single( terminal.Aliases ) );
			}
		);
	}

	[Fact]
	public void WriterOutputIsDeterministicAcrossPublicationOrder() {
		BerkeleyDbTerminalDatabaseEntry first = CreateEntry(
			"hw04-first",
			"HW04 first"
		);
		BerkeleyDbTerminalDatabaseEntry second = CreateEntry(
			"hw04-second",
			"HW04 second"
		);

		WithDirectory(
			directory => {
				string forward = Path.Combine( directory, "forward.db" );
				string reversed = Path.Combine( directory, "reversed.db" );
				BerkeleyDbTerminalDatabaseWriter.Write(
					forward,
					[ first, second ]
				);
				BerkeleyDbTerminalDatabaseWriter.Write(
					reversed,
					[ second, first ]
				);

				Assert.Equal(
					File.ReadAllBytes( forward ),
					File.ReadAllBytes( reversed )
				);
			}
		);
	}

	[Fact]
	public void WriterRefusesExistingDestinationByDefault() {
		byte[] original = [ 0x11, 0x22, 0x33 ];
		WithDirectory(
			directory => {
				string destination = Path.Combine( directory, "existing.db" );
				File.WriteAllBytes( destination, original );

				Assert.Throws<IOException>(
					() => BerkeleyDbTerminalDatabaseWriter.Write(
						destination,
						[ CreateEntry( "hw04-refuse", "HW04 refuse" ) ]
					)
				);
				Assert.Equal( original, File.ReadAllBytes( destination ) );
			}
		);
	}

	[Fact]
	public void WriterReplacesExistingDestinationWhenExplicitlyAllowed() {
		const string canonical = "hw04-replace";
		BerkeleyDbTerminalDatabaseEntry entry = CreateEntry(
			canonical,
			"HW04 replace"
		);
		WithDirectory(
			directory => {
				string destination = Path.Combine( directory, "existing.db" );
				File.WriteAllBytes( destination, [ 0x44, 0x55 ] );

				BerkeleyDbTerminalDatabaseWriter.Write(
					destination,
					[ entry ],
					new BerkeleyDbTerminalDatabaseWriterOptions(
						overwriteExisting: true
					)
				);

				Assert.True(
					new BerkeleyDbTerminalDescriptionProvider( destination )
						.TryLoad( canonical, out TerminalDescription? terminal )
				);
				Assert.Equal( canonical, terminal!.Name );
			}
		);
	}

	[Fact]
	public void ImageConstructionFailureLeavesExistingDestinationUnchanged() {
		byte[] original = [ 0x66, 0x77, 0x88 ];
		WithDirectory(
			directory => {
				string destination = Path.Combine( directory, "existing.db" );
				File.WriteAllBytes( destination, original );

				Assert.Throws<InvalidOperationException>(
					() => BerkeleyDbTerminalDatabaseWriter.Write(
						destination,
						[ CreateEntry( "hw04-bounded", "HW04 bounded" ) ],
						new BerkeleyDbTerminalDatabaseWriterOptions(
							maximumDatabaseSize:
								BerkeleyDbHashV9ImageBuilder.PageSize,
							overwriteExisting: true
						)
					)
				);
				Assert.Equal( original, File.ReadAllBytes( destination ) );
			}
		);
	}

	private static BerkeleyDbTerminalDatabaseEntry CreateEntry(
		string canonical,
		string description
	) => new(
		canonical,
		[],
		Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			canonical,
			description
		)
	);

	private static void WithDirectory( Action<string> action ) {
		string directory = Path.Combine(
			Path.GetTempPath(),
			"icod-terminfo-hw04-" + Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( directory );
		try {
			action( directory );
		} finally {
			Directory.Delete( directory, recursive: true );
		}
	}
}
