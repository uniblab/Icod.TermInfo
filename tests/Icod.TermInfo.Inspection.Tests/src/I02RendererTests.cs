using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I02RendererTests {
	private static readonly TerminalDescription[] BuiltInProfiles = [
		TerminalProfiles.Dumb,
		TerminalProfiles.Ansi,
		TerminalProfiles.Vt100,
		TerminalProfiles.Vt102,
		TerminalProfiles.Vt220,
		TerminalProfiles.Xterm,
		TerminalProfiles.Xterm16Color,
		TerminalProfiles.Xterm88Color,
		TerminalProfiles.Xterm256Color,
		TerminalProfiles.XtermDirect,
		TerminalProfiles.XtermDirect16,
		TerminalProfiles.XtermDirect256,
		TerminalProfiles.WinConsole,
		TerminalProfiles.MsTerminal,
		TerminalProfiles.MsTerminalDirect,
	];

	private static readonly string[] T29CompiledFixtures = [
		"t29-extended.bin",
		"t29-extended32.bin",
		"t29-legacy-alignment.bin",
		"t29-legacy-edge.bin",
		"t29-legacy-minimal.bin",
	];

	[Fact]
	public void Render_BuiltInProfiles_RoundTripSemantically() {
		foreach ( TerminalDescription description in BuiltInProfiles ) {
			AssertRoundTrip( description );
		}
	}

	[Fact]
	public void Render_T29CompiledFixtures_RoundTripSemantically() {
		string root =
			FindRepositoryRoot();
		string fixtureRoot =
			Path.Combine(
				root,
				"tests",
				"Icod.TermInfo.Tests",
				"fixtures",
				"compiled-terminfo",
				"compiled"
			);

		foreach ( string fixtureName in T29CompiledFixtures ) {
			TerminalDescription description =
				CompiledTermInfoParser.Parse(
					File.ReadAllBytes(
						Path.Combine(
							fixtureRoot,
							fixtureName
						)
					)
				);
			AssertRoundTrip( description );
		}
	}

	[Fact]
	public void Render_UsesCanonicalOrderingAndSourceEscapes() {
		string edgeValue =
			new string(
				new[] {
					'\x01',
					'\x1b',
					' ',
					',',
					'\\',
					'^',
					':',
					'|',
					'\x7f',
					'\x80',
					'\xff',
				}
			);
		TerminalDescription description =
			new TerminalDescriptionBuilder( "i02-edge" )
				.SetDescription( "I02 renderer edge fixture" )
				.SetExtendedString( "zString", "z" )
				.SetExtendedNumber( "zNumber", 2 )
				.SetExtendedBoolean( "zBoolean" )
				.SetString(
					StringCapability.Bell,
					edgeValue
				)
				.SetNumber(
					NumericCapability.Columns,
					132
				)
				.SetBoolean(
					BooleanCapability.AutoRightMargin
				)
				.SetExtendedString( "aString", "a" )
				.SetExtendedNumber( "aNumber", 1 )
				.SetExtendedBoolean( "aBoolean" )
				.Build();

		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				description
			);

		Assert.Contains(
			@"    bel=\001\E\s\,\\\^\:\|\177\200\377,",
			rendered
		);

		int standardBoolean =
			rendered.IndexOf(
				"\n    am,\n",
				StringComparison.Ordinal
			);
		int standardNumber =
			rendered.IndexOf(
				"\n    cols#132,\n",
				StringComparison.Ordinal
			);
		int standardString =
			rendered.IndexOf(
				"\n    bel=",
				StringComparison.Ordinal
			);
		int extendedBooleanA =
			rendered.IndexOf(
				"\n    aBoolean,\n",
				StringComparison.Ordinal
			);
		int extendedBooleanZ =
			rendered.IndexOf(
				"\n    zBoolean,\n",
				StringComparison.Ordinal
			);
		int extendedNumberA =
			rendered.IndexOf(
				"\n    aNumber#1,\n",
				StringComparison.Ordinal
			);
		int extendedNumberZ =
			rendered.IndexOf(
				"\n    zNumber#2,\n",
				StringComparison.Ordinal
			);
		int extendedStringA =
			rendered.IndexOf(
				"\n    aString=a,\n",
				StringComparison.Ordinal
			);
		int extendedStringZ =
			rendered.IndexOf(
				"\n    zString=z,\n",
				StringComparison.Ordinal
			);

		Assert.True(
			standardBoolean >= 0
				&& standardBoolean < standardNumber
				&& standardNumber < standardString
				&& standardString < extendedBooleanA
				&& extendedBooleanA < extendedBooleanZ
				&& extendedBooleanZ < extendedNumberA
				&& extendedNumberA < extendedNumberZ
				&& extendedNumberZ < extendedStringA
				&& extendedStringA < extendedStringZ
		);
		AssertRoundTrip( description );
	}

	[Fact]
	public void Render_MaximumPracticalStandardSet_RoundTrips() {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( "i02-maximum" )
				.SetDescription(
					"I02 maximum practical capability fixture"
				);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			builder.SetBoolean( metadata.Capability );
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			builder.SetNumber(
				metadata.Capability,
				metadata.BinaryIndex + 1
			);
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			builder.SetString(
				metadata.Capability,
				"value-"
					+ metadata.BinaryIndex.ToString(
						CultureInfo.InvariantCulture
					)
			);
		}

		AssertRoundTrip(
			builder.Build()
		);
	}

	[Fact]
	public void Render_WrapsLongStringsDeterministicallyWithLf() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "i02-wrap" )
				.SetDescription(
					"I02 deterministic wrapping fixture"
				)
				.SetString(
					StringCapability.ClearScreen,
					new string( 'x', 240 )
				)
				.Build();

		string first =
			TerminalDescriptionSourceRenderer.Render(
				description
			);
		string second =
			TerminalDescriptionSourceRenderer.Render(
				description
			);

		Assert.Equal(
			first,
			second
		);
		Assert.DoesNotContain(
			"\r",
			first
		);
		Assert.All(
			first.Split( '\n' ),
			line =>
				Assert.True(
					line.Length <= 80,
					$"Rendered line exceeds 80 characters: '{line}'."
				)
		);
		Assert.Contains(
			"\n        ",
			first
		);
		AssertRoundTrip( description );
	}

	[Fact]
	public void Render_IsCultureIndependentAndTextWriterMatchesStringEntryPoint() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "i02-culture" )
				.SetDescription(
					"I02 culture independence fixture"
				)
				.SetNumber(
					NumericCapability.Columns,
					1234567
				)
				.Build();

		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture =
			CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			string french =
				TerminalDescriptionSourceRenderer.Render(
					description
				);

			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			string turkish =
				TerminalDescriptionSourceRenderer.Render(
					description
				);

			Assert.Equal(
				french,
				turkish
			);
			Assert.Contains(
				"cols#1234567,",
				french
			);

			using StringWriter writer =
				new(
					CultureInfo.GetCultureInfo( "de-DE" )
				);
			TerminalDescriptionSourceRenderer.Write(
				writer,
				description
			);
			Assert.Equal(
				french,
				writer.ToString()
			);
		}
		finally {
			CultureInfo.CurrentCulture =
				originalCulture;
			CultureInfo.CurrentUICulture =
				originalUiCulture;
		}
	}

	[Fact]
	public void Render_RejectsEffectiveStateWhichSourceCannotPreserveLosslessly() {
		TerminalDescription aliasWithoutDescription =
			new TerminalDescriptionBuilder( "i02-alias" )
				.AddAlias( "i02a" )
				.Build();
		TerminalDescription oneWordDescription =
			new TerminalDescriptionBuilder( "i02-description" )
				.SetDescription( "OneWord" )
				.Build();
		TerminalDescription negativeNumber =
			new TerminalDescriptionBuilder( "i02-negative" )
				.SetDescription( "I02 negative number fixture" )
				.SetNumber(
					NumericCapability.Columns,
					-3
				)
				.Build();
		TerminalDescription embeddedNull =
			new TerminalDescriptionBuilder( "i02-null" )
				.SetDescription( "I02 embedded null fixture" )
				.SetString(
					StringCapability.Bell,
					"a\0b"
				)
				.Build();
		TerminalDescription nonLatinOne =
			new TerminalDescriptionBuilder( "i02-unicode" )
				.SetDescription( "I02 Unicode representation fixture" )
				.SetString(
					StringCapability.Bell,
					"\u0100"
				)
				.Build();

		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					aliasWithoutDescription
				)
		);
		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					oneWordDescription
				)
		);
		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					negativeNumber
				)
		);
		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					embeddedNull
				)
		);
		Assert.Throws<InvalidOperationException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					nonLatinOne
				)
		);
	}

	[Fact]
	public void Render_ValidatesPublicArguments() {
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceRenderer.Render(
					null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceRenderer.Write(
					null!,
					TerminalProfiles.Dumb
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceRenderer.Write(
					TextWriter.Null,
					null!
				)
		);
	}

	private static void AssertRoundTrip(
		TerminalDescription expected
	) {
		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				expected
			);
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				rendered,
				"i02-rendered.ti"
			);

		Assert.False(
			parsed.HasErrors
		);
		Assert.Single(
			parsed.Document.Entries
		);

		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				expected.Name
			);
		Assert.False(
			resolved.HasErrors
		);
		Assert.NotNull(
			resolved.Entry
		);

		TerminalDescription actual =
			resolved.Entry!.ToTerminalDescription();
		AssertEquivalent(
			expected,
			actual
		);
	}

	private static void AssertEquivalent(
		TerminalDescription expected,
		TerminalDescription actual
	) {
		Assert.Equal(
			expected.Name,
			actual.Name
		);
		Assert.Equal(
			expected.Description,
			actual.Description
		);
		Assert.Equal(
			expected.Aliases.ToArray(),
			actual.Aliases.ToArray()
		);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			Assert.Equal(
				expected.GetBoolean( metadata.Capability ),
				actual.GetBoolean( metadata.Capability )
			);
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			Assert.Equal(
				expected.GetNumber( metadata.Capability ),
				actual.GetNumber( metadata.Capability )
			);
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			Assert.Equal(
				expected.GetString( metadata.Capability ),
				actual.GetString( metadata.Capability )
			);
		}

		Assert.Equal(
			expected.ExtendedCapabilities.Count,
			actual.ExtendedCapabilities.Count
		);
		foreach (
			KeyValuePair<string, TermInfoCapabilityValue> pair
			in expected.ExtendedCapabilities
		) {
			Assert.True(
				actual.ExtendedCapabilities.TryGetValue(
					pair.Key,
					out TermInfoCapabilityValue actualValue
				)
			);
			Assert.Equal(
				pair.Value,
				actualValue
			);
		}
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);

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
