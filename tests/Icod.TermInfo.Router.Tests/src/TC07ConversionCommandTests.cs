using System.Text;
using Icod.CommandFramework.Diagnostics;
using CapToInfoCommand = Icod.TermInfo.CapToInfo.Command;
using InfoToCapCommand = Icod.TermInfo.InfoToCap.Command;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class TC07ConversionCommandTests {
	[Fact]
	public async Task CapToInfoConvertsStandardInputThroughExistingEngines() {
		using MemoryStream stdin =
			CreateInput(
				"demo|Demo terminal:am:co#80:\n"
			);
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await CapToInfoCommand.RunAsync(
				[ "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.Success, status );
		string output =
			ReadText( stdout );
		Assert.Contains( "demo|Demo terminal,", output );
		Assert.Contains( "am,", output );
		Assert.Contains( "cols#80,", output );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CapToInfoResolvesTcInheritanceBeforeRendering() {
		const string source =
			"base|Base terminal:co#132:\n"
			+ "child|Child terminal:am:tc=base:\n";
		using MemoryStream stdin =
			CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await CapToInfoCommand.RunAsync(
				[ "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.Success, status );
		string output =
			ReadText( stdout );
		Assert.Contains( "child|Child terminal,", output );
		Assert.Contains( "cols#132,", output );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task InfoToCapConvertsStandardInputThroughExistingEngines() {
		const string source =
			"demo|Demo terminal,\n"
			+ "    am,\n"
			+ "    cols#80,\n";
		using MemoryStream stdin =
			CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await InfoToCapCommand.RunAsync(
				[ "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.Success, status );
		string output =
			ReadText( stdout );
		Assert.Contains( "demo|Demo terminal:", output );
		Assert.Contains( ":am:", output );
		Assert.Contains( ":co#80:", output );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task InfoToCapRejectsUnrepresentableExtendedCapability() {
		const string source =
			"demo|Demo terminal,\n"
			+ "    VendorFlag,\n";
		using MemoryStream stdin =
			CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await InfoToCapCommand.RunAsync(
				[ "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.NotEmpty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "captoinfo" )]
	[InlineData( "infotocap" )]
	public async Task RouterExecutesConversionCommands(
		string commandName
	) {
		string source =
			( commandName == "captoinfo" )
				? "demo|Demo terminal:am:co#80:\n"
				: "demo|Demo terminal,\n    am,\n    cols#80,\n"
			;
		using MemoryStream stdin =
			CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await Command.RunAsync(
				[ commandName, "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "demo", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "captoinfo" )]
	[InlineData( "infotocap" )]
	public async Task ConversionCommandsRejectInvalidWidth(
		string commandName
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status =
			await Command.RunAsync(
				[ commandName, "-w", "1", "-" ],
				stdin,
				stdout,
				stderr
			);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "invalid line width", ReadText( stderr ) );
	}

	private static MemoryStream CreateInput(
		string text
	) {
		ArgumentNullException.ThrowIfNull( text );

		return new MemoryStream(
			new UTF8Encoding( false ).GetBytes( text )
		);
	}

	private static string ReadText(
		MemoryStream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}
}
