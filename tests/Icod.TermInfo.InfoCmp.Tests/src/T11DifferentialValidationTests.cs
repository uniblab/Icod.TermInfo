using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class T11DifferentialValidationTests {
	[Theory]
	[InlineData( "t29-extended" )]
	[InlineData( "t29-extended32" )]
	[InlineData( "t29-legacy-alignment" )]
	[InlineData( "t29-legacy-edge" )]
	[InlineData( "t29-legacy-minimal" )]
	public async Task CheckedInNcursesEntriesRenderToEquivalentEffectiveSource(
		string fixtureStem
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( fixtureStem );

		string repositoryRoot = FindRepositoryRoot();
		string compiledPath = System.IO.Path.Combine(
			repositoryRoot,
			"tests",
			"Icod.TermInfo.Tests",
			"fixtures",
			"compiled-terminfo",
			"compiled",
			fixtureStem + ".bin"
		);
		byte[] data = await File.ReadAllBytesAsync( compiledPath );
		TerminalDescription expected = CompiledTermInfoParser.Parse( data );
		string databaseRoot = CreateTemporaryDirectory();

		try {
			await InstallCompiledEntryAsync(
				databaseRoot,
				expected.Name,
				data
			);

			CommandResult command = await RunAsync(
				[ "-A", databaseRoot, "-x", expected.Name ]
			);

			Assert.Equal( CommandExitCodes.Success, command.Status );
			Assert.Equal( string.Empty, command.Stderr );

			TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
				command.Stdout,
				"t11-infocmp-output.ti"
			);
			Assert.False( parsed.HasErrors );

			TermInfoSourceResolveResult resolved =
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					expected.Name
				);
			Assert.False( resolved.HasErrors );
			TermInfoSourceResolvedEntry rendered =
				Assert.IsType<TermInfoSourceResolvedEntry>(
					resolved.Entry
				);
			TermInfoComparisonResult comparison =
				TerminalDescriptionComparer.Compare(
					expected,
					rendered.ToTerminalDescription()
				);

			Assert.True(
				comparison.AreEqual,
				$"infocmp rendering changed the effective semantics of '{fixtureStem}'."
			);
		} finally {
			DeleteTemporaryDirectory( databaseRoot );
		}
	}

	private static async Task InstallCompiledEntryAsync(
		string root,
		string name,
		byte[] data
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( data );

		string directory = System.IO.Path.Combine(
			root,
			name[ 0 ].ToString()
		);
		Directory.CreateDirectory( directory );
		await File.WriteAllBytesAsync(
			System.IO.Path.Combine(
				directory,
				name
			),
			data
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

	private static string ReadText(
		MemoryStream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"Icod.TermInfo.InfoCmp.Tests",
			"T11-" + Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		if ( Directory.Exists( path ) ) {
			Directory.Delete(
				path,
				recursive: true
			);
		}
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new(
			AppContext.BaseDirectory
		);
		while ( current is not null ) {
			if (
				File.Exists(
					System.IO.Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
