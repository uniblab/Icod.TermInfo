using System.Text.RegularExpressions;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class CodingConventionTests {
	private static readonly Regex AllmanBlockBracePattern = new(
		@"(?m)(?:\)|\b(?:class|struct|interface|enum|record|namespace)\b[^\r\n]*|\b(?:else|try|finally|do))\r?\n[ \t]*\{",
		RegexOptions.CultureInvariant
	);

	private static readonly string[] InitialAuditPaths =
	[
		"src/Environment/TerminalEnvironment.cs",
		"src/Environment/TerminalSize.cs",
		"src/Environment/TerminalStandardStream.cs",
		"src/Platform/ITerminalSizeProvider.cs",
		"src/Platform/IWindowsConsoleModeApi.cs",
		"src/Platform/UnixTerminalSizeProvider.cs",
		"src/Platform/WindowsTerminalSizeProvider.cs",
		"src/Platform/WindowsVirtualTerminal.cs",
		"src/Platform/WindowsVirtualTerminalLease.cs",
		"src/TerminalDescription.cs",
		"src/TerminalDescriptionBuilder.cs",
		"tools/public-api-snapshot/Program.cs",
	];

	[Fact]
	public void AuditedCSharpFilesUseOneTrueBraceStyle() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string relativePath in InitialAuditPaths ) {
			string path = Path.Combine(
				repositoryRoot,
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar
				)
			);
			string source = File.ReadAllText(path);

			if ( AllmanBlockBracePattern.IsMatch(source) ) {
				violations.Add(relativePath);
			}
		}

		Assert.Empty(violations);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory = new(AppContext.BaseDirectory);

		while ( directory is not null ) {
			if ( File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				) ) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
