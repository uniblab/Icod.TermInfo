using System.Globalization;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class T06OneTerminalInspectionTests {
	[Fact]
	public async Task ExplicitNameRendersEffectiveStandardCapabilities() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);

			CommandResult result =
				await RunAsync(
					[ "-A", root, "demo" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.StartsWith(
				"demo|demo-alias|Demo terminal,\n",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains( "    am,\n", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "    cols#80,\n", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "xDemo", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task AliasAcquisitionRendersCanonicalIdentity() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);

			CommandResult result =
				await RunAsync(
					[ "-A", root, "demo-alias" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.StartsWith(
				"demo|demo-alias|",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task TermFallbackUsesOperandNameFromEnvironment() {
		string root = CreateTemporaryDirectory();
		string? previousTerm = Environment.GetEnvironmentVariable( "TERM" );
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);
			Environment.SetEnvironmentVariable( "TERM", "demo" );

			CommandResult result =
				await RunAsync(
					[ "-A", root ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.StartsWith(
				"demo|",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			Environment.SetEnvironmentVariable( "TERM", previousTerm );
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task MissingTermIsControlledOperationalFailure() {
		string root = CreateTemporaryDirectory();
		string? previousTerm = Environment.GetEnvironmentVariable( "TERM" );
		try {
			Environment.SetEnvironmentVariable( "TERM", null );

			CommandResult result =
				await RunAsync(
					[ "-A", root ]
				);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0001 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			Environment.SetEnvironmentVariable( "TERM", previousTerm );
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task MissingEntryIsControlledOperationalFailure() {
		string root = CreateTemporaryDirectory();
		try {
			CommandResult result =
				await RunAsync(
					[ "-A", root, "missing" ]
				);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0002 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task MalformedCompiledEntryIsControlledOperationalFailure() {
		string root = CreateTemporaryDirectory();
		try {
			string directory =
				System.IO.Path.Combine(
					root,
					((byte)'b').ToString(
						"x2",
						CultureInfo.InvariantCulture
					)
				);
			Directory.CreateDirectory( directory );
			await File.WriteAllBytesAsync(
				System.IO.Path.Combine( directory, "bad" ),
				[ 0x00, 0x01, 0x02, 0x03 ]
			);

			CommandResult result =
				await RunAsync(
					[ "-A", root, "bad" ]
				);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0003 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task SingleLineOptionEmitsOneLogicalLine() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);

			CommandResult result =
				await RunAsync(
					[ "-A", root, "-0", "demo" ]
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( 1, result.Stdout.Count( character => character == '\n' ) );
			Assert.Contains( " am,", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( " cols#80,", result.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task OneCapabilityPerLineDoesNotWrapLongString() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription description =
				new TerminalDescriptionBuilder( "long" )
					.SetDescription( "Long string terminal" )
					.SetString(
						StringCapability.ClearScreen,
						new string( 'x', 120 )
					)
					.Build();
			Publish( root, description );

			CommandResult result =
				await RunAsync(
					[ "-A", root, "-1", "long" ]
				);
			string[] lines = result.Stdout.Split( '\n' );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( 3, lines.Length );
			Assert.StartsWith(
				"    clear=",
				lines[ 1 ],
				StringComparison.Ordinal
			);
			Assert.True( lines[ 1 ].Length > 80 );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task WidthOptionControlsCanonicalWrappingBoundary() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription description =
				new TerminalDescriptionBuilder( "width" )
					.SetDescription( "Width test terminal" )
					.SetString(
						StringCapability.ClearScreen,
						new string( 'x', 96 )
					)
					.Build();
			Publish( root, description );

			CommandResult result =
				await RunAsync(
					[ "-A", root, "-w", "24", "width" ]
				);
			string[] lines =
				result.Stdout.Split(
					'\n',
					StringSplitOptions.RemoveEmptyEntries
				);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.True( lines.Length > 2 );
			foreach ( string line in lines.Skip( 1 ) ) {
				Assert.InRange( line.Length, 1, 24 );
			}
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Theory]
	[InlineData( "d", TerminalDescriptionSourceCapabilityOrder.Database )]
	[InlineData( "i", TerminalDescriptionSourceCapabilityOrder.TermInfoName )]
	[InlineData( "l", TerminalDescriptionSourceCapabilityOrder.LongName )]
	[InlineData( "c", TerminalDescriptionSourceCapabilityOrder.TermcapCode )]
	public async Task SortKeysUseRuntimeMetadata(
		string key,
		TerminalDescriptionSourceCapabilityOrder order
	) {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescriptionBuilder builder =
				new TerminalDescriptionBuilder( "order" )
					.SetDescription( "Ordering test terminal" );
			foreach (
				StandardCapabilityMetadata<BooleanCapability> metadata
				in StandardCapabilityCatalog.BooleanCapabilities
			) {
				builder.SetBoolean( metadata.Capability );
			}
			Publish( root, builder.Build() );

			CommandResult result =
				await RunAsync(
					[ "-A", root, "-1", "-s", key, "order" ]
				);
			string[] actual = ExtractCapabilityNames( result.Stdout );
			string[] expected =
				OrderBooleanMetadata( order )
					.Select( item => item.ShortName )
					.ToArray();

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( expected, actual );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ExtendedCapabilitiesRequireExplicitXOption() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);

			CommandResult standard =
				await RunAsync(
					[ "-A", root, "demo" ]
				);
			CommandResult extended =
				await RunAsync(
					[ "-A", root, "-x", "demo" ]
				);

			Assert.DoesNotContain( "xDemo", standard.Stdout, StringComparison.Ordinal );
			Assert.Contains( "xDemo#7", extended.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task RenderingIsCultureIndependent() {
		string root = CreateTemporaryDirectory();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try {
			TerminalDescriptionBuilder builder =
				new TerminalDescriptionBuilder( "culture" )
					.SetDescription( "Culture test terminal" );
			foreach (
				StandardCapabilityMetadata<BooleanCapability> metadata
				in StandardCapabilityCatalog.BooleanCapabilities
			) {
				builder.SetBoolean( metadata.Capability );
			}
			Publish( root, builder.Build() );

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CommandResult turkish =
				await RunAsync(
					[ "-A", root, "-1", "-s", "l", "culture" ]
				);
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "en-US" );
			CommandResult english =
				await RunAsync(
					[ "-A", root, "-1", "-s", "l", "culture" ]
				);

			Assert.Equal( turkish.Stdout, english.Stdout );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ExtendedOutputIsIndependentOfDescriptionInsertionOrder() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription first =
				new TerminalDescriptionBuilder( "same" )
					.SetDescription( "Insertion test terminal" )
					.SetExtendedString( "zText", "z" )
					.SetExtendedBoolean( "aFlag" )
					.SetExtendedNumber( "mNumber", 3 )
					.Build();
			TerminalDescription second =
				new TerminalDescriptionBuilder( "same" )
					.SetDescription( "Insertion test terminal" )
					.SetExtendedNumber( "mNumber", 3 )
					.SetExtendedBoolean( "aFlag" )
					.SetExtendedString( "zText", "z" )
					.Build();
			Publish( firstRoot, first );
			Publish( secondRoot, second );

			CommandResult firstResult =
				await RunAsync(
					[ "-A", firstRoot, "-x", "same" ]
				);
			CommandResult secondResult =
				await RunAsync(
					[ "-A", secondRoot, "-x", "same" ]
				);

			Assert.Equal( firstResult.Stdout, secondResult.Stdout );
		} finally {
			DeleteTemporaryDirectory( firstRoot );
			DeleteTemporaryDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task DatabaseReportingUsesInspectionDiscoveryModel() {
		CommandResult result =
			await RunAsync(
				[ "-D" ]
			);

		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Equal( string.Empty, result.Stderr );
	}

	[Theory]
	[InlineData( "-0", "-1" )]
	[InlineData( "-w", "0" )]
	[InlineData( "-s", "z" )]
	public async Task InvalidPresentationOptionsAreUsageErrors(
		string first,
		string second
	) {
		CommandResult result =
			await RunAsync(
				[ first, second ]
			);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task RedirectedOutputUsesCallerOwnedStream() {
		string root = CreateTemporaryDirectory();
		try {
			Publish(
				root,
				CreateRepresentativeDescription()
			);
			using var stdin = new MemoryStream();
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-A", root, "demo" ],
				stdin,
				stdout,
				stderr
			);
			stdout.WriteByte( 0 );
			stderr.WriteByte( 0 );

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.True( stdout.CanWrite );
			Assert.True( stderr.CanWrite );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task CancellationBeforeAcquisitionReturnsCanceledWithoutOutput() {
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "demo" ],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	private static TerminalDescription CreateRepresentativeDescription() {
		return new TerminalDescriptionBuilder( "demo" )
			.AddAlias( "demo-alias" )
			.SetDescription( "Demo terminal" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.SetString( StringCapability.ClearScreen, "clear-sequence" )
			.SetExtendedNumber( "xDemo", 7 )
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
					int separator = line.IndexOfAny( [ '#', '=' ] );
					int comma = line.IndexOf( ',' );
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

	private static string CreateTemporaryDirectory() {
		string path =
			System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				$"icod-terminfo-infocmp-t06-{Guid.NewGuid():N}"
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
