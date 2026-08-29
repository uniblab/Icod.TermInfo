using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class T09ToeSourceDependencyAndDuplicateSemanticsTests {
	[Fact]
	public async Task SourceWithoutDependenciesReturnsSuccessWithoutOutput() {
		string sourcePath = CreateSourceFile(
			"standalone|Standalone terminal,\n"
			+ "\tcols#80,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task DuplicateSourceIdentityUsesFirstMatchDeterministically() {
		string sourcePath = CreateSourceFile(
			"first|shared-alias|First terminal,\n"
			+ "\tcols#80,\n"
			+ "second|shared-alias|Second terminal,\n"
			+ "\tcols#90,\n"
			+ "child|Child terminal,\n"
			+ "\tuse=shared-alias,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"child\tfirst{Environment.NewLine}",
				result.Stdout
			);
			Assert.Contains( "TIS0026", result.Stderr, StringComparison.Ordinal );
			Assert.Contains( "warning", result.Stderr, StringComparison.Ordinal );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task MalformedSourceReturnsFailureWithoutGraphOutput() {
		string sourcePath = CreateSourceFile(
			"broken|Broken terminal,\n"
			+ "\tcols#not-a-number,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "TIS0011", result.Stderr, StringComparison.Ordinal );
			Assert.Contains( "error", result.Stderr, StringComparison.Ordinal );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task ForwardDependenciesPreserveSourceUseOrder() {
		string sourcePath = CreateSourceFile(
			"left|Left parent,\n"
			+ "\tcols#80,\n"
			+ "right|Right parent,\n"
			+ "\tcols#90,\n"
			+ "child|Child terminal,\n"
			+ "\tuse=left,\n"
			+ "\tuse=right,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"child\tleft{Environment.NewLine}"
					+ $"child\tright{Environment.NewLine}",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task AliasReferencesReportCanonicalParentIdentity() {
		string sourcePath = CreateSourceFile(
			"base|base-alias|Base terminal,\n"
			+ "\tcols#80,\n"
			+ "child|Child terminal,\n"
			+ "\tuse=base-alias,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"child\tbase{Environment.NewLine}",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task ReverseDependenciesUseCompleteDocumentAndStableSourceOrder() {
		string sourcePath = CreateSourceFile(
			"child-b|Child B,\n"
			+ "\tuse=base-alias,\n"
			+ "child-a|Child A,\n"
			+ "\tuse=base-alias,\n"
			+ "base|base-alias|Base terminal,\n"
			+ "\tcols#80,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-U",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"base\tchild-b{Environment.NewLine}"
					+ $"base\tchild-a{Environment.NewLine}",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task MissingParentRemainsVisibleAndReturnsFailure() {
		string sourcePath = CreateSourceFile(
			"child|Missing parent test,\n"
			+ "\tuse=missing-parent,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal(
				$"child\tmissing-parent{Environment.NewLine}",
				result.Stdout
			);
			Assert.Contains( "TIS", result.Stderr, StringComparison.Ordinal );
			Assert.Contains(
				"could not be found",
				result.Stderr,
				StringComparison.Ordinal
			);
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task InheritanceCycleReportsFailureAfterDependencyOutput() {
		string sourcePath = CreateSourceFile(
			"a|Cycle A,\n"
			+ "\tuse=b,\n"
			+ "b|Cycle B,\n"
			+ "\tuse=a,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal(
				$"a\tb{Environment.NewLine}"
					+ $"b\ta{Environment.NewLine}",
				result.Stdout
			);
			Assert.Contains(
				"Inheritance cycle detected",
				result.Stderr,
				StringComparison.Ordinal
			);
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task SourceModeRejectsListingOptions() {
		string sourcePath = CreateSourceFile(
			"base|Base terminal,\n"
			+ "\tcols#80,\n"
		);

		try {
			CommandResult result = await RunAsync(
				"-a",
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.UsageError, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains(
				"standalone",
				result.Stderr,
				StringComparison.Ordinal
			);
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	[Fact]
	public async Task EqualDuplicateDescriptionsUseComparerMarker() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			TerminalDescription first = CreateTerminal(
				"shared",
				80
			);
			TerminalDescription second = CreateTerminal(
				"shared",
				80
			);
			Publish( firstRoot, first );
			Publish( secondRoot, second );

			CommandResult result = await RunAsync(
				"-a",
				"-s",
				firstRoot,
				secondRoot
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				$"# Icod duplicate shared: semantically equal to {System.IO.Path.GetFullPath( firstRoot )}",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task DifferentDuplicateDescriptionsUseComparerMarker() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"shared",
					80
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"shared",
					132
				)
			);

			CommandResult result = await RunAsync(
				"-a",
				"-s",
				firstRoot,
				secondRoot
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				$"# Icod duplicate shared: semantically different from {System.IO.Path.GetFullPath( firstRoot )}",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task PreCanceledSourceAnalysisProducesNoOutput() {
		string sourcePath = CreateSourceFile(
			"base|Base terminal,\n"
			+ "\tcols#80,\n"
		);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		try {
			CommandResult result = await RunAsync(
				cancellation.Token,
				"-u",
				sourcePath
			);

			Assert.Equal( CommandExitCodes.Canceled, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			System.IO.File.Delete( sourcePath );
		}
	}

	private static TerminalDescription CreateTerminal(
		string name,
		int columns
	) {
		return new TerminalDescriptionBuilder( name )
			.SetDescription( "Duplicate semantics fixture" )
			.SetNumber(
				NumericCapability.Columns,
				columns
			)
			.Build();
	}

	private static void Publish(
		string root,
		TerminalDescription terminal
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( terminal );

		CompiledTermInfoDatabaseWriter.Write(
			root,
			terminal
		);
	}

	private static string CreateSourceFile( string source ) {
		ArgumentNullException.ThrowIfNull( source );

		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-toe-t09-{Guid.NewGuid():N}.ti"
		);
		System.IO.File.WriteAllText(
			path,
			source,
			new UTF8Encoding( false )
		);
		return path;
	}

	private static string CreateTemporaryDirectory() {
		string root = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-toe-t09-{Guid.NewGuid():N}"
		);
		System.IO.Directory.CreateDirectory( root );
		return root;
	}

	private static void DeleteDirectory( string root ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );

		if ( System.IO.Directory.Exists( root ) ) {
			System.IO.Directory.Delete(
				root,
				recursive: true
			);
		}
	}

	private static Task<CommandResult> RunAsync(
		params string[] args
	) {
		return RunAsync(
			CancellationToken.None,
			args
		);
	}

	private static async Task<CommandResult> RunAsync(
		CancellationToken cancellationToken,
		params string[] args
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr,
			cancellationToken
		);

		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
