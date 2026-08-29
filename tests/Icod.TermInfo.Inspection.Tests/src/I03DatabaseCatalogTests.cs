using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I03DatabaseCatalogTests {
	[Fact]
	public void MissingRootReturnsEmptyMissingCatalog() {
		string parent = CreateTemporaryDirectory();
		string root = Path.Combine( parent, "missing" );

		try {
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( Path.GetFullPath( root ), catalog.Root );
			Assert.Equal( TermInfoDatabaseCatalogKind.Missing, catalog.Kind );
			Assert.Empty( catalog.Entries );
			Assert.Empty( catalog.Issues );
			Assert.Empty( catalog.DuplicateCanonicalNames );
			Assert.False( catalog.HasIssues );
		} finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public void EmptyDirectoryReturnsEmptyConventionalCatalog() {
		string root = CreateTemporaryDirectory();

		try {
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( TermInfoDatabaseCatalogKind.ConventionalDirectory, catalog.Kind );
			Assert.Empty( catalog.Entries );
			Assert.Empty( catalog.Issues );
			Assert.Empty( catalog.DuplicateCanonicalNames );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void FileRootIsReportedAsUnsupportedStore() {
		string parent = CreateTemporaryDirectory();
		string root = Path.Combine( parent, "terminfo.db" );
		File.WriteAllText( root, "not a directory" );

		try {
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( TermInfoDatabaseCatalogKind.UnsupportedStore, catalog.Kind );
			Assert.Empty( catalog.Entries );
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public void HexadecimalLayoutParsesTerminalMetadata() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"xterm-i03",
				"I03 hexadecimal layout",
				"xterm-i03-alias"
			);

		try {
			string path =
				WriteCandidate(
					root,
					"78",
					terminal.Name,
					CompiledTermInfoWriter.Write( terminal )
				);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );
			TermInfoDatabaseCatalogEntry entry = Assert.Single( catalog.Entries );

			Assert.Equal( TermInfoDatabaseCatalogKind.ConventionalDirectory, catalog.Kind );
			Assert.Equal( Path.GetFullPath( path ), entry.Path );
			Assert.Equal( terminal.Name, entry.Name );
			Assert.Equal( terminal.Description, entry.Description );
			Assert.Equal( terminal.Aliases.ToArray(), entry.Aliases.ToArray() );
			Assert.Equal( terminal.Name, entry.Terminal.Name );
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void LiteralLayoutIsAccepted() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"xterm-literal",
				"I03 literal layout"
			);

		try {
			WriteCandidate(
				root,
				"x",
				terminal.Name,
				CompiledTermInfoWriter.Write( terminal )
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( terminal.Name, Assert.Single( catalog.Entries ).Name );
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void AliasPublicationsAreRetainedAsPhysicalEntriesAndDetectedAsDuplicates() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"catalog-main",
				"I03 aliases",
				"catalog-a",
				"catalog-b"
			);

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				terminal
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( 3, catalog.Entries.Count );
			Assert.All(
				catalog.Entries,
				entry => Assert.Equal( terminal.Name, entry.Name )
			);
			Assert.Equal(
				new[] { terminal.Name },
				catalog.DuplicateCanonicalNames.ToArray()
			);
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void LiteralAndHexadecimalCopiesProduceDeterministicDuplicateIdentity() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"xterm-duplicate",
				"I03 duplicate"
			);
		byte[] data = CompiledTermInfoWriter.Write( terminal );

		try {
			WriteCandidate( root, "x", terminal.Name, data );
			WriteCandidate( root, "78", terminal.Name, data );

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( 2, catalog.Entries.Count );
			Assert.Equal(
				new[] { terminal.Name },
				catalog.DuplicateCanonicalNames.ToArray()
			);
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void MalformedAndTruncatedEntriesAreReportedWithoutHidingValidEntries() {
		string root = CreateTemporaryDirectory();
		TerminalDescription valid =
			CreateTerminal(
				"valid-i03",
				"I03 valid"
			);

		try {
			WriteCandidate(
				root,
				"76",
				valid.Name,
				CompiledTermInfoWriter.Write( valid )
			);
			WriteCandidate(
				root,
				"62",
				"broken",
				[ 0x1a, 0x01, 0x00 ]
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( valid.Name, Assert.Single( catalog.Entries ).Name );
			TermInfoDatabaseCatalogIssue issue = Assert.Single( catalog.Issues );
			Assert.Equal( TermInfoDatabaseCatalogIssueKind.MalformedEntry, issue.Kind );
			Assert.Contains( "broken", issue.Path, StringComparison.Ordinal );
			Assert.True( catalog.HasIssues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void ParserResourceLimitIsAppliedToCatalogEntries() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"limit-i03",
				"I03 parser limit"
			);

		try {
			WriteCandidate(
				root,
				"6c",
				terminal.Name,
				CompiledTermInfoWriter.Write( terminal )
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory(
					root,
					new CompiledTermInfoParserOptions( 16 )
				);

			Assert.Empty( catalog.Entries );
			Assert.Equal(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Assert.Single( catalog.Issues ).Kind
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void ParsedEntryWithWrongPhysicalPlacementIsRetainedAndReported() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"xterm-misplaced",
				"I03 misplaced"
			);

		try {
			WriteCandidate(
				root,
				"z",
				terminal.Name,
				CompiledTermInfoWriter.Write( terminal )
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Equal( terminal.Name, Assert.Single( catalog.Entries ).Name );
			Assert.Equal(
				TermInfoDatabaseCatalogIssueKind.InvalidPlacement,
				Assert.Single( catalog.Issues ).Kind
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void UnrecognizedAndNestedDirectoriesAreNotTraversed() {
		string root = CreateTemporaryDirectory();
		TerminalDescription terminal =
			CreateTerminal(
				"nested-i03",
				"I03 nested"
			);

		try {
			WriteCandidate(
				root,
				Path.Combine( "ignored", "6e" ),
				terminal.Name,
				CompiledTermInfoWriter.Write( terminal )
			);

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			Assert.Empty( catalog.Entries );
			Assert.Empty( catalog.Issues );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void ResultsAreIndependentOfFileCreationOrder() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		TerminalDescription alpha = CreateTerminal( "alpha-i03", "Alpha" );
		TerminalDescription zulu = CreateTerminal( "zulu-i03", "Zulu" );

		try {
			WriteCandidate( firstRoot, "7a", zulu.Name, CompiledTermInfoWriter.Write( zulu ) );
			WriteCandidate( firstRoot, "61", alpha.Name, CompiledTermInfoWriter.Write( alpha ) );

			WriteCandidate( secondRoot, "61", alpha.Name, CompiledTermInfoWriter.Write( alpha ) );
			WriteCandidate( secondRoot, "7a", zulu.Name, CompiledTermInfoWriter.Write( zulu ) );

			TermInfoDatabaseCatalog first =
				TermInfoDatabaseInspector.InspectDirectory( firstRoot );
			TermInfoDatabaseCatalog second =
				TermInfoDatabaseInspector.InspectDirectory( secondRoot );

			Assert.Equal(
				ProjectEntries( first ),
				ProjectEntries( second )
			);
			Assert.Equal(
				first.DuplicateCanonicalNames.ToArray(),
				second.DuplicateCanonicalNames.ToArray()
			);
			Assert.Empty( first.Issues );
			Assert.Empty( second.Issues );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public void PreCanceledInspectionThrowsBeforeFilesystemTraversal() {
		string root = CreateTemporaryDirectory();
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		try {
			Assert.Throws<OperationCanceledException>(
				() => TermInfoDatabaseInspector.InspectDirectory(
					root,
					null,
					cancellation.Token
				)
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public void FilesystemFailuresHaveStableIssueClassification() {
		Assert.Equal(
			TermInfoDatabaseCatalogIssueKind.PermissionFailure,
			TermInfoDatabaseInspector.ClassifyCatalogIoException(
				new UnauthorizedAccessException()
			)
		);
		Assert.Equal(
			TermInfoDatabaseCatalogIssueKind.IoFailure,
			TermInfoDatabaseInspector.ClassifyCatalogIoException(
				new IOException()
			)
		);
	}

	[Fact]
	public void ReparsePointClassificationIsExplicit() {
		Assert.True(
			TermInfoDatabaseInspector.IsCatalogReparsePoint(
				FileAttributes.ReparsePoint
			)
		);
		Assert.False(
			TermInfoDatabaseInspector.IsCatalogReparsePoint(
				FileAttributes.Normal
			)
		);
	}

	[Fact]
	public void ConventionalDirectoryNameRecognitionIsBounded() {
		Assert.True( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( "x" ) );
		Assert.True( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( "78" ) );
		Assert.True( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( "AF" ) );
		Assert.False( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( "abc" ) );
		Assert.False( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( "g0" ) );
		Assert.False( TermInfoDatabaseInspector.IsConventionalCatalogDirectoryName( string.Empty ) );
	}

	[Fact]
	public void LinkedConventionalSubdirectoryIsSkippedWhenLinksAreSupported() {
		string parent = CreateTemporaryDirectory();
		string root = Path.Combine( parent, "root" );
		string target = Path.Combine( parent, "target" );
		Directory.CreateDirectory( root );
		Directory.CreateDirectory( target );
		string link = Path.Combine( root, "78" );

		try {
			try {
				Directory.CreateSymbolicLink(
					link,
					target
				);
			} catch (Exception exception) when (
				exception is UnauthorizedAccessException
				|| exception is IOException
				|| exception is PlatformNotSupportedException
			) {
				return;
			}

			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );
			TermInfoDatabaseCatalogIssue issue = Assert.Single( catalog.Issues );

			Assert.Equal( TermInfoDatabaseCatalogIssueKind.LinkSkipped, issue.Kind );
			Assert.Empty( catalog.Entries );
		} finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	private static TerminalDescription CreateTerminal(
		string name,
		string description,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( name )
				.SetDescription( description )
				.SetBoolean( BooleanCapability.AutoLeftMargin );

		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}

		return builder.Build();
	}

	private static string WriteCandidate(
		string root,
		string directoryName,
		string fileName,
		byte[] data
	) {
		string directory =
			Path.Combine(
				root,
				directoryName
			);
		Directory.CreateDirectory( directory );
		string path =
			Path.Combine(
				directory,
				fileName
			);
		File.WriteAllBytes(
			path,
			data
		);
		return path;
	}

	private static string[] ProjectEntries(
		TermInfoDatabaseCatalog catalog
	) {
		ArgumentNullException.ThrowIfNull( catalog );

		return catalog.Entries
			.Select(
				entry =>
					entry.Name
					+ "|"
					+ Path.GetRelativePath(
						catalog.Root,
						entry.Path
					)
					+ "|"
					+ entry.Description
			)
			.ToArray();
	}

	private static string CreateTemporaryDirectory() {
		string path =
			Path.Combine(
				Path.GetTempPath(),
				$"icod-terminfo-i03-{Guid.NewGuid():N}"
			);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		if (!Directory.Exists(path)) {
			return;
		}

		try {
			Directory.Delete(
				path,
				recursive: true
			);
		} catch (IOException) {
		} catch (UnauthorizedAccessException) {
		}
	}
}
