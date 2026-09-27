using System.Globalization;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[CollectionDefinition( "UC01 process state", DisableParallelization = true )]
public sealed class UC01ProcessStateCollection { }

[Collection( "UC01 process state" )]
public sealed class UC01CatalogModelTests {
	private static readonly string Root = Path.GetFullPath( "uc01-model" );
	private static TerminalCatalogSource Source => new( Root, TerminalCatalogSourceKind.ConventionalDirectory );
	private static TerminalDescription Terminal => new TerminalDescriptionBuilder( "sample" ).AddAlias( "alias" ).Build();
	private static TerminalCatalogEntry Entry( string name = "sample", string file = "s/sample" ) =>
		new( name, name == "sample" ? TerminalCatalogEntryKind.Canonical : TerminalCatalogEntryKind.Alias, Terminal, Root, Path.Combine( Root, file ) );
	private static TerminalCatalogIssue Issue( TerminalCatalogIssueKind kind = TerminalCatalogIssueKind.IoFailure, string? name = null ) => new( kind, Root, null, name, "fixture issue" );

	[Fact]
	public void SourceNormalizesOnceAndValidatesPathBeforeKind() {
		string original = Environment.CurrentDirectory;
		TerminalCatalogSource source = new( "relative", TerminalCatalogSourceKind.BerkeleyDbHash );
		string expected = Path.GetFullPath( "relative" );
		try {
			Environment.CurrentDirectory = Path.GetTempPath();
			Assert.Equal( expected, source.Path );
		} finally { Environment.CurrentDirectory = original; }
		Assert.Equal( TerminalCatalogSourceKind.BerkeleyDbHash, source.Kind );
		Assert.Throws<ArgumentNullException>( () => new TerminalCatalogSource( null!, (TerminalCatalogSourceKind)99 ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogSource( " ", (TerminalCatalogSourceKind)99 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogSource( "valid", (TerminalCatalogSourceKind)99 ) );
	}

	[Fact]
	public void OptionsSnapshotDefaultsAndIndependentExtremeBudgets() {
		CompiledTermInfoParserOptions parser = new();
		TerminalCatalogReadOptions options = new( parser );
		Assert.NotSame( parser, options.ParserOptions );
		Assert.Equal( 1_048_576, options.ParserOptions.MaximumEntrySize );
		Assert.Equal( 131_072, options.MaximumCandidateCount );
		Assert.Equal( 65_536, options.MaximumEntryCount );
		Assert.Equal( 4_096, options.MaximumIssueCount );
		Assert.Equal( 67_108_864L, options.MaximumParsedBytes );
		Assert.Equal( 67_108_864, options.MaximumDatabaseSize );
		Assert.Equal( 65_536, options.MaximumRecordCount );
		Assert.Equal( 16, options.MaximumIndexHops );
		Assert.Equal( 67_108_864L, options.MaximumDecodedBytes );
		Assert.Equal( 0, new TerminalCatalogReadOptions( maximumIndexHops: 0 ).MaximumIndexHops );
		Assert.Equal( 1024, new TerminalCatalogReadOptions( maximumIndexHops: 1024 ).MaximumIndexHops );
		Assert.Equal( long.MaxValue, new TerminalCatalogReadOptions( maximumParsedBytes: long.MaxValue ).MaximumParsedBytes );
		Assert.Equal( int.MaxValue, new TerminalCatalogReadOptions( maximumEntryCount: int.MaxValue ).MaximumEntryCount );
		Assert.Equal( 1, new TerminalCatalogReadOptions( maximumDecodedBytes: 1 ).MaximumDecodedBytes );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectInvalidBudgets( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumCandidateCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumEntryCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumIssueCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumParsedBytes: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumDatabaseSize: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumRecordCount: value ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumDecodedBytes: value ) );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 1025 )]
	public void OptionsRejectUnsupportedHops( int value ) => Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogReadOptions( maximumIndexHops: value ) );

	[Fact]
	public void EntriesSeparatePublicationNamesFromDeclaredAliases() {
		TerminalCatalogEntry canonical = Entry();
		TerminalCatalogEntry alias = Entry( "alias", "a/alias" );
		Assert.Equal( "sample", canonical.PublicationName );
		Assert.Equal( "alias", alias.PublicationName );
		Assert.Equal( "sample", alias.Terminal.Name );
		Assert.Equal( TerminalCatalogEntryKind.Alias, alias.Kind );
		Assert.Equal( Root, alias.SourcePath );
		Assert.True( Path.IsPathFullyQualified( alias.EntryPath! ) );
		TerminalCatalogEntry hashed = new( "alias", TerminalCatalogEntryKind.Alias, Terminal, Root, null );
		Assert.Null( hashed.EntryPath );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogEntry( "ALIAS", TerminalCatalogEntryKind.Alias, Terminal, Root, null ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogEntry( "alias", TerminalCatalogEntryKind.Canonical, Terminal, Root, null ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogEntry( "sample", TerminalCatalogEntryKind.Alias, Terminal, Root, null ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogEntry( "sample", (TerminalCatalogEntryKind)99, Terminal, Root, null ) );
		Assert.Throws<ArgumentNullException>( () => new TerminalCatalogEntry( "sample", TerminalCatalogEntryKind.Canonical, null!, Root, null ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogEntry( "sample", TerminalCatalogEntryKind.Canonical, Terminal, "relative", null ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogEntry( "sample", TerminalCatalogEntryKind.Canonical, Terminal, Root, "relative" ) );
	}

	[Fact]
	public void IssuesValidateTypedFieldsAndOptionalLocators() {
		TerminalCatalogIssue issue = Issue();
		Assert.Null( issue.EntryPath );
		Assert.Null( issue.PublicationName );
		Assert.Equal( Root, issue.SourcePath );
		Assert.Equal( "fixture issue", issue.Message );
		Assert.Throws<ArgumentOutOfRangeException>( () => Issue( (TerminalCatalogIssueKind)99 ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogIssue( TerminalCatalogIssueKind.IoFailure, "relative", null, null, "issue" ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogIssue( TerminalCatalogIssueKind.IoFailure, Root, "relative", null, "issue" ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogIssue( TerminalCatalogIssueKind.IoFailure, Root, null, " ", "issue" ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogIssue( TerminalCatalogIssueKind.IoFailure, Root, null, null, " " ) );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "tr-TR" )]
	public void ResultsCopyAndSortCollectionsOrdinally( string culture ) {
		CultureInfo previous = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = new( culture );
			List<TerminalCatalogEntry> entries = [ Entry(), Entry( "alias", "a/alias" ), Entry( "sample", "73/sample" ) ];
			List<TerminalCatalogIssue> issues = [ Issue( TerminalCatalogIssueKind.IoFailure ), Issue( TerminalCatalogIssueKind.DuplicatePublication, "sample" ) ];
			List<string> duplicates = [ "sample" ];
			TerminalCatalog catalog = new( Source, TerminalCatalogStatus.Partial, entries, issues, duplicates );
			entries.Clear(); issues.Clear(); duplicates.Clear();
			Assert.Equal( new[] { "alias", "sample", "sample" }, catalog.Entries.Select( x => x.PublicationName ) );
			Assert.EndsWith( Path.Combine( "73", "sample" ), catalog.Entries[1].EntryPath, StringComparison.Ordinal );
			Assert.Equal( new[] { TerminalCatalogIssueKind.IoFailure, TerminalCatalogIssueKind.DuplicatePublication }, catalog.Issues.Select( x => x.Kind ) );
			Assert.Equal( new[] { "sample" }, catalog.DuplicatePublicationNames );
			Assert.True( catalog.HasIssues );
			Assert.Equal( TerminalCatalogStatus.Partial, catalog.Status );
			Assert.Throws<NotSupportedException>( () => ((IList<TerminalCatalogEntry>)catalog.Entries).Clear() );
			Assert.Throws<NotSupportedException>( () => ((IList<TerminalCatalogIssue>)catalog.Issues).Clear() );
			Assert.Throws<NotSupportedException>( () => ((IList<string>)catalog.DuplicatePublicationNames).Clear() );
		} finally { CultureInfo.CurrentCulture = previous; }
	}

	[Fact]
	public void TurkishIdentitiesAndIssueMessagesUseOrdinalOrdering() {
		CultureInfo previous = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = new( "tr-TR" );
			string[] names = [ "ı", "İ", "i", "I" ];
			TerminalCatalogEntry[] entries = names.Select( name => new TerminalCatalogEntry(
				name, TerminalCatalogEntryKind.Canonical, new TerminalDescriptionBuilder( name ).Build(), Root, Path.Combine( Root, name )
			)
			).ToArray();
			TerminalCatalogIssue[] issues = names.Select( name => new TerminalCatalogIssue(
				TerminalCatalogIssueKind.IoFailure, Root, null, null, name
			)
			).ToArray();
			TerminalCatalog result = new( Source, TerminalCatalogStatus.Partial, entries, issues, [] );
			Assert.Equal( new[] { "I", "i", "İ", "ı" }, result.Entries.Select( entry => entry.PublicationName ) );
			Assert.Equal( new[] { "I", "i", "İ", "ı" }, result.Issues.Select( issue => issue.Message ) );
		} finally { CultureInfo.CurrentCulture = previous; }
	}

	[Fact]
	public void ResultsRejectInconsistentProvenanceStatusDuplicatesAndNullElements() {
		Assert.False( new TerminalCatalog( Source, TerminalCatalogStatus.Complete, [], [], [] ).HasIssues );
		Assert.Throws<ArgumentNullException>( () => new TerminalCatalog( null!, TerminalCatalogStatus.Complete, [], [], [] ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalog( Source, (TerminalCatalogStatus)99, [], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Complete, [ null! ], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [], [ null! ], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [], [ Issue() ], [ null! ] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Complete, [], [ Issue() ], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Missing, [ Entry() ], [ Issue() ], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Unavailable, [], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Complete, [ new( "sample", TerminalCatalogEntryKind.Canonical, Terminal, Root + "other", Path.Combine( Root, "file" ) ) ], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [], [ new( TerminalCatalogIssueKind.IoFailure, Root + "other", null, null, "issue" ) ], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Complete, [ new( "sample", TerminalCatalogEntryKind.Canonical, Terminal, Root, null ) ], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( new( Root, TerminalCatalogSourceKind.BerkeleyDbHash ), TerminalCatalogStatus.Complete, [ Entry() ], [], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [ Entry(), Entry( "sample", "73/sample" ) ], [ Issue() ], [] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [ Entry(), Entry( "sample", "73/sample" ) ], [ Issue( TerminalCatalogIssueKind.DuplicatePublication, "sample" ) ], [ "sample", "sample" ] ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalog( Source, TerminalCatalogStatus.Partial, [ Entry(), Entry( "sample", "73/sample" ) ], [ Issue() ], [ "sample" ] ) );
	}

	[Fact]
	public void LimitExceptionPreservesTypedSourceAndUnderlyingFailure() {
		TerminalCatalogSource source = Source;
		IOException inner = new( "cause" );
		TerminalCatalogLimitException error = new( source, "MaximumIndexHops", 0, inner );
		Assert.Same( source, error.Source );
		Assert.Equal( "MaximumIndexHops", error.LimitName );
		Assert.Equal( 0, error.Limit );
		Assert.Same( inner, error.InnerException );
		Assert.Throws<ArgumentNullException>( () => new TerminalCatalogLimitException( null!, "MaximumEntryCount", 1 ) );
		Assert.Throws<ArgumentException>( () => new TerminalCatalogLimitException( source, "Unknown", 1 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogLimitException( source, "MaximumEntryCount", 0 ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new TerminalCatalogLimitException( source, "MaximumIndexHops", -1 ) );
	}
}
