using System.Text.RegularExpressions;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class CodingConventionTests {
	private static readonly Regex AllmanBlockBracePattern = new(
		@"(?m)(?:\)|\b(?:class|struct|interface|enum|record|namespace)\b[^\r\n]*|\b(?:else|catch|try|finally|do))\r?\n[ \t]*\{",
		RegexOptions.CultureInvariant
	);

	[Fact]
	public void RepositoryCSharpFilesUseOneTrueBraceStyle() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = File.ReadAllText( path );

			if ( AllmanBlockBracePattern.IsMatch( source ) ) {
				violations.Add(
					Path.GetRelativePath( repositoryRoot, path )
						.Replace( Path.DirectorySeparatorChar, '/' )
				);
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using legacy block-brace placement:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	private static IEnumerable<string> EnumerateCSharpFiles(
		string repositoryRoot
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( repositoryRoot );

		string binSegment =
			$"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
		string objSegment =
			$"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";

		return Directory
			.EnumerateFiles(
				repositoryRoot,
				"*.cs",
				SearchOption.AllDirectories
			)
			.Where(
				path =>
					!path.Contains( binSegment, StringComparison.OrdinalIgnoreCase )
					&& !path.Contains( objSegment, StringComparison.OrdinalIgnoreCase )
			)
			.OrderBy(
				path => path,
				StringComparer.Ordinal
			);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory = new( AppContext.BaseDirectory );

		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
