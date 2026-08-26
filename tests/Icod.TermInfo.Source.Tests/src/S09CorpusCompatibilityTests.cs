using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S09CorpusCompatibilityTests {
	private const string MutationCharacters =
		"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789|,#=@\\^ \t\r\n";

	[Fact]
	public void AuthoritativeValidCorpusCoversSystemVExtendedAndInheritance() {
		TerminalDescription systemV =
			ResolveCorpusEntry(
				"valid/system-v-basic.ti",
				"s09-sysv"
			);
		Assert.Equal( "s09-sysv", systemV.Name );
		Assert.Equal(
			new[] { "s09sv" },
			systemV.Aliases
		);
		Assert.True(
			systemV.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Equal(
			80,
			systemV.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			24,
			systemV.GetNumber( NumericCapability.Lines )
		);
		Assert.Equal(
			"\a",
			systemV.GetString( StringCapability.Bell )
		);
		Assert.Equal(
			"\x1b[H\x1b[2J",
			systemV.GetString( StringCapability.ClearScreen )
		);

		TerminalDescription extended =
			ResolveCorpusEntry(
				"valid/ncurses-extended.ti",
				"s09-ncurses"
			);
		Assert.Equal(
			256,
			extended.GetNumber( NumericCapability.Colors )
		);
		Assert.True(
			extended.TryGetExtendedBoolean(
				"AX",
				out bool ax
			)
		);
		Assert.True( ax );
		Assert.True(
			extended.TryGetExtendedBoolean(
				"RGB",
				out bool rgb
			)
		);
		Assert.True( rgb );
		Assert.True(
			extended.TryGetExtendedBoolean(
				"XBool",
				out bool xbool
			)
		);
		Assert.True( xbool );
		Assert.True(
			extended.TryGetExtendedNumber(
				"XNum",
				out int xnum
			)
		);
		Assert.Equal( 0x1234, xnum );
		Assert.True(
			extended.TryGetExtendedString(
				"XStr",
				out string? xstr
			)
		);
		Assert.Equal(
			"left,right \x1b\x80",
			xstr
		);

		TerminalDescription inherited =
			ResolveCorpusEntry(
				"valid/inheritance.ti",
				"s09-child"
			);
		Assert.Equal( "s09-child", inherited.Name );
		Assert.Equal(
			new[] { "s09c" },
			inherited.Aliases
		);
		Assert.True(
			inherited.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Equal(
			132,
			inherited.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			24,
			inherited.GetNumber( NumericCapability.Lines )
		);
		Assert.Equal(
			"\x1b[H",
			inherited.GetString( StringCapability.ClearScreen )
		);
		Assert.False(
			inherited.TryGetExtendedCapability(
				"Vendor",
				out _
			)
		);
	}

	[Fact]
	public void DuplicateIdentitiesWarnAndFirstSourceOrderMatchRemainsDeterministic() {
		string path =
			GetCorpusPath( "malformed/duplicate-identities.ti" );
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				File.ReadAllText( path ),
				path
			);

		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		Assert.Equal(
			new[] {
				TermInfoSourceDiagnosticCodes.DuplicateSourceEntryName,
				TermInfoSourceDiagnosticCodes.DuplicateSourceAlias,
				TermInfoSourceDiagnosticCodes.DuplicateSourceAlias,
			},
			parsed.Diagnostics.Select(
				diagnostic => diagnostic.Code
			)
		);
		Assert.All(
			parsed.Diagnostics,
			diagnostic =>
				Assert.Equal(
					TermInfoSourceDiagnosticSeverity.Warning,
					diagnostic.Severity
				)
		);
		Assert.All(
			parsed.Diagnostics,
			diagnostic =>
				Assert.Equal(
					path,
					diagnostic.Span?.SourceName
				)
		);

		TermInfoSourceResolveResult first =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				"shared"
			);
		TermInfoSourceResolveResult second =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				"shared"
			);
		Assert.Equal(
			CreateResolveSignature( first ),
			CreateResolveSignature( second )
		);

		TermInfoSourceResolvedEntry resolved =
			AssertResolved( first );
		Assert.Equal(
			"s09-dup-one",
			resolved.SourceEntry.CanonicalName
		);
		Assert.Equal(
			80,
			resolved.GetNumber( NumericCapability.Columns )
		);

		TermInfoSourceResolveResult caseDistinct =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				"Shared"
			);
		Assert.Equal(
			"s09-case",
			AssertResolved( caseDistinct ).SourceEntry.CanonicalName
		);
	}

	[Fact]
	public void MalformedCorpusDiagnosticsAreStableAndReproducible() {
		string path =
			GetCorpusPath( "malformed/diagnostics.ti" );
		string source =
			File.ReadAllText( path );

		TermInfoSourceParseResult first =
			TermInfoSourceParser.Parse(
				source,
				path
			);
		TermInfoSourceParseResult second =
			TermInfoSourceParser.Parse(
				source,
				path
			);

		Assert.True( first.HasErrors );
		Assert.Equal(
			new[] {
				TermInfoSourceDiagnosticCodes.OrphanedCapabilityField,
				TermInfoSourceDiagnosticCodes.InvalidNumericValue,
				TermInfoSourceDiagnosticCodes.IncompleteControlEscape,
				TermInfoSourceDiagnosticCodes.MissingUseReference,
				TermInfoSourceDiagnosticCodes.EmptyField,
				TermInfoSourceDiagnosticCodes.MissingCapabilityName,
			},
			first.Diagnostics.Select(
				diagnostic => diagnostic.Code
			)
		);
		Assert.Equal(
			CreateParseSignature( first ),
			CreateParseSignature( second )
		);
	}

	[Fact]
	public void MalformedInheritanceProducesStableCycleAndMissingParentDiagnostics() {
		string path =
			GetCorpusPath( "malformed/inheritance.ti" );
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				File.ReadAllText( path ),
				path
			);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		Assert.Empty( parsed.Diagnostics );

		TermInfoSourceResolveResult cycle =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				"s09-cycle-a"
			);
		Assert.True( cycle.HasErrors );
		Assert.Null( cycle.Entry );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.InheritanceCycle,
			Assert.Single( cycle.Diagnostics ).Code
		);
		Assert.Equal(
			CreateResolveSignature( cycle ),
			CreateResolveSignature(
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					"s09-cycle-a"
				)
			)
		);

		TermInfoSourceResolveResult missing =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				"s09-missing"
			);
		Assert.True( missing.HasErrors );
		Assert.Null( missing.Entry );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MissingSourceEntry,
			Assert.Single( missing.Diagnostics ).Code
		);
		Assert.Equal(
			CreateResolveSignature( missing ),
			CreateResolveSignature(
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					"s09-missing"
				)
			)
		);
	}

	[Fact]
	public void SourceLengthLimitRejectsStringAndReaderWithoutPartialDocument() {
		TermInfoSourceLexerOptions options =
			new( 64 );
		string source =
			new string( 'x', 65 );

		TermInfoSourceParseResult fromString =
			TermInfoSourceParser.Parse(
				source,
				"s09-limit.ti",
				options
			);
		using StringReader reader =
			new( source );
		TermInfoSourceParseResult fromReader =
			TermInfoSourceParser.Parse(
				reader,
				"s09-limit.ti",
				options
			);

		Assert.True( fromString.HasErrors );
		Assert.True( fromReader.HasErrors );
		Assert.Empty( fromString.Document.Entries );
		Assert.Empty( fromString.Document.Tokens );
		Assert.Empty( fromReader.Document.Entries );
		Assert.Empty( fromReader.Document.Tokens );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MaximumSourceLengthExceeded,
			Assert.Single( fromString.Diagnostics ).Code
		);
		Assert.Equal(
			CreateParseSignature( fromString ),
			CreateParseSignature( fromReader )
		);
	}

	[Fact]
	public void MaximumInheritanceDepthAcceptsBoundaryAndRejectsNextEdge() {
		const int maximumDepth = 64;
		string source =
			CreateInheritanceChain( maximumDepth + 1 );
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"s09-depth.ti"
			);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);

		TermInfoSourceResolverOptions options =
			new( maximumDepth );
		TermInfoSourceResolvedEntry boundary =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					$"s09-depth-{maximumDepth}",
					options
				)
			);
		Assert.True(
			boundary.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Equal(
			80,
			boundary.GetNumber( NumericCapability.Columns )
		);

		TermInfoSourceResolveResult exceeded =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				$"s09-depth-{maximumDepth + 1}",
				options
			);
		Assert.True( exceeded.HasErrors );
		Assert.Null( exceeded.Entry );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
			Assert.Single( exceeded.Diagnostics ).Code
		);

		TermInfoSourceResolveResult repeated =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				$"s09-depth-{maximumDepth + 1}",
				options
			);
		Assert.Equal(
			CreateResolveSignature( exceeded ),
			CreateResolveSignature( repeated )
		);
	}

	[Fact]
	public void DeterministicMutationFuzzingDoesNotThrowOrDrift() {
		string seed =
			File.ReadAllText(
				GetCorpusPath( "valid/inheritance.ti" )
			);
		uint state = 0x1C0D_5EEDU;

		for ( int iteration = 0; iteration < 256; iteration++ ) {
			string mutated =
				Mutate(
					seed,
					ref state
				);

			TermInfoSourceParseResult first =
				TermInfoSourceParser.Parse(
					mutated,
					$"fuzz-{iteration:D3}.ti"
				);
			TermInfoSourceParseResult second =
				TermInfoSourceParser.Parse(
					mutated,
					$"fuzz-{iteration:D3}.ti"
				);

			Assert.Equal(
				CreateParseSignature( first ),
				CreateParseSignature( second )
			);

			if ( first.HasErrors
				|| first.Document.Entries.Count == 0 ) {
				continue;
			}

			string rootName =
				first.Document.Entries[ 0 ].CanonicalName;
			if ( string.IsNullOrWhiteSpace( rootName ) ) {
				continue;
			}

			TermInfoSourceResolveResult firstResolved =
				TermInfoSourceResolver.Resolve(
					first.Document,
					rootName
				);
			TermInfoSourceResolveResult secondResolved =
				TermInfoSourceResolver.Resolve(
					second.Document,
					rootName
				);
			Assert.Equal(
				CreateResolveSignature( firstResolved ),
				CreateResolveSignature( secondResolved )
			);
		}
	}

	[Fact]
	public void TicGeneratedCompatibilityCorpusRemainsOfflineAndCheckedIn() {
		string root = FindRepositoryRoot();
		string fixtureRoot =
			Path.Combine(
				root,
				"tests",
				"Icod.TermInfo.Tests",
				"fixtures",
				"compiled-terminfo"
			);
		string manifestPath =
			Path.Combine(
				fixtureRoot,
				"manifests",
				"manifest.json"
			);

		using JsonDocument manifest =
			JsonDocument.Parse(
				File.ReadAllText( manifestPath )
			);
		JsonElement generator =
			manifest.RootElement.GetProperty( "generator" );
		Assert.Equal(
			"tic",
			generator.GetProperty( "tool" ).GetString()
		);
		Assert.False(
			generator
				.GetProperty( "normalTestsRequireGenerator" )
				.GetBoolean()
		);

		JsonElement fixtures =
			manifest.RootElement.GetProperty( "fixtures" );
		Assert.Equal( 5, fixtures.GetArrayLength() );
		foreach ( JsonElement fixture in fixtures.EnumerateArray() ) {
			string source =
				fixture
					.GetProperty( "source" )
					.GetString()
				?? throw new InvalidOperationException(
					"A compatibility fixture does not name its source file."
				);
			string binary =
				fixture
					.GetProperty( "binary" )
					.GetString()
				?? throw new InvalidOperationException(
					"A compatibility fixture does not name its compiled file."
				);

			Assert.True(
				File.Exists(
					Path.Combine(
						fixtureRoot,
						source.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				),
				$"Missing compatibility source fixture '{source}'."
			);
			Assert.True(
				File.Exists(
					Path.Combine(
						fixtureRoot,
						binary.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				),
				$"Missing compatibility compiled fixture '{binary}'."
			);
		}
	}

	private static TerminalDescription ResolveCorpusEntry(
		string relativePath,
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		string path =
			GetCorpusPath( relativePath );
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				File.ReadAllText( path ),
				path
			);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		Assert.Empty( parsed.Diagnostics );

		return AssertResolved(
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					name
				)
			)
			.ToTerminalDescription();
	}

	private static TermInfoSourceResolvedEntry AssertResolved(
		TermInfoSourceResolveResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		Assert.False(
			result.HasErrors,
			FormatDiagnostics( result.Diagnostics )
		);
		Assert.Empty( result.Diagnostics );
		return Assert.IsType<TermInfoSourceResolvedEntry>( result.Entry );
	}

	private static string CreateInheritanceChain(
		int maximumEdge
	) {
		if ( maximumEdge < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( maximumEdge ) );
		}

		StringBuilder builder = new();
		builder.Append(
			"s09-depth-0|S09 depth zero,\n"
			+ "\tam,\n"
			+ "\tcols#80,\n"
		);

		for ( int index = 1; index <= maximumEdge; index++ ) {
			builder.Append(
				$"s09-depth-{index}|S09 depth {index},\n"
				+ $"\tuse=s09-depth-{index - 1},\n"
			);
		}

		return builder.ToString();
	}

	private static string Mutate(
		string seed,
		ref uint state
	) {
		ArgumentNullException.ThrowIfNull( seed );

		StringBuilder builder =
			new( seed );
		int operationCount =
			1 + (int)(Next( ref state ) % 4U);

		for ( int operation = 0; operation < operationCount; operation++ ) {
			uint choice =
				Next( ref state ) % 3U;
			char value =
				MutationCharacters[
					(int)(
						Next( ref state )
						% (uint)MutationCharacters.Length
					)
				];

			if ( choice == 0U
				|| builder.Length == 0 ) {
				int position =
					(int)(
						Next( ref state )
							% (uint)(builder.Length + 1)
					);
				builder.Insert(
					position,
					value
				);
				continue;
			}

			int existingPosition =
				(int)(
					Next( ref state )
						% (uint)builder.Length
				);
			if ( choice == 1U ) {
				builder[ existingPosition ] = value;
			}
			else {
				builder.Remove(
					existingPosition,
					1
				);
			}
		}

		return builder.ToString();
	}

	private static uint Next(
		ref uint state
	) {
		state =
			unchecked(
				(state * 1_664_525U)
				+ 1_013_904_223U
			);
		return state;
	}

	private static string CreateParseSignature(
		TermInfoSourceParseResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		StringBuilder builder = new();
		foreach ( TermInfoSourceDiagnostic diagnostic in result.Diagnostics ) {
			builder.Append( "D|" );
			builder.Append( diagnostic.Code );
			builder.Append( '|' );
			builder.Append( diagnostic.Severity );
			builder.Append( '|' );
			builder.Append( diagnostic.Span?.Offset ?? -1 );
			builder.Append( '|' );
			builder.Append( diagnostic.Span?.Length ?? -1 );
			builder.AppendLine();
		}

		foreach ( TermInfoSourceToken token in result.Document.Tokens ) {
			builder.Append( "T|" );
			builder.Append( token.Kind );
			builder.Append( '|' );
			builder.Append( token.Span.Offset );
			builder.Append( '|' );
			builder.Append( token.Span.Length );
			builder.Append( '|' );
			builder.AppendLine( token.Text );
		}

		foreach ( TermInfoSourceEntry entry in result.Document.Entries ) {
			builder.Append( "E|" );
			builder.Append( entry.CanonicalName );
			builder.Append( '|' );
			builder.Append(
				string.Join(
					",",
					entry.Aliases
				)
			);
			builder.Append( '|' );
			builder.Append( entry.Fields.Count );
			builder.AppendLine();
		}

		return builder.ToString();
	}

	private static string CreateResolveSignature(
		TermInfoSourceResolveResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		StringBuilder builder = new();
		builder.Append( result.HasErrors );
		builder.AppendLine();

		foreach ( TermInfoSourceDiagnostic diagnostic in result.Diagnostics ) {
			builder.Append( diagnostic.Code );
			builder.Append( '|' );
			builder.Append( diagnostic.Severity );
			builder.Append( '|' );
			builder.Append( diagnostic.Span?.Offset ?? -1 );
			builder.Append( '|' );
			builder.Append( diagnostic.Span?.Length ?? -1 );
			builder.AppendLine();
		}

		if ( result.Entry is not null ) {
			TerminalDescription description =
				result.Entry.ToTerminalDescription();
			builder.AppendLine( description.Name );
			builder.AppendLine( description.Description ?? "<null>" );
			builder.AppendLine(
				string.Join(
					",",
					description.Aliases
				)
			);
			builder.AppendLine(
				string.Join(
					",",
					description.BooleanCapabilities
				)
			);
			builder.AppendLine(
				string.Join(
					",",
					description.NumericCapabilities.Select(
						pair =>
							$"{pair.Key}={pair.Value}"
					)
				)
			);
			builder.AppendLine(
				string.Join(
					",",
					description.StringCapabilities.Select(
						pair =>
							$"{pair.Key}={pair.Value}"
					)
				)
			);
			builder.AppendLine(
				string.Join(
					",",
					description.ExtendedCapabilities
						.OrderBy(
							pair => pair.Key,
							StringComparer.Ordinal
						)
						.Select(
							pair =>
								$"{pair.Key}:{pair.Value.Kind}={pair.Value}"
						)
				)
			);
		}

		return builder.ToString();
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return string.Join(
			"; ",
			diagnostics.Select(
				diagnostic =>
					diagnostic.Code
						+ " "
						+ diagnostic.Message
			)
		);
	}

	private static string GetCorpusPath(
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return Path.Combine(
			FindRepositoryRoot(),
			"tests",
			"Icod.TermInfo.Source.Tests",
			"fixtures",
			"source-terminfo",
			relativePath.Replace(
				'/',
				Path.DirectorySeparatorChar
			)
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
				Path.Combine(
					current.FullName,
					"Icod.TermInfo.sln"
				)
			) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
