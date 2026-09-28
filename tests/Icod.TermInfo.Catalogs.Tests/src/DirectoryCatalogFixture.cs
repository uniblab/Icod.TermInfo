using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Catalogs.Tests;

internal sealed class DirectoryCatalogFixture : IDisposable {
	internal string Root { get; } = Path.Combine( Path.GetTempPath(), "icod-uc02-" + Guid.NewGuid().ToString( "N" ) );
	internal TerminalCatalogSource Source => new( Root, TerminalCatalogSourceKind.ConventionalDirectory );
	internal DirectoryCatalogFixture() => Directory.CreateDirectory( Root );
	internal string Write( string directory, string name, byte[]? bytes = null ) {
		string parent = Path.Combine( Root, directory );
		Directory.CreateDirectory( parent );
		string path = Path.Combine( parent, name );
		File.WriteAllBytes( path, bytes ?? Bytes() );
		return path;
	}
	internal static TerminalDescription Terminal( string name = "sample", string description = "UC02 fixture" ) =>
		new TerminalDescriptionBuilder( name ).SetDescription( description ).AddAlias( "a" ).AddAlias( "b" ).Build();
	internal static byte[] Bytes() => CompiledTermInfoWriter.Write( Terminal() );
	internal TermInfoDatabaseCatalog Physical(
		TermInfoDatabaseCatalogKind kind = TermInfoDatabaseCatalogKind.ConventionalDirectory,
		IEnumerable<TermInfoDatabaseCatalogEntry>? entries = null,
		IEnumerable<TermInfoDatabaseCatalogIssue>? issues = null,
		IEnumerable<string>? duplicates = null
	) => new( Root, kind, entries ?? [], issues ?? [], duplicates ?? [] );
	internal TermInfoDatabaseCatalogEntry Entry( string relativePath, TerminalDescription? terminal = null ) =>
		new( Path.Combine( Root, relativePath ), terminal ?? Terminal() );
	internal TermInfoDatabaseCatalogIssue Issue( TermInfoDatabaseCatalogIssueKind kind, string? relativePath = null, string message = "fixture issue" ) =>
		new( kind, relativePath is null ? Root : Path.Combine( Root, relativePath ), message );
	internal TerminalCatalog Read( TerminalCatalogReadOptions? options = null, CancellationToken token = default ) =>
		ConventionalTerminalCatalogAdapter.Read( Source, options ?? new(), token );
	public void Dispose() => Directory.Delete( Root, true );
}
