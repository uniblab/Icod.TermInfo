using Icod.TermInfo.BerkeleyDb.PackageVerifier;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw08ApiFreezeTests {
	[Fact]
	public void RemovingOnlyWriterTypesReconstructsTheComplete115Reader() {
		BerkeleyDbApiFreeze.VerifyReaderReconstruction( Read( "1.16.0" ), Read( "1.15.0" ) );
		string crlf = Read( "1.16.0" ).Replace( "\r\n", "\n" ).Replace( "\n", "\r\n" );
		BerkeleyDbApiFreeze.VerifyReaderReconstruction( crlf, Read( "1.15.0" ) );
	}

	[Theory]
	[InlineData( "System.Int32 MaximumIndexHops", "System.Int64 MaximumIndexHops" )]
	[InlineData( "default=16", "default=17" )]
	[InlineData( "# AssemblyVersion: 1.0.0.0", "# AssemblyVersion: 2.0.0.0" )]
	[InlineData( "BerkeleyDbTerminalDatabaseEntry [sealed]", "UnexpectedWriterEntry [sealed]" )]
	public void ReaderOrDeltaDriftCannotBeHiddenBySubtractingWriterTypes( string before, string after ) {
		string current = Read( "1.16.0" );
		Assert.Contains( before, current );
		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbApiFreeze.VerifyReaderReconstruction( current.Replace( before, after ), Read( "1.15.0" ) )
		);
	}

	[Fact]
	public void RewritingBothManifestsCannotRedefineTheHistoricalReader() {
		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbApiFreeze.VerifyReaderReconstruction(
				Read( "1.16.0" ).Replace( "default=16", "default=17" ),
				Read( "1.15.0" ).Replace( "default=16", "default=17" )
			)
		);
	}

	[Fact]
	public void UnapprovedAdditionalTypeIsRejected() {
		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbApiFreeze.VerifyReaderReconstruction(
				Read( "1.16.0" ) + "\nTYPE class Unexpected [sealed]\nEND\n", Read( "1.15.0" )
			)
		);
	}

	private static string Read( string version ) {
		DirectoryInfo? root = new( AppContext.BaseDirectory );
		while ( root is not null && !File.Exists( Path.Combine( root.FullName, "Icod.TermInfo.sln" ) ) ) {
			root = root.Parent;
		}
		return File.ReadAllText( Path.Combine( root!.FullName, "docs", version + "-BERKELEYDB-PUBLIC-API-BASELINE.txt" ) );
	}
}
