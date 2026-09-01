using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS07ContractTests {
	private const string DevelopmentVersion = "1.7.0-Alpha-7";
	private const int GeneratedCaseCount = 64;
	private const uint FirstGeneratedSeed = 0x17070001u;
	private const uint SeedStride = 0x9E3779B9u;

	private static readonly BooleanCapability[] GeneratedBooleans = [
		BooleanCapability.AutoRightMargin,
		BooleanCapability.BackColorErase,
		BooleanCapability.HasMetaKey,
		BooleanCapability.MoveInsertMode,
	];

	private static readonly NumericCapability[] GeneratedNumbers = [
		NumericCapability.Columns,
		NumericCapability.Lines,
		NumericCapability.Colors,
		NumericCapability.ColorPairs,
	];

	private static readonly int[] GeneratedNumericValues = [
		0,
		1,
		80,
		255,
		256,
		32767,
		32768,
		65535,
		1_000_000,
		int.MaxValue,
	];

	private static readonly StringCapability[] GeneratedStrings = [
		StringCapability.Bell,
		StringCapability.ClearScreen,
		StringCapability.CursorAddress,
	];

	private static readonly string[] GeneratedStringValues = [
		"bell\a",
		"comma,value",
		"backslash\\value",
		"line\r\nbreak",
		"\u001b[H\u001b[2J",
		"\u001b[%i%p1%d;%p2%dH",
	];

	[Fact]
	public void CoordinatedVersionAndImplementationRecordIdentifyRs07() {
		string root = FindRepositoryRoot();
		XDocument buildProperties = XDocument.Load(
			Path.Combine( root, "Directory.Build.props" ),
			LoadOptions.None
		);
		string version = buildProperties
			.Descendants()
			.Single(
				element => element.Name.LocalName == "IcodTermInfoSuiteVersion"
			)
			.Value
			.Trim();
		string implementation = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.7.0-RS07-DIFFERENTIAL-FUZZ-AND-HARDENING.md"
			)
		);

		Assert.Equal( DevelopmentVersion, version );
		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "6.5.20250216", implementation, StringComparison.Ordinal );
		Assert.Contains( "reproducible seed", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "RS08", implementation, StringComparison.Ordinal );
	}

	[Fact]
	public void DeterministicGeneratedStateSpaceRoundTripsSemantically() {
		for ( int index = 0; index < GeneratedCaseCount; index++ ) {
			uint seed = unchecked(
				FirstGeneratedSeed
				+ ( (uint)index * SeedStride )
			);
			Exception? failure = Record.Exception(
				() => VerifyGeneratedCase( seed )
			);

			Assert.True(
				failure is null,
				$"RS07 generated case failed; reproducible seed=0x{seed:X8}{Environment.NewLine}{failure}"
			);
		}
	}

	[Fact]
	public void MaximumSupportedParentCountIsAcceptedWithoutReordering() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs07-max-child" )
				.SetDescription( "RS07 maximum-parent child" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents =
			Enumerable.Range(
				0,
				TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount
			)
			.Select(
				index => {
					string name = $"rs07-max-parent-{index:D3}";
					TerminalDescription description =
						new TerminalDescriptionBuilder( name )
							.SetDescription( $"RS07 maximum parent {index}" )
							.Build();
					return new TerminalDescriptionSourceSynthesisParent(
						name,
						description
					);
			}
			)
			.ToArray();
		TerminalDescriptionSourceSynthesisOptions options = new(
			80,
			TerminalDescriptionSourceLayout.Canonical,
			TerminalDescriptionSourceCapabilityOrder.Database,
			TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount,
			includeExtendedCapabilities: true
		);

		string source = TerminalDescriptionSourceSynthesizer.Synthesize(
			target,
			parents,
			options
		);
		string[] useLines = source
			.Split( '\n', StringSplitOptions.RemoveEmptyEntries )
			.Where(
				line => line.TrimStart().StartsWith(
					"use=",
					StringComparison.Ordinal
				)
			)
			.ToArray();

		Assert.Equal(
			TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount,
			useLines.Length
		);
		Assert.Equal( "use=rs07-max-parent-000,", useLines[ 0 ].Trim() );
		Assert.Equal( "use=rs07-max-parent-255,", useLines[ ^1 ].Trim() );
	}

	[Fact]
	public void LargeExtendedUnionAndManyCancellationsRoundTrip() {
		List<TerminalDescriptionSourceSynthesisParent> parents = [];
		for ( int parentIndex = 0; parentIndex < 4; parentIndex++ ) {
			string parentName = $"rs07-union-parent-{parentIndex}";
			TerminalDescriptionBuilder builder =
				new TerminalDescriptionBuilder( parentName )
					.SetDescription( $"RS07 union parent {parentIndex}" );
			for ( int capabilityIndex = 0; capabilityIndex < 64; capabilityIndex++ ) {
				builder.SetExtendedString(
					$"X{parentIndex:D2}{capabilityIndex:D3}",
					$"value-{parentIndex}-{capabilityIndex}"
				);
			}
			TerminalDescription description = builder.Build();
			parents.Add(
				new TerminalDescriptionSourceSynthesisParent(
					parentName,
					description
				)
			);
		}
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs07-union-child" )
				.SetDescription( "RS07 union child" )
				.Build();

		string source = TerminalDescriptionSourceSynthesizer.Synthesize(
			target,
			parents
		);

		Assert.Equal( 256, source.Count( character => character == '@' ) );
		AssertSemanticRoundTrip( target, parents, source );
	}

	[Fact]
	public void LongStringRoundTripsWithoutResourceOrLayoutCorruption() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs07-long-parent" )
				.SetDescription( "RS07 long-string parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs07-long-child" )
				.SetDescription( "RS07 long-string child" )
				.SetString(
					StringCapability.ClearScreen,
					"prefix," + new string( 'x', 32_768 ) + "\\suffix"
				)
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source = TerminalDescriptionSourceSynthesizer.Synthesize(
			target,
			parents
		);

		Assert.True( source.Length > 32_768 );
		Assert.DoesNotContain( "\r", source );
		AssertSemanticRoundTrip( target, parents, source );
	}

	[Fact]
	public void AliasMediatedRepeatedParentReferencesRemainSemanticallyStable() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs07-repeat-parent" )
				.AddAlias( "rs07-repeat-alias" )
				.SetDescription( "RS07 repeated parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs07-repeat-child" )
				.SetDescription( "RS07 repeated child" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( "rs07-repeat-parent", parent ),
			new( "rs07-repeat-alias", parent ),
		];

		string source = TerminalDescriptionSourceSynthesizer.Synthesize(
			target,
			parents
		);

		int canonicalUse = source.IndexOf(
			"use=rs07-repeat-parent",
			StringComparison.Ordinal
		);
		int aliasUse = source.IndexOf(
			"use=rs07-repeat-alias",
			StringComparison.Ordinal
		);
		Assert.True( canonicalUse >= 0 );
		Assert.True( aliasUse > canonicalUse );
		AssertSemanticRoundTrip( target, parents, source );
	}

	[Fact]
	public void GeneratedCaseIsCultureIndependent() {
		uint seed = 0x1707C017u;
		GeneratedSynthesisCase generatedCase = CreateGeneratedCase( seed );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			string turkish = TerminalDescriptionSourceSynthesizer.Synthesize(
				generatedCase.Target,
				generatedCase.Parents
			);

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			string french = TerminalDescriptionSourceSynthesizer.Synthesize(
				generatedCase.Target,
				generatedCase.Parents
			);

			Assert.Equal( turkish, french );
			AssertSemanticRoundTrip(
				generatedCase.Target,
				generatedCase.Parents,
				turkish
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static void VerifyGeneratedCase(
		uint seed
	) {
		GeneratedSynthesisCase generatedCase = CreateGeneratedCase( seed );
		string first = TerminalDescriptionSourceSynthesizer.Synthesize(
			generatedCase.Target,
			generatedCase.Parents
		);
		string second = TerminalDescriptionSourceSynthesizer.Synthesize(
			generatedCase.Target,
			generatedCase.Parents
		);

		Assert.Equal( first, second );
		Assert.DoesNotContain( "\r", first );
		AssertSemanticRoundTrip(
			generatedCase.Target,
			generatedCase.Parents,
			first
		);
	}

	private static GeneratedSynthesisCase CreateGeneratedCase(
		uint seed
	) {
		DeterministicRandom random = new( seed );
		int parentCount = 1 + random.Next( 4 );
		List<TerminalDescriptionSourceSynthesisParent> parents = [];
		for ( int index = 0; index < parentCount; index++ ) {
			string name = FormattableString.Invariant(
				$"rs07-{seed:X8}-parent-{index}"
			);
			string alias = FormattableString.Invariant(
				$"rs07-{seed:X8}-parent-{index}-alias"
			);
			bool addAlias = random.Next( 2 ) == 1;
			TerminalDescriptionBuilder builder =
				new TerminalDescriptionBuilder( name )
					.SetDescription(
						FormattableString.Invariant(
							$"RS07 generated parent {index} for seed {seed:X8}"
						)
					);
			if ( addAlias ) {
				builder.AddAlias( alias );
			}
			ApplyGeneratedCapabilities( builder, ref random );
			TerminalDescription description = builder.Build();
			string useName = name;
			if ( addAlias && random.Next( 2 ) == 1 ) {
				useName = alias;
			}
			parents.Add(
				new TerminalDescriptionSourceSynthesisParent(
					useName,
					description
				)
			);
		}

		string targetName = FormattableString.Invariant(
			$"rs07-{seed:X8}-child"
		);
		TerminalDescriptionBuilder targetBuilder =
			new TerminalDescriptionBuilder( targetName )
				.SetDescription(
					FormattableString.Invariant(
						$"RS07 generated child for seed {seed:X8}"
					)
				);
		if ( random.Next( 2 ) == 1 ) {
			targetBuilder.AddAlias(
				FormattableString.Invariant(
					$"rs07-{seed:X8}-child-alias"
				)
			);
		}
		ApplyGeneratedCapabilities( targetBuilder, ref random );

		return new GeneratedSynthesisCase(
			targetBuilder.Build(),
			parents.ToArray()
		);
	}

	private static void ApplyGeneratedCapabilities(
		TerminalDescriptionBuilder builder,
		ref DeterministicRandom random
	) {
		ArgumentNullException.ThrowIfNull( builder );

		foreach ( BooleanCapability capability in GeneratedBooleans ) {
			if ( random.Next( 3 ) == 0 ) {
				builder.SetBoolean( capability );
			}
		}
		foreach ( NumericCapability capability in GeneratedNumbers ) {
			if ( random.Next( 3 ) == 0 ) {
				builder.SetNumber(
					capability,
					GeneratedNumericValues[ random.Next( GeneratedNumericValues.Length ) ]
				);
			}
		}
		foreach ( StringCapability capability in GeneratedStrings ) {
			if ( random.Next( 3 ) == 0 ) {
				builder.SetString(
					capability,
					GeneratedStringValues[ random.Next( GeneratedStringValues.Length ) ]
				);
			}
		}

		for ( int index = 0; index < 6; index++ ) {
			string name = FormattableString.Invariant( $"XGen{index:D2}" );
			switch ( random.Next( 4 ) ) {
				case 0:
					break;
				case 1:
					builder.SetExtendedBoolean( name );
					break;
				case 2:
					builder.SetExtendedNumber(
						name,
						GeneratedNumericValues[ random.Next( GeneratedNumericValues.Length ) ]
					);
					break;
				case 3:
					builder.SetExtendedString(
						name,
						GeneratedStringValues[ random.Next( GeneratedStringValues.Length ) ]
					);
					break;
				default:
					throw new InvalidOperationException(
						"The deterministic generator produced an invalid extended-capability choice."
					);
			}
		}
	}

	private static void AssertSemanticRoundTrip(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		StringBuilder combined = new();
		combined.Append( relativeSource );
		HashSet<string> rendered = new( StringComparer.Ordinal );
		foreach ( TerminalDescriptionSourceSynthesisParent parent in parents ) {
			if ( !rendered.Add( parent.Description.Name ) ) {
				continue;
			}
			combined.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}

		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			combined.ToString(),
			"rs07-generated-roundtrip.ti"
		);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			parsed.Document,
			target.Name
		);
		Assert.False(
			resolved.HasErrors,
			FormatDiagnostics( resolved.Diagnostics )
		);
		Assert.NotNull( resolved.Entry );
		TerminalDescription actual = resolved.Entry!.ToTerminalDescription();
		TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
			target,
			actual
		);
		Assert.True(
			comparison.AreEqual,
			string.Join(
				Environment.NewLine,
				comparison.Differences.Select(
					difference => difference.ToString()
				)
			)
		);
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return string.Join(
			Environment.NewLine,
			diagnostics.Select(
				diagnostic => diagnostic.Message
			)
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private sealed record GeneratedSynthesisCase(
		TerminalDescription Target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> Parents
	);

	private struct DeterministicRandom {
		private uint _state;

		internal DeterministicRandom(
			uint seed
		) {
			_state = seed == 0
				? 0xA341316Cu
				: seed
			;
		}

		internal int Next(
			int exclusiveMaximum
		) {
			if ( exclusiveMaximum <= 0 ) {
				throw new ArgumentOutOfRangeException(
					nameof( exclusiveMaximum )
				);
			}

			return (int)( NextUInt32() % (uint)exclusiveMaximum );
		}

		private uint NextUInt32() {
			uint value = _state;
			value ^= value << 13;
			value ^= value >> 17;
			value ^= value << 5;
			_state = value;
			return value;
		}
	}
}
