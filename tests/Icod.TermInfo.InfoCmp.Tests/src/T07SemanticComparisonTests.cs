using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class T07SemanticComparisonTests {
	[Fact]
	public async Task MultipleOperandsDefaultToDifferenceComparison() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				new TerminalDescriptionBuilder( "same" )
					.SetDescription( "Equal terminal" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			);
			Publish(
				secondRoot,
				new TerminalDescriptionBuilder( "same" )
					.SetDescription( "Equal terminal" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			);

			CommandResult result =
				await RunAsync(
					[ "-A", firstRoot, "-B", secondRoot, "same", "same" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				"Comparing 'same' with 'same':",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"no reported semantic differences.",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task DifferenceModeReportsIdentityAndStandardCapabilityDifferences() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				new TerminalDescriptionBuilder( "left" )
					.AddAlias( "left-alias" )
					.SetDescription( "left description" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "left-clear" )
					.Build()
			);
			Publish(
				secondRoot,
				new TerminalDescriptionBuilder( "right" )
					.AddAlias( "right-alias" )
					.SetDescription( "right description" )
					.SetNumber( NumericCapability.Columns, 132 )
					.SetString( StringCapability.ClearScreen, "right-clear" )
					.Build()
			);

			CommandResult result =
				await RunAsync(
					[ "-d", "-A", firstRoot, "-B", secondRoot, "left", "right" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "name: 'left', 'right'.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "aliases: 'left-alias', 'right-alias'.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "description: 'left description', 'right description'.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "am: T, F.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "cols: 80, 132.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "clear: 'left-clear', 'right-clear'.", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task ExtendedDifferencesRequireXAndExposeKindMismatch() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				new TerminalDescriptionBuilder( "extended" )
					.SetDescription( "Extended terminal" )
					.SetExtendedBoolean( "leftOnly" )
					.SetExtendedNumber( "kindMismatch", 7 )
					.SetExtendedString( "value", "left" )
					.Build()
			);
			Publish(
				secondRoot,
				new TerminalDescriptionBuilder( "extended" )
					.SetDescription( "Extended terminal" )
					.SetExtendedBoolean( "rightOnly" )
					.SetExtendedString( "kindMismatch", "7" )
					.SetExtendedString( "value", "right" )
					.Build()
			);

			string[] baseArguments =
				[ "-A", firstRoot, "-B", secondRoot, "extended", "extended" ];
			CommandResult standard =
				await RunAsync( baseArguments );
			CommandResult extended =
				await RunAsync(
					[ "-x", .. baseArguments ]
				);

			Assert.DoesNotContain( "leftOnly", standard.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "kindMismatch", standard.Stdout, StringComparison.Ordinal );
			Assert.Contains( "leftOnly: T, F.", extended.Stdout, StringComparison.Ordinal );
			Assert.Contains( "rightOnly: F, T.", extended.Stdout, StringComparison.Ordinal );
			Assert.Contains(
				"kindMismatch: number:7, string:'7'.",
				extended.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains( "value: 'left', 'right'.", extended.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task CommonModeReportsEqualEffectiveCapabilities() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription first =
				new TerminalDescriptionBuilder( "common" )
					.SetDescription( "First" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "same" )
					.SetExtendedNumber( "xCommon", 5 )
					.SetExtendedString( "xDifferent", "left" )
					.Build();
			TerminalDescription second =
				new TerminalDescriptionBuilder( "common" )
					.SetDescription( "Second" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "same" )
					.SetExtendedNumber( "xCommon", 5 )
					.SetExtendedString( "xDifferent", "right" )
					.Build();
			Publish( firstRoot, first );
			Publish( secondRoot, second );

			CommandResult result =
				await RunAsync(
					[ "-c", "-x", "-A", firstRoot, "-B", secondRoot, "common", "common" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "am = T.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "cols = 80.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "clear = 'same'.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "xCommon = 5.", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "xDifferent", result.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task AbsentModeUsesOnlyClosedStandardCatalogAcrossAllEntries() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				new TerminalDescriptionBuilder( "first" )
					.SetDescription( "First" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetExtendedBoolean( "xOnlyFirst" )
					.Build()
			);
			Publish(
				root,
				new TerminalDescriptionBuilder( "second" )
					.SetDescription( "Second" )
					.SetExtendedBoolean( "xOnlySecond" )
					.Build()
			);

			CommandResult standard =
				await RunAsync(
					[ "-n", "-A", root, "-B", root, "first", "second" ]
				);
			CommandResult withExtensions =
				await RunAsync(
					[ "-n", "-x", "-A", root, "-B", root, "first", "second" ]
				);

			Assert.Equal( CommandExitCodes.Success, standard.Status );
			Assert.Contains( "!cols.", standard.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "!am.", standard.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "xOnly", standard.Stdout, StringComparison.Ordinal );
			Assert.Equal( standard.Stdout, withExtensions.Stdout );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task FirstOperandIsComparedWithEverySubsequentOperand() {
		string firstRoot = CreateTemporaryDirectory();
		string otherRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				CreateColumnsDescription( "first", 80 )
			);
			Publish(
				otherRoot,
				CreateColumnsDescription( "second", 100 )
			);
			Publish(
				otherRoot,
				CreateColumnsDescription( "third", 120 )
			);

			CommandResult result =
				await RunAsync(
					[ "-A", firstRoot, "-B", otherRoot, "first", "second", "third" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "Comparing 'first' with 'second':", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "Comparing 'first' with 'third':", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "Comparing 'second' with 'third':", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "cols: 80, 100.", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "cols: 80, 120.", result.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( otherRoot );
		}
	}

	[Fact]
	public async Task SameTerminalNameCanBeComparedAcrossExplicitRoots() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				CreateColumnsDescription( "same", 80 )
			);
			Publish(
				secondRoot,
				CreateColumnsDescription( "same", 132 )
			);

			CommandResult result =
				await RunAsync(
					[ "-A", firstRoot, "-B", secondRoot, "same", "same" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "cols: 80, 132.", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task MissingSubsequentTerminalIsOperationalFailureWithoutPartialStdout() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			Publish(
				firstRoot,
				CreateColumnsDescription( "first", 80 )
			);

			CommandResult result =
				await RunAsync(
					[ "-A", firstRoot, "-B", secondRoot, "first", "missing" ]
				);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0002 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	public static TheoryData<string[]> InvalidComparisonShapes =>
		new() {
			new[] { "-d", "one" },
			new[] { "-B", "directory", "one" },
			new[] { "-d", "-c", "one", "two" },
			new[] { "-0", "one", "two" },
			new[] { "-w", "80", "one", "two" },
			new[] { "-s", "d", "one", "two" },
			new[] { "-q", "one" },
		};

	[Theory]
	[MemberData( nameof( InvalidComparisonShapes ) )]
	public async Task InvalidComparisonShapesAreUsageErrors(
		string[] args
	) {
		CommandResult result =
			await RunAsync( args );

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task QuietModeUsesShortDeterministicPresentation() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateColumnsDescription( "first", 80 )
			);
			Publish(
				root,
				CreateColumnsDescription( "second", 132 )
			);

			CommandResult result =
				await RunAsync(
					[ "-q", "-A", root, "-B", root, "first", "second" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.StartsWith(
				"'first' -> 'second'",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.DoesNotContain( "Comparing", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( $"{Environment.NewLine}cols: 80, 132.", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( $"{Environment.NewLine}    cols", result.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task DifferenceOrderingIsDeterministicAndExtendedNamesAreOrdinal() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription first =
				new TerminalDescriptionBuilder( "order" )
					.SetDescription( "Ordering" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "left" )
					.SetExtendedString( "zValue", "left" )
					.SetExtendedBoolean( "AFlag" )
					.Build();
			TerminalDescription second =
				new TerminalDescriptionBuilder( "order" )
					.SetDescription( "Ordering" )
					.SetString( StringCapability.ClearScreen, "right" )
					.SetNumber( NumericCapability.Columns, 132 )
					.SetExtendedBoolean( "aFlag" )
					.SetExtendedString( "zValue", "right" )
					.Build();
			Publish( firstRoot, first );
			Publish( secondRoot, second );

			CommandResult result =
				await RunAsync(
					[ "-x", "-A", firstRoot, "-B", secondRoot, "order", "order" ]
				);

			int booleanIndex = result.Stdout.IndexOf( "am:", StringComparison.Ordinal );
			int numericIndex = result.Stdout.IndexOf( "cols:", StringComparison.Ordinal );
			int stringIndex = result.Stdout.IndexOf( "clear:", StringComparison.Ordinal );
			int upperExtendedIndex = result.Stdout.IndexOf( "AFlag:", StringComparison.Ordinal );
			int lowerExtendedIndex = result.Stdout.IndexOf( "aFlag:", StringComparison.Ordinal );
			int valueExtendedIndex = result.Stdout.IndexOf( "zValue:", StringComparison.Ordinal );

			Assert.True( booleanIndex >= 0 );
			Assert.True( numericIndex > booleanIndex );
			Assert.True( stringIndex > numericIndex );
			Assert.True( upperExtendedIndex > stringIndex );
			Assert.True( lowerExtendedIndex > upperExtendedIndex );
			Assert.True( valueExtendedIndex > lowerExtendedIndex );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	private static TerminalDescription CreateColumnsDescription(
		string name,
		int columns
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( $"{name} terminal" )
			.SetNumber( NumericCapability.Columns, columns )
			.Build();
	}

	private static void Publish(
		string root,
		TerminalDescription description
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( description );

		CompiledTermInfoDatabaseWriter.Write(
			root,
			description
		);
	}

	private static async Task<CommandResult> RunAsync(
		string[] args
	) {
		ArgumentNullException.ThrowIfNull( args );

		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr
		);

		return new CommandResult(
			status,
			ReadText( stdout ),
			ReadText( stderr )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path =
			System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				$"icod-terminfo-infocmp-t07-{Guid.NewGuid():N}"
			);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			Directory.Delete(
				path,
				recursive: true
			);
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private static string ReadText(
		MemoryStream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
