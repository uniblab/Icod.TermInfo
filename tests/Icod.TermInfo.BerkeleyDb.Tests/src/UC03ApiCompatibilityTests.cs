using Icod.TermInfo.BerkeleyDb.PackageVerifier;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class UC03ApiCompatibilityTests {
	[Fact]
	public void ActualFourteenTypeAssemblyReconstructsTwelveThenNineTypes() {
		string current = Current();
		Assert.Equal( 14, current.Split( '\n' ).Count( l => l.StartsWith( "TYPE ", StringComparison.Ordinal ) ) );
		string reconstructed = BerkeleyDbUc03Compatibility.Reconstruct( current, Types(), Member() );
		Assert.Equal( Read( "1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt" ).Replace( "\r\n", "\n" ), reconstructed );
		BerkeleyDbApiFreeze.VerifyReaderReconstruction( reconstructed, Read( "1.15.0-BERKELEYDB-PUBLIC-API-BASELINE.txt" ) );
	}

	[Theory]
	[InlineData( "type-missing" )]
	[InlineData( "type-duplicate" )]
	[InlineData( "type-change" )]
	[InlineData( "limits-change" )]
	[InlineData( "member-missing" )]
	[InlineData( "member-duplicate" )]
	[InlineData( "member-change" )]
	[InlineData( "unapproved-member" )]
	[InlineData( "legacy-change" )]
	public void AdditiveOrHistoricalDriftIsRejected( string change ) {
		string current = Current(), types = Types(), member = Member().TrimEnd( '\r', '\n' );
		string block = types.Split( "\n\n" )[0];
		string mutated = change switch {
			"type-missing" => current.Replace( block + "\n\n", "" ),
			"type-duplicate" => current + "\n" + block + "\n",
			"type-change" => current.Replace( "System.Int64 Limit {", "System.Int32 Limit {" ),
			"limits-change" => current.Replace( "default=67108864", "default=67108865" ),
			"member-missing" => current.Replace( member + "\n", "" ),
			"member-duplicate" => current.Replace( member, member + "\n" + member ),
			"member-change" => current.Replace( "ReadBounded(", "ReadChanged(" ),
			"unapproved-member" => current.Replace( member, member + "\n  METHOD public System.Void Unexpected()" ),
			_ => current.Replace( "default=16", "default=17" ),
		};
		Assert.NotEqual( current, mutated );
		Assert.Throws<InvalidDataException>( () => BerkeleyDbUc03Compatibility.Reconstruct( mutated, types, Member() ) );
	}

	private static string Current() => Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( typeof( BerkeleyDbTerminalCatalogReader ).Assembly );
	private static string Types() => Read( "1.17.0-UC03-BERKELEYDB-PUBLIC-API-ADDITIONS.txt" );
	private static string Member() => Read( "1.17.0-UC03-BERKELEYDB-PUBLIC-API-ADDITIVE-MEMBERS.txt" );
	private static string Read( string name ) {
		DirectoryInfo? root = new( AppContext.BaseDirectory );
		while ( root is not null && !File.Exists( Path.Combine( root.FullName, "Icod.TermInfo.sln" ) ) ) { root = root.Parent; }
		return File.ReadAllText( Path.Combine( root!.FullName, "docs", name ) ).Replace( "\r\n", "\n" );
	}
}
