using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class MI06JsonHardeningCommandTests {
	[Fact]
	public async Task LargeEscapedJsonIsCultureInvariantAndRendererExact() {
		string root = CreateTemporaryDirectory();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			TerminalDescription description =
				new TerminalDescriptionBuilder( "mi06-command-large" )
					.SetDescription( "MI06 command large and escaped" )
					.SetExtendedString(
						"XMI06",
						new string( 'x', 8_192 )
							+ "\u001b[31m\n\\\"\u00ff"
					)
					.Build();
			CompiledTermInfoDatabaseWriter.Write( root, description );
			TerminalDescription inspected =
				TermInfoDatabaseInspector
					.InspectDirectory( root )
					.Entries
					.Single( entry => entry.Name == description.Name )
					.Terminal;
			string expected =
				TermInfoJsonRenderer.Render( inspected ) + "\n";

			foreach ( string cultureName in new[] { "ar-SA", "tr-TR" } ) {
				CultureInfo.CurrentCulture =
					CultureInfo.GetCultureInfo( cultureName );
				CultureInfo.CurrentUICulture =
					CultureInfo.GetCultureInfo( cultureName );
				CommandResult result = await RunAsync(
					"--json",
					"-A",
					root,
					description.Name
				);

				Assert.Equal( 0, result.Status );
				Assert.Equal( expected, result.Stdout );
				Assert.Equal( string.Empty, result.Stderr );
				using JsonDocument document =
					JsonDocument.Parse( result.Stdout );
				Assert.Equal(
					"terminalDescription",
					document.RootElement.GetProperty( "documentKind" ).GetString()
				);
			}
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
			DeleteTemporaryDirectory( root );
		}
	}

	private static async Task<CommandResult> RunAsync(
		params string[] args
	) {
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
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-infocmp-mi06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
