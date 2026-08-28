using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class T06RendererControlsTests {
	[Fact]
	public void DefaultOptionsPreserveFrozenRendererOutput() {
		TerminalDescription description =
			CreateRepresentativeDescription();

		string frozen =
			TerminalDescriptionSourceRenderer.Render(
				description
			);
		string configured =
			TerminalDescriptionSourceRenderer.Render(
				description,
				new TerminalDescriptionSourceRendererOptions()
			);

		Assert.Equal( frozen, configured );
	}

	[Fact]
	public void SingleLineLayoutEmitsOneLogicalLine() {
		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				CreateRepresentativeDescription(),
				new TerminalDescriptionSourceRendererOptions(
					80,
					TerminalDescriptionSourceLayout.SingleLine
				)
			);

		Assert.True(
			rendered.EndsWith(
				"\n",
				StringComparison.Ordinal
			)
		);
		Assert.DoesNotContain( "\r", rendered, StringComparison.Ordinal );
		Assert.Equal( 1, rendered.Count( character => character == '\n' ) );
		Assert.Contains( " am,", rendered, StringComparison.Ordinal );
		Assert.Contains( " cols#132,", rendered, StringComparison.Ordinal );
		Assert.Contains( " clear=", rendered, StringComparison.Ordinal );
	}

	[Fact]
	public void OneCapabilityPerLineNeverCreatesContinuationLines() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "t06-one" )
				.SetDescription( "T06 one capability fixture" )
				.SetString(
					StringCapability.ClearScreen,
					new string( 'x', 120 )
				)
				.Build();

		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				description,
				new TerminalDescriptionSourceRendererOptions(
					20,
					TerminalDescriptionSourceLayout.OneCapabilityPerLine
				)
			);

		string[] lines = rendered.Split( '\n' );
		Assert.Equal( 3, lines.Length );
		Assert.True(
			lines[ 1 ].StartsWith(
				"    clear=",
				StringComparison.Ordinal
			)
		);
		Assert.True( lines[ 1 ].Length > 20 );
		Assert.Equal( string.Empty, lines[ 2 ] );
	}

	[Fact]
	public void CanonicalLineWidthControlsStringWrapping() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "t06-width" )
				.SetDescription( "T06 width fixture" )
				.SetString(
					StringCapability.ClearScreen,
					new string( 'x', 96 )
				)
				.Build();

		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				description,
				new TerminalDescriptionSourceRendererOptions( 24 )
			);
		string[] lines =
			rendered.Split(
				'\n',
				StringSplitOptions.RemoveEmptyEntries
			);

		Assert.True( lines.Length > 2 );
		foreach ( string line in lines.Skip( 1 ) ) {
			Assert.InRange( line.Length, 1, 24 );
		}
	}

	[Theory]
	[InlineData( TerminalDescriptionSourceCapabilityOrder.Database )]
	[InlineData( TerminalDescriptionSourceCapabilityOrder.TermInfoName )]
	[InlineData( TerminalDescriptionSourceCapabilityOrder.LongName )]
	[InlineData( TerminalDescriptionSourceCapabilityOrder.TermcapCode )]
	public void CapabilityOrderingMatchesRequestedMetadataKey(
		TerminalDescriptionSourceCapabilityOrder order
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( "t06-order" )
				.SetDescription( "T06 ordering fixture" );
		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			builder.SetBoolean( metadata.Capability );
		}

		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				builder.Build(),
				new TerminalDescriptionSourceRendererOptions(
					80,
					TerminalDescriptionSourceLayout.OneCapabilityPerLine,
					order,
					includeExtendedCapabilities: false
				)
			);
		string[] actual =
			ExtractCapabilityNames( rendered );
		string[] expected =
			OrderBooleanMetadata( order )
				.Select( item => item.ShortName )
				.ToArray();

		Assert.Equal( expected, actual );
	}

	[Fact]
	public void ExtendedCapabilitiesCanBeIncludedOrExcluded() {
		TerminalDescription description =
			CreateRepresentativeDescription();

		string standardOnly =
			TerminalDescriptionSourceRenderer.Render(
				description,
				new TerminalDescriptionSourceRendererOptions(
					80,
					includeExtendedCapabilities: false
				)
			);
		string withExtensions =
			TerminalDescriptionSourceRenderer.Render(
				description,
				new TerminalDescriptionSourceRendererOptions()
			);

		Assert.DoesNotContain( "xT06", standardOnly, StringComparison.Ordinal );
		Assert.Contains( "xT06#7", withExtensions, StringComparison.Ordinal );
	}

	[Fact]
	public void ConfiguredOrderingIsCultureIndependent() {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( "t06-culture" )
				.SetDescription( "T06 culture fixture" );
		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			builder.SetBoolean( metadata.Capability );
		}
		TerminalDescription description = builder.Build();
		TerminalDescriptionSourceRendererOptions options =
			new(
				80,
				TerminalDescriptionSourceLayout.OneCapabilityPerLine,
				TerminalDescriptionSourceCapabilityOrder.LongName,
				includeExtendedCapabilities: false
			);

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			string turkish =
				TerminalDescriptionSourceRenderer.Render(
					description,
					options
				);
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "en-US" );
			string english =
				TerminalDescriptionSourceRenderer.Render(
					description,
					options
				);

			Assert.Equal( turkish, english );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[Fact]
	public void ExtendedOrderingIgnoresBuilderInsertionOrder() {
		TerminalDescription first =
			new TerminalDescriptionBuilder( "t06-insertion" )
				.SetDescription( "T06 insertion fixture" )
				.SetExtendedString( "zText", "z" )
				.SetExtendedBoolean( "aFlag" )
				.SetExtendedNumber( "mNumber", 3 )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "t06-insertion" )
				.SetDescription( "T06 insertion fixture" )
				.SetExtendedNumber( "mNumber", 3 )
				.SetExtendedBoolean( "aFlag" )
				.SetExtendedString( "zText", "z" )
				.Build();
		TerminalDescriptionSourceRendererOptions options =
			new(
				80,
				TerminalDescriptionSourceLayout.OneCapabilityPerLine
			);

		Assert.Equal(
			TerminalDescriptionSourceRenderer.Render( first, options ),
			TerminalDescriptionSourceRenderer.Render( second, options )
		);
	}

	[Fact]
	public void RendererOptionsRejectInvalidValues() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourceRendererOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourceRendererOptions(
				80,
				(TerminalDescriptionSourceLayout)99
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TerminalDescriptionSourceRendererOptions(
				80,
				TerminalDescriptionSourceLayout.Canonical,
				(TerminalDescriptionSourceCapabilityOrder)99
			)
		);
	}

	private static TerminalDescription CreateRepresentativeDescription() {
		return new TerminalDescriptionBuilder( "t06-demo" )
			.AddAlias( "t06-alias" )
			.SetDescription( "T06 representative terminal" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 132 )
			.SetString( StringCapability.ClearScreen, "clear-sequence" )
			.SetExtendedNumber( "xT06", 7 )
			.Build();
	}

	private static IEnumerable<StandardCapabilityMetadata<BooleanCapability>>
		OrderBooleanMetadata(
			TerminalDescriptionSourceCapabilityOrder order
		) {
		return order switch {
			TerminalDescriptionSourceCapabilityOrder.Database =>
				StandardCapabilityCatalog.BooleanCapabilities,
			TerminalDescriptionSourceCapabilityOrder.TermInfoName =>
				StandardCapabilityCatalog.BooleanCapabilities
					.OrderBy(
						item => item.ShortName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			TerminalDescriptionSourceCapabilityOrder.LongName =>
				StandardCapabilityCatalog.BooleanCapabilities
					.OrderBy(
						item => item.LongName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			TerminalDescriptionSourceCapabilityOrder.TermcapCode =>
				StandardCapabilityCatalog.BooleanCapabilities
					.OrderBy(
						item => item.TermcapCode,
						StringComparer.Ordinal
					)
					.ThenBy(
						item => item.ShortName,
						StringComparer.Ordinal
					)
					.ThenBy( item => item.BinaryIndex ),
			_ => throw new ArgumentOutOfRangeException(
				nameof( order )
			),
		};
	}

	private static string[] ExtractCapabilityNames(
		string rendered
	) {
		ArgumentNullException.ThrowIfNull( rendered );

		return rendered
			.Split(
				'\n',
				StringSplitOptions.RemoveEmptyEntries
			)
			.Skip( 1 )
			.Select( line => line.Trim() )
			.Select(
				line => {
					int separator =
						line.IndexOfAny( [ '#', '=' ] );
					int comma =
						line.IndexOf( ',' );
					int end =
						(separator >= 0)
							? separator
							: comma
						;
					return line[ ..end ];
				}
			)
			.ToArray();
	}
}
