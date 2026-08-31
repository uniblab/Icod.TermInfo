using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC08DifferentialValidationTests {
	private static readonly (string FileName, string TerminalName)[] Corpus = [
		( "tc08-bsd.termcap", "tc08-bsd" ),
		( "tc08-gnu.termcap", "tc08-gnu" ),
	];

	[Fact]
	public void CheckedInCorpusRoundTripsDeterministicallyThroughRuntime() {
		string root = FindRepositoryRoot();
		foreach ( (string fileName, string terminalName) in Corpus ) {
			string source =
				File.ReadAllText(
					Path.Combine(
						root,
						"tests",
						"Icod.TermInfo.Termcap.Tests",
						"fixtures",
						fileName
					)
				);
			TermcapSourceParseResult first =
				TermcapSourceParser.Parse(
					source,
					fileName
				);
			TermcapSourceParseResult second =
				TermcapSourceParser.Parse(
					source,
					fileName
				);

			Assert.False( first.HasErrors );
			Assert.Equal(
				DescribeParseResult( first ),
				DescribeParseResult( second )
			);

			TerminalDescription original =
				ResolveAndConvert(
					first.Document,
					terminalName
				);
			TermcapRenderResult renderedFirst =
				TermcapRenderer.Render(
					original,
					new TermcapRenderOptions( 72 )
				);
			TermcapRenderResult renderedSecond =
				TermcapRenderer.Render(
					original,
					new TermcapRenderOptions( 72 )
				);

			Assert.True( renderedFirst.IsRepresentable );
			Assert.False( renderedFirst.HasErrors );
			Assert.Equal( renderedFirst.Text, renderedSecond.Text );
			string renderedText =
				Assert.IsType<string>( renderedFirst.Text );
			TermcapSourceParseResult reparsed =
				TermcapSourceParser.Parse(
					renderedText,
					fileName + ".roundtrip"
				);
			Assert.False( reparsed.HasErrors );
			TerminalDescription roundTrip =
				ResolveAndConvert(
					reparsed.Document,
					terminalName
				);

			AssertEquivalentCorpusDescription(
				original,
				roundTrip
			);
		}
	}

	[Fact]
	public void AdoptedStringEscapeAndByteBoundariesRemainFrozen() {
		const string Source =
			"esc|Escape terminal:"
			+ "a1=^A:d1=^?:e1=\\E:e2=\\e:a2=\\a:"
			+ "n1=\\n:n2=\\l:r1=\\r:t1=\\t:b1=\\b:"
			+ "f1=\\f:v1=\\v:s1=\\s:c1=\\^:bs=\\\\:"
			+ "o1=\\001:o2=\\377:";
		TermcapSourceParseResult parsed =
			TermcapSourceParser.Parse( Source );

		Assert.False( parsed.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( parsed.Document.Entries );
		Dictionary<string, string?> values =
			entry.Fields.ToDictionary(
				field => field.CapabilityName,
				field => field.StringValue,
				StringComparer.Ordinal
			);
		Assert.Equal( "\u0001", values["a1"] );
		Assert.Equal( "\u007f", values["d1"] );
		Assert.Equal( "\u001b", values["e1"] );
		Assert.Equal( "\u001b", values["e2"] );
		Assert.Equal( "\a", values["a2"] );
		Assert.Equal( "\n", values["n1"] );
		Assert.Equal( "\n", values["n2"] );
		Assert.Equal( "\r", values["r1"] );
		Assert.Equal( "\t", values["t1"] );
		Assert.Equal( "\b", values["b1"] );
		Assert.Equal( "\f", values["f1"] );
		Assert.Equal( "\v", values["v1"] );
		Assert.Equal( " ", values["s1"] );
		Assert.Equal( "^", values["c1"] );
		Assert.Equal( "\\", values["bs"] );
		Assert.Equal( "\u0001", values["o1"] );
		Assert.Equal( "\u00ff", values["o2"] );
	}

	[Theory]
	[InlineData( "co#2147483647", int.MaxValue )]
	[InlineData( "co#0x7fffffff", int.MaxValue )]
	[InlineData( "co#017777777777", int.MaxValue )]
	public void SignedThirtyTwoBitNumericMaximumRemainsAccepted(
		string field,
		int expected
	) {
		TermcapSourceParseResult parsed =
			TermcapSourceParser.Parse(
				$"demo|Demo terminal:{field}:"
			);

		Assert.False( parsed.HasErrors );
		Assert.Equal(
			expected,
			Assert.Single(
				Assert.Single( parsed.Document.Entries ).Fields
			).NumericValue
		);
	}

	[Theory]
	[InlineData( "broken", TermcapSourceDiagnosticCodes.MissingHeaderTerminator )]
	[InlineData( "demo|Demo terminal:co#2147483648:", TermcapSourceDiagnosticCodes.NumericValueOutOfRange )]
	[InlineData( "demo|Demo terminal:cl=\\400:", TermcapSourceDiagnosticCodes.OctalEscapeOutOfRange )]
	[InlineData( "demo|Demo terminal:cl=\\:", TermcapSourceDiagnosticCodes.IncompleteBackslashEscape )]
	public void MalformedAndOverflowInputsFailDeterministically(
		string source,
		string expectedCode
	) {
		TermcapSourceParseResult first =
			TermcapSourceParser.Parse( source );
		TermcapSourceParseResult second =
			TermcapSourceParser.Parse( source );

		Assert.True( first.HasErrors );
		Assert.Contains(
			first.Diagnostics,
			diagnostic => diagnostic.Code == expectedCode
		);
		Assert.Equal(
			DescribeParseResult( first ),
			DescribeParseResult( second )
		);
	}

	[Fact]
	public void LiteralAndEscapedNullsAreRejected() {
		foreach (
			string source
			in new[] {
				"demo|Demo terminal:cl=\0:",
				"demo|Demo terminal:cl=\\000:",
				"demo|Demo terminal:cl=^@:",
			}
		) {
			TermcapSourceParseResult parsed =
				TermcapSourceParser.Parse( source );
			Assert.True( parsed.HasErrors );
			Assert.Contains(
				parsed.Diagnostics,
				diagnostic =>
					diagnostic.Code
						== TermcapSourceDiagnosticCodes.EmbeddedNullCharacter
			);
		}
	}

	[Fact]
	public void InheritanceCyclesAndDepthRemainBoundedAtClosure() {
		TermcapSourceParseResult cycle =
			TermcapSourceParser.Parse(
				"one|One terminal:tc=two:\n"
				+ "two|Two terminal:tc=one:\n"
			);
		Assert.False( cycle.HasErrors );
		TermcapSourceResolveResult cycleResult =
			TermcapSourceResolver.Resolve(
				cycle.Document,
				"one"
			);
		Assert.True( cycleResult.HasErrors );
		Assert.Contains(
			cycleResult.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.InheritanceCycle
		);

		TermcapSourceParseResult deep =
			TermcapSourceParser.Parse(
				"one|One terminal:tc=two:\n"
				+ "two|Two terminal:tc=three:\n"
				+ "three|Three terminal:co#80:\n"
			);
		TermcapSourceResolveResult depthResult =
			TermcapSourceResolver.Resolve(
				deep.Document,
				"one",
				new TermcapSourceResolverOptions( 1 )
			);
		Assert.True( depthResult.HasErrors );
		Assert.Contains(
			depthResult.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapSourceDiagnosticCodes.MaximumInheritanceDepthExceeded
		);
	}

	[Fact]
	public void MissingParentAndCancellationPrecedenceRemainFrozen() {
		TermcapSourceParseResult missing =
			TermcapSourceParser.Parse(
				"child|Child terminal:tc=missing:"
			);
		Assert.False( missing.HasErrors );
		TermcapSourceResolveResult missingResult =
			TermcapSourceResolver.Resolve(
				missing.Document,
				"child"
			);
		Assert.True( missingResult.HasErrors );
		Assert.Contains(
			missingResult.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.MissingSourceEntry
		);

		TermcapSourceParseResult cancellation =
			TermcapSourceParser.Parse(
				"base|Base terminal:am:co#80:\n"
				+ "child|Child terminal:am@:co#132:tc=base:\n"
			);
		TermcapSourceResolveResult cancellationResult =
			TermcapSourceResolver.Resolve(
				cancellation.Document,
				"child"
			);
		Assert.False( cancellationResult.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>(
				cancellationResult.Entry
			);
		Assert.False(
			resolved.TryGetField(
				"am",
				out _
			)
		);
		Assert.True(
			resolved.TryGetField(
				"co",
				out TermcapSourceResolvedField? columns
			)
		);
		Assert.Equal( 132, columns!.SourceField.NumericValue );
		Assert.Equal( 0, columns.InheritanceDepth );
	}

	[Fact]
	public void ExactLosslessAndLossyMappingsRemainObservable() {
		TerminalDescription exact =
			ResolveAndConvert(
				TermcapSourceParser.Parse(
					"exact|Exact terminal:am:co#80:"
				).Document,
				"exact"
			);
		Assert.True( exact.GetBoolean( BooleanCapability.AutoRightMargin ) );

		TermcapSourceParseResult lossySource =
			TermcapSourceParser.Parse(
				"lossy|Lossy terminal:mr=canonical:BO=alias:"
			);
		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				lossySource.Document,
				"lossy"
			);
		Assert.False( resolved.HasErrors );
		TermcapConversionResult lossy =
			TermcapConverter.Convert(
				Assert.IsType<TermcapSourceResolvedEntry>( resolved.Entry )
			);
		Assert.False( lossy.HasErrors );
		Assert.True( lossy.HasLoss );
		Assert.Contains(
			lossy.Diagnostics,
			diagnostic =>
				diagnostic.Decision == TermcapConversionDecision.Approximation
		);
	}

	[Fact]
	public void ExplicitEnvironmentAcquisitionUsesOnlyCallerSuppliedProviders() {
		MemoryEnvironmentProvider environment =
			new(
				new Dictionary<string, string?>( StringComparer.Ordinal ) {
					["TERMCAP"] = "/missing",
					["TERMPATH"] = string.Join( Path.PathSeparator, "/first", "/second" ),
					["HOME"] = "/home/tc08",
				}
			);
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/second"] = "tc08-env|TC08 environment terminal:co#132:\n",
				}
			);
		TermcapAcquisitionOptions options =
			TermcapAcquisitionOptions.FromEnvironment(
				environment,
				files,
				TermcapDefaultPathPolicy.None
			);
		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"tc08-env",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			new[] { "/missing", "/first", "/second" },
			files.OpenedPaths
		);
		Assert.Equal(
			132,
			result.Description!.GetNumber( NumericCapability.Columns )
		);
	}

	[Fact]
	public void SeededMutationCorpusRemainsDeterministicAndBounded() {
		const string Seed =
			"mut|Mutation terminal:am:co#80:li#24:cl=\\E[H:cm=\\E[%i%d;%dH:";
		uint mutationState = 0x00001608u;
		TermcapSourceParserOptions parserOptions =
			new( 4096 );
		TermcapSourceResolverOptions resolverOptions =
			new( 8 );

		for ( int iteration = 0; iteration < 256; iteration++ ) {
			string mutated =
				Mutate(
					Seed,
					ref mutationState
				);
			TermcapSourceParseResult first =
				TermcapSourceParser.Parse(
					mutated,
					$"mutation-{iteration}",
					parserOptions
				);
			TermcapSourceParseResult second =
				TermcapSourceParser.Parse(
					mutated,
					$"mutation-{iteration}",
					parserOptions
				);
			Assert.Equal(
				DescribeParseResult( first ),
				DescribeParseResult( second )
			);

			if ( !first.HasErrors && first.Document.Entries.Count != 0 ) {
				string name = first.Document.Entries[0].Names[0];
				TermcapSourceResolveResult firstResolved =
					TermcapSourceResolver.Resolve(
						first.Document,
						name,
						resolverOptions
					);
				TermcapSourceResolveResult secondResolved =
					TermcapSourceResolver.Resolve(
						second.Document,
						name,
						resolverOptions
					);
				Assert.Equal(
					DescribeResolveResult( firstResolved ),
					DescribeResolveResult( secondResolved )
				);
			}
		}
	}

	private static TerminalDescription ResolveAndConvert(
		TermcapSourceDocument document,
		string terminalName
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentException.ThrowIfNullOrWhiteSpace( terminalName );

		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				document,
				terminalName
			);
		Assert.False( resolved.HasErrors );
		TermcapConversionResult converted =
			TermcapConverter.Convert(
				Assert.IsType<TermcapSourceResolvedEntry>( resolved.Entry )
			);
		Assert.False( converted.HasErrors );
		return Assert.IsType<TerminalDescription>( converted.Description );
	}

	private static void AssertEquivalentCorpusDescription(
		TerminalDescription expected,
		TerminalDescription actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		Assert.Equal( expected.Name, actual.Name );
		Assert.Equal( expected.Aliases.ToArray(), actual.Aliases.ToArray() );
		Assert.Equal( expected.Description, actual.Description );
		Assert.Equal(
			expected.GetBoolean( BooleanCapability.AutoRightMargin ),
			actual.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Equal(
			expected.GetNumber( NumericCapability.Columns ),
			actual.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			expected.GetNumber( NumericCapability.Lines ),
			actual.GetNumber( NumericCapability.Lines )
		);
		Assert.Equal(
			expected.GetString( StringCapability.ClearScreen ),
			actual.GetString( StringCapability.ClearScreen )
		);
		Assert.Equal(
			expected.GetString( StringCapability.CursorAddress ),
			actual.GetString( StringCapability.CursorAddress )
		);
		Assert.Equal(
			expected.GetString( StringCapability.ClearToEndOfLine ),
			actual.GetString( StringCapability.ClearToEndOfLine )
		);
		bool expectedHasExtended =
			expected.ExtendedCapabilities.TryGetValue(
				"!!",
				out TermInfoCapabilityValue expectedExtended
			);
		bool actualHasExtended =
			actual.ExtendedCapabilities.TryGetValue(
				"!!",
				out TermInfoCapabilityValue actualExtended
			);
		Assert.Equal( expectedHasExtended, actualHasExtended );
		if ( expectedHasExtended ) {
			Assert.Equal( expectedExtended.Kind, actualExtended.Kind );
			Assert.Equal( expectedExtended.StringValue, actualExtended.StringValue );
		}
	}

	private static string DescribeParseResult(
		TermcapSourceParseResult result
	) {
		ArgumentNullException.ThrowIfNull( result );
		return string.Join(
			"|",
			new[] {
				$"entries={result.Document.Entries.Count}",
				string.Join(
					",",
					result.Diagnostics.Select(
						diagnostic =>
							$"{diagnostic.Code}@{diagnostic.Span?.Offset}:{diagnostic.Span?.Length}"
					)
				),
			}
		);
	}

	private static string DescribeResolveResult(
		TermcapSourceResolveResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		string fields =
			result.Entry is null
				? string.Empty
				: string.Join(
					",",
					result.Entry.Fields.Select(
						field =>
							$"{field.CapabilityName}:{field.InheritanceDepth}:{field.SourceField.Kind}"
					)
				)
		;
		string diagnostics =
			string.Join(
				",",
				result.Diagnostics.Select(
					diagnostic =>
						$"{diagnostic.Code}@{diagnostic.Span?.Offset}:{diagnostic.Span?.Length}"
				)
			);
		return $"entry={( result.Entry is null ? 0 : 1 )}|fields={fields}|diagnostics={diagnostics}";
	}

	private static string Mutate(
		string source,
		ref uint state
	) {
		ArgumentNullException.ThrowIfNull( source );
		char[] alphabet = [
			':', '\\', '^', '#', '=', '@', '.', '|', '0', '7', '8', 'x', 'A', '\n', '\r', '\0',
		];
		int operation = NextMutationValue( ref state, 3 );
		int index = NextMutationValue( ref state, source.Length + 1 );
		char value =
			alphabet[
				NextMutationValue(
					ref state,
					alphabet.Length
				)
			];

		return operation switch {
			0 => source.Insert( index, value.ToString() ),
			1 when source.Length != 0 => source.Remove( Math.Min( index, source.Length - 1 ), 1 ),
			_ when source.Length != 0 =>
				source[..Math.Min( index, source.Length - 1 )]
					+ value
					+ source[( Math.Min( index, source.Length - 1 ) + 1 )..],
			_ => value.ToString(),
		};
	}

	private static int NextMutationValue(
		ref uint state,
		int maximumExclusive
	) {
		if ( maximumExclusive < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumExclusive )
			);
		}

		state ^= state << 13;
		state ^= state >> 17;
		state ^= state << 5;
		return (int)( state % (uint)maximumExclusive );
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}

	private sealed class MemoryEnvironmentProvider : ITermcapEnvironmentProvider {
		private readonly IReadOnlyDictionary<string, string?> _values;

		internal MemoryEnvironmentProvider(
			IReadOnlyDictionary<string, string?> values
		) {
			ArgumentNullException.ThrowIfNull( values );
			_values = values;
		}

		public string? GetEnvironmentVariable(
			string name
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
			return _values.TryGetValue(
				name,
				out string? value
			)
				? value
				: null
			;
		}
	}

	private sealed class MemoryFileProvider : ITermcapFileProvider {
		private readonly IReadOnlyDictionary<string, string> _sources;
		private readonly List<string> _openedPaths = [];

		internal MemoryFileProvider(
			IReadOnlyDictionary<string, string> sources
		) {
			ArgumentNullException.ThrowIfNull( sources );
			_sources = sources;
		}

		internal IReadOnlyList<string> OpenedPaths => _openedPaths;

		public bool TryOpenText(
			string path,
			[NotNullWhen( true )] out TextReader? reader
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( path );
			_openedPaths.Add( path );
			if ( !_sources.TryGetValue( path, out string? source ) ) {
				reader = null;
				return false;
			}
			reader = new StringReader( source );
			return true;
		}
	}
}
