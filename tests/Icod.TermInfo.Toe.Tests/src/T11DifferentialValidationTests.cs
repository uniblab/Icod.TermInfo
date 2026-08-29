using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class T11DifferentialValidationTests {
	private static readonly string[] FixtureStems = [
		"t29-extended",
		"t29-extended32",
		"t29-legacy-alignment",
		"t29-legacy-edge",
		"t29-legacy-minimal",
	];

	[Fact]
	public async Task CheckedInNcursesCorpusListsControlledRootDeterministically() {
		string repositoryRoot = FindRepositoryRoot();
		string databaseRoot = CreateTemporaryDirectory();

		try {
			var expected = new List<string>();
			foreach ( string fixtureStem in FixtureStems ) {
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
				TerminalDescription description =
					CompiledTermInfoParser.Parse( data );

				await InstallCompiledEntryAsync(
					databaseRoot,
					description.Name,
					data
				);
				expected.Add(
					description.Name
						+ "\t"
						+ (description.Description ?? string.Empty)
				);
			}

			string expectedOutput =
				string.Join(
					Environment.NewLine,
					expected.OrderBy(
						line => line,
						StringComparer.Ordinal
					)
				)
				+ Environment.NewLine;

			CommandResult first = await RunAsync(
				[ "-s", databaseRoot ]
			);
			CommandResult second = await RunAsync(
				[ "-s", databaseRoot ]
			);

			Assert.Equal( CommandExitCodes.Success, first.Status );
			Assert.Equal( string.Empty, first.Stderr );
			Assert.Equal( expectedOutput, first.Stdout );
			Assert.Equal( first, second );
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
			"Icod.TermInfo.Toe.Tests",
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
