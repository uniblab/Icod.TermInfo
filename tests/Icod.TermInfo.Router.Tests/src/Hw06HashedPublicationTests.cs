using System.Text;
using Icod.TermInfo.BerkeleyDb;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class Hw06HashedPublicationTests {
	[Fact]
	public async Task RoutedTicPublishesSameHashedBytesAsDirectCommand() {
		string root = System.IO.Path.Combine( System.IO.Path.GetTempPath(), "icod-hw06-route-" + Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( root );
		try {
			string direct = System.IO.Path.Combine( root, "direct" );
			string routed = System.IO.Path.Combine( root, "routed" );
			const string source = "hw06-main|hw06-alias|HW06 routed terminal,am,cols#80,\n";
			using var directInput = new MemoryStream( Encoding.UTF8.GetBytes( source ) );
			using var routedInput = new MemoryStream( Encoding.UTF8.GetBytes( source ) );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();
			Assert.Equal(
				0,
				await Tic.Command.RunAsync( [ "--database-format", "hashed", "-o", direct, "-" ], directInput, stdout, stderr )
			);
			Assert.Equal(
				0,
				await Command.RunAsync( [ "tic", "--database-format", "hashed", "-o", routed, "-" ], routedInput, stdout, stderr )
			);
			Assert.Empty( stdout.ToArray() );
			Assert.Empty( stderr.ToArray() );
			Assert.Equal( File.ReadAllBytes( direct ), File.ReadAllBytes( routed ) );
			Assert.True( new BerkeleyDbTerminalDescriptionProvider( routed ).TryLoad( "hw06-alias", out var entry ) );
			Assert.Equal( "hw06-main", entry!.Name );
		} finally {
			Directory.Delete( root, recursive: true );
		}
	}
}
