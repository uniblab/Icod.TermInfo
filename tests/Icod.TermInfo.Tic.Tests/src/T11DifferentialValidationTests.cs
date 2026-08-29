using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T11DifferentialValidationTests {
	[Theory]
	[InlineData( "t29-extended" )]
	[InlineData( "t29-extended32" )]
	[InlineData( "t29-legacy-alignment" )]
	[InlineData( "t29-legacy-edge" )]
	[InlineData( "t29-legacy-minimal" )]
	public async Task CheckedInNcursesTicCorpusIsSemanticallyEquivalent(
		string fixtureStem
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( fixtureStem );

		string repositoryRoot = FindRepositoryRoot();
		string sourcePath = System.IO.Path.Combine(
			repositoryRoot,
			"tests",
			"Icod.TermInfo.Tests",
			"fixtures",
			"compiled-terminfo",
			"source",
			fixtureStem + ".ti"
		);
		string compiledPath = System.IO.Path.Combine(
			repositoryRoot,
			"tests",
			"Icod.TermInfo.Tests",
			"fixtures",
			"compiled-terminfo",
			"compiled",
			fixtureStem + ".bin"
		);
		TerminalDescription expected = CompiledTermInfoParser.Parse(
			await File.ReadAllBytesAsync( compiledPath )
		);
		string outputRoot = CreateTemporaryDirectory();

		try {
			CommandResult result = await RunAsync(
				[ "-x", "-o", outputRoot, sourcePath ]
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			DirectoryTerminalDescriptionProvider provider = new( outputRoot );
			Assert.True(
				provider.TryLoad(
					expected.Name,
					out TerminalDescription? actual
				)
			);
			TerminalDescription loaded = Assert.IsType<TerminalDescription>(
				actual
			);
			TermInfoComparisonResult comparison =
				TerminalDescriptionComparer.Compare(
					expected,
					loaded
				);

			Assert.True(
				comparison.AreEqual,
				$"Icod tic changed the effective semantics of '{fixtureStem}'."
			);
		} finally {
			DeleteTemporaryDirectory( outputRoot );
		}
	}

	[Fact]
	public async Task MalformedUtf8SourceIsABoundedDeterministicInputFailure() {
		string root = CreateTemporaryDirectory();
		try {
			string sourcePath = System.IO.Path.Combine(
				root,
				"malformed.ti"
			);
			await File.WriteAllBytesAsync(
				sourcePath,
				[ 0x74, 0x65, 0x72, 0x6D, 0x7C, 0xFF, 0xFE, 0xFD ]
			);

			CommandResult first = await RunAsync(
				[ "-c", sourcePath ]
			);
			CommandResult second = await RunAsync(
				[ "-c", sourcePath ]
			);

			Assert.Equal( CommandExitCodes.Failure, first.Status );
			Assert.Equal( first.Status, second.Status );
			Assert.Equal( string.Empty, first.Stdout );
			Assert.Equal( first.Stdout, second.Stdout );
			Assert.Contains(
				"TIC0005 error",
				first.Stderr,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"input is not valid UTF-8",
				first.Stderr,
				StringComparison.Ordinal
			);
			Assert.Equal( first.Stderr, second.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
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
			"Icod.TermInfo.Tic.Tests",
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
