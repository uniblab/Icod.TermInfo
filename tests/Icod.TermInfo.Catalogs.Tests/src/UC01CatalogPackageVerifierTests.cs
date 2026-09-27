using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC01CatalogPackageVerifierTests {
	[Theory]
	[InlineData( "missing-tfm" )]
	[InlineData( "wrong-dependency" )]
	[InlineData( "missing-xml" )]
	[InlineData( "missing-license" )]
	[InlineData( "missing-icon" )]
	[InlineData( "missing-readme" )]
	[InlineData( "native" )]
	[InlineData( "wrong-assembly" )]
	[InlineData( "wrong-api" )]
	public void RejectsCorruptedPackages( string corruption ) {
		string root = FindRoot();
		string temp = Path.Combine( Path.GetTempPath(), "uc01-package-" + Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( temp );
		try {
			string package = Path.Combine( temp, "fixture.nupkg" );
			CreateFixture( package, root );
			string baseline = File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) );
			Assert.Equal( new string( 'a', 40 ), PackageVerifier.Program.VerifyPackage( package, "1.17.0-Alpha-1", baseline ) );
			if ( corruption == "wrong-api" ) {
				baseline += "TYPE class Unexpected [sealed]\nEND\n";
			}
			else {
				using ZipArchive zip = ZipFile.Open( package, ZipArchiveMode.Update );
				string? remove = corruption switch {
					"missing-tfm" => "lib/net9.0/Icod.TermInfo.Catalogs.dll",
					"missing-xml" => "lib/net8.0/Icod.TermInfo.Catalogs.xml",
					"missing-license" => "LICENSE",
					"missing-icon" => "icon.png",
					"missing-readme" => "README.md",
					_ => null,
				};
				if ( remove is not null ) {
					zip.GetEntry( remove )!.Delete();
				}
				if ( corruption == "native" ) {
					Write( zip, "runtimes/linux-x64/native/libdb.so", [ 0 ] );
				}
				if ( corruption == "wrong-assembly" ) {
					zip.GetEntry( "lib/net8.0/Icod.TermInfo.Catalogs.dll" )!.Delete();
					Write( zip, "lib/net8.0/Icod.TermInfo.Catalogs.dll", File.ReadAllBytes( typeof( TerminalDescription ).Assembly.Location ) );
				}
				if ( corruption == "wrong-dependency" ) {
					ZipArchiveEntry entry = zip.GetEntry( "Icod.TermInfo.Catalogs.nuspec" )!;
					string xml;
					using ( StreamReader reader = new( entry.Open() ) ) {
						xml = reader.ReadToEnd();
					}
					entry.Delete();
					WriteText( zip, "Icod.TermInfo.Catalogs.nuspec", xml.Replace( "Icod.TermInfo.Inspection", "Icod.TermInfo.Compiler", StringComparison.Ordinal ) );
				}
			}
			Assert.Throws<InvalidDataException>( () => PackageVerifier.Program.VerifyPackage( package, "1.17.0-Alpha-1", baseline ) );
		} finally { Directory.Delete( temp, recursive: true ); }
	}

	private static void CreateFixture( string path, string root ) {
		XDocument project = XDocument.Load( Path.Combine( root, "Icod.TermInfo.Catalogs/Icod.TermInfo.Catalogs.csproj" ) );
		string Value( string name ) => project.Descendants( name ).Single().Value;
		XElement metadata = new( "metadata",
			new XElement( "id", "Icod.TermInfo.Catalogs" ), new XElement( "version", "1.17.0-Alpha-1" ),
			new XElement( "title", Value( "Title" ) ), new XElement( "authors", Value( "Authors" ) ),
			new XElement( "description", Value( "Description" ) ), new XElement( "copyright", Value( "Copyright" ) ),
			new XElement( "projectUrl", Value( "PackageProjectUrl" ) ), new XElement( "readme", "README.md" ),
			new XElement( "icon", "icon.png" ), new XElement( "requireLicenseAcceptance", "true" ),
			new XElement( "tags", Value( "PackageTags" ).Replace( ';', ' ' ) ),
			new XElement( "license", new XAttribute( "type", "expression" ), "LGPL-3.0-or-later" ),
			new XElement( "repository", new XAttribute( "type", "git" ), new XAttribute( "url", Value( "RepositoryUrl" ) ), new XAttribute( "commit", new string( 'a', 40 ) ) )
		);
		XElement dependencies = new( "dependencies" );
		using ZipArchive zip = ZipFile.Open( path, ZipArchiveMode.Create );
		foreach ( string tfm in new[] { "net8.0", "net9.0", "net10.0" } ) {
			// Surface/identity checks are independent of the host test runtime.
			Write( zip, $"lib/{tfm}/Icod.TermInfo.Catalogs.dll", File.ReadAllBytes( typeof( TerminalCatalog ).Assembly.Location ) );
			WriteText( zip, $"lib/{tfm}/Icod.TermInfo.Catalogs.xml", "<doc><assembly><name>Icod.TermInfo.Catalogs</name></assembly><members><member name=\"T:Icod.TermInfo.Catalogs.TerminalCatalog\" /></members></doc>" );
			dependencies.Add( new XElement( "group", new XAttribute( "targetFramework", tfm ),
				new[] { "Icod.TermInfo", "Icod.TermInfo.Inspection", "Icod.TermInfo.BerkeleyDb" }.Select( id => new XElement( "dependency", new XAttribute( "id", id ), new XAttribute( "version", "1.17.0-Alpha-1" ), new XAttribute( "exclude", "Build,Analyzers" ) ) )
			)
			);
		}
		metadata.Add( dependencies );
		WriteText( zip, "Icod.TermInfo.Catalogs.nuspec", new XElement( "package", metadata ).ToString() );
		WriteText( zip, "LICENSE", File.ReadAllText( Path.Combine( root, "LICENSE" ) ) );
		WriteText( zip, "README.md", "Catalogs fixture" );
		Write( zip, "icon.png", File.ReadAllBytes( Path.Combine( root, "icon.png" ) ) );
	}
	private static void WriteText( ZipArchive zip, string path, string value ) => Write( zip, path, System.Text.Encoding.UTF8.GetBytes( value ) );
	private static void Write( ZipArchive zip, string path, byte[] bytes ) { using Stream stream = zip.CreateEntry( path ).Open(); stream.Write( bytes ); }
	private static string FindRoot() {
		DirectoryInfo? root = new( AppContext.BaseDirectory );
		while ( root is not null && !File.Exists( Path.Combine( root.FullName, "Icod.TermInfo.sln" ) ) ) {
			root = root.Parent;
		}
		return root?.FullName ?? throw new InvalidOperationException( "Repository root not found." );
	}
}
