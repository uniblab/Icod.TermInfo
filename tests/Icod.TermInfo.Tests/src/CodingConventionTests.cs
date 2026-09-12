using System.Text.RegularExpressions;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class CodingConventionTests {
	private static readonly Regex AllmanBlockBracePattern = new(
		@"(?m)(?:\)|\b(?:class|struct|interface|enum|record|namespace)\b[^\r\n]*|\b(?:else|catch|try|finally|do))\r?\n[ \t]*\{",
		RegexOptions.CultureInvariant
	);
	private static readonly Regex SpaceIndentationPattern = new(
		@"(?m)^ +\S",
		RegexOptions.CultureInvariant
	);
	private static readonly Regex ParenthesizedControlFlowPattern = new(
		@"(?m)^[\t ]*(?:\}[\t ]+else[\t ]+)?(?:(?:await[\t ]+)?(?:foreach|using)|if|for|while|lock|fixed)[\t ]*\(",
		RegexOptions.CultureInvariant
	);
	private static readonly Regex ElseControlFlowPattern = new(
		@"(?m)^[\t ]*(?:\}[\t ]+)?else\b(?![\t ]+if\b)",
		RegexOptions.CultureInvariant
	);
	private static readonly Regex DoControlFlowPattern = new(
		@"(?m)^[\t ]*do\b",
		RegexOptions.CultureInvariant
	);
	private static readonly Regex ControlKeywordWithoutSpaceBeforeParenthesisPattern = new(
		@"\b(?:if|for|foreach|while|switch|catch|using|lock|fixed)\(",
		RegexOptions.CultureInvariant
	);

	[Fact]
	public void RepositoryCSharpFilesUseOneTrueBraceStyle() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = File.ReadAllText( path );

			if ( AllmanBlockBracePattern.IsMatch( source ) ) {
				violations.Add( GetRelativePath( repositoryRoot, path ) );
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using legacy block-brace placement:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	[Fact]
	public void RepositoryCSharpFilesUseTabsForIndentation() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = File.ReadAllText( path );

			if ( SpaceIndentationPattern.IsMatch( source ) ) {
				violations.Add( GetRelativePath( repositoryRoot, path ) );
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using spaces for indentation:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	[Fact]
	public void RepositoryCSharpControlFlowBodiesUseBraces() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = MaskNonCode( File.ReadAllText( path ) );

			if ( ContainsUnbracedControlFlow( source ) ) {
				violations.Add( GetRelativePath( repositoryRoot, path ) );
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using unbraced control-flow bodies:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	[Fact]
	public void RepositoryCSharpCallsAndControlsUseRepositoryParenthesisSpacing() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = MaskNonCode( File.ReadAllText( path ) );

			if ( ContainsParenthesisSpacingViolation( source ) ) {
				violations.Add( GetRelativePath( repositoryRoot, path ) );
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using non-repository call/control parenthesis spacing:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	private static bool ContainsUnbracedControlFlow( string source ) {
		ArgumentNullException.ThrowIfNull( source );

		foreach ( Match match in ParenthesizedControlFlowPattern.Matches( source ) ) {
			int openParenthesis =
				match.Index + match.Value.LastIndexOf( '(' );
			int closeParenthesis = FindMatchingParenthesis( source, openParenthesis );
			if ( closeParenthesis < 0 ) {
				continue;
			}

			int next = FindNextNonWhitespace( source, closeParenthesis + 1 );
			if ( next >= 0 && source[next] == '{' ) {
				continue;
			}

			if (
				next >= 0
				&& source[next] == ';'
				&& match.Value.TrimStart().StartsWith( "while", StringComparison.Ordinal )
			) {
				int previous = FindPreviousNonWhitespace( source, match.Index - 1 );
				if ( previous >= 0 && source[previous] == '}' ) {
					continue;
				}
			}

			return true;
		}

		foreach ( Match match in ElseControlFlowPattern.Matches( source ) ) {
			int next = FindNextNonWhitespace( source, match.Index + match.Length );
			if ( next < 0 || source[next] != '{' ) {
				return true;
			}
		}

		foreach ( Match match in DoControlFlowPattern.Matches( source ) ) {
			int next = FindNextNonWhitespace( source, match.Index + match.Length );
			if ( next < 0 || source[next] != '{' ) {
				return true;
			}
		}

		return false;
	}

	private static bool ContainsParenthesisSpacingViolation( string source ) {
		ArgumentNullException.ThrowIfNull( source );

		if ( ControlKeywordWithoutSpaceBeforeParenthesisPattern.IsMatch( source ) ) {
			return true;
		}

		foreach ( Match match in ParenthesizedControlFlowPattern.Matches( source ) ) {
			int openParenthesis =
				match.Index + match.Value.LastIndexOf( '(' );
			if ( !ParenthesisInteriorUsesRepositorySpacing( source, openParenthesis ) ) {
				return true;
			}
		}

		for ( int i = 0; i < source.Length; i++ ) {
			if ( source[i] != '(' || !IsCallLikeOpenParenthesis( source, i ) ) {
				continue;
			}

			if ( !ParenthesisInteriorUsesRepositorySpacing( source, i ) ) {
				return true;
			}
		}

		return false;
	}

	private static bool IsCallLikeOpenParenthesis(
		string source,
		int openParenthesis
	) {
		ArgumentNullException.ThrowIfNull( source );

		if ( openParenthesis <= 0 ) {
			return false;
		}

		char previous = source[openParenthesis - 1];
		return char.IsLetterOrDigit( previous )
			|| previous == '_'
			|| previous == '>'
			|| previous == ']'
			|| previous == ')'
			|| previous == '!';
	}

	private static bool ParenthesisInteriorUsesRepositorySpacing(
		string source,
		int openParenthesis
	) {
		ArgumentNullException.ThrowIfNull( source );

		int closeParenthesis = FindMatchingParenthesis( source, openParenthesis );
		if ( closeParenthesis < 0 || closeParenthesis == openParenthesis + 1 ) {
			return true;
		}

		return char.IsWhiteSpace( source[openParenthesis + 1] )
			&& char.IsWhiteSpace( source[closeParenthesis - 1] );
	}

	private static int FindMatchingParenthesis(
		string source,
		int openParenthesis
	) {
		ArgumentNullException.ThrowIfNull( source );

		int depth = 0;
		for ( int i = openParenthesis; i < source.Length; i++ ) {
			if ( source[i] == '(' ) {
				depth++;
			} else if ( source[i] == ')' ) {
				depth--;
				if ( depth == 0 ) {
					return i;
				}
			}
		}

		return -1;
	}

	private static int FindNextNonWhitespace(
		string source,
		int startIndex
	) {
		ArgumentNullException.ThrowIfNull( source );

		for ( int i = Math.Max( startIndex, 0 ); i < source.Length; i++ ) {
			if ( !char.IsWhiteSpace( source[i] ) ) {
				return i;
			}
		}

		return -1;
	}

	private static int FindPreviousNonWhitespace(
		string source,
		int startIndex
	) {
		ArgumentNullException.ThrowIfNull( source );

		for ( int i = Math.Min( startIndex, source.Length - 1 ); i >= 0; i-- ) {
			if ( !char.IsWhiteSpace( source[i] ) ) {
				return i;
			}
		}

		return -1;
	}

	private static string MaskNonCode( string source ) {
		ArgumentNullException.ThrowIfNull( source );

		char[] masked = source.ToCharArray();
		int i = 0;
		while ( i < source.Length ) {
			if ( i + 1 < source.Length && source[i] == '/' && source[i + 1] == '/' ) {
				int commentEnd = source.IndexOf( '\n', i + 2 );
				if ( commentEnd < 0 ) {
					commentEnd = source.Length;
				}
				MaskRange( masked, i, commentEnd );
				i = commentEnd;
				continue;
			}

			if ( i + 1 < source.Length && source[i] == '/' && source[i + 1] == '*' ) {
				int blockCommentEnd = source.IndexOf( "*/", i + 2, StringComparison.Ordinal );
				blockCommentEnd = ( blockCommentEnd < 0 )
					? source.Length
					: blockCommentEnd + 2
				;
				MaskRange( masked, i, blockCommentEnd );
				i = blockCommentEnd;
				continue;
			}

			if ( source[i] == '"' ) {
				int quoteCount = CountConsecutiveQuotes( source, i );
				if ( quoteCount >= 3 ) {
					int rawStringEnd = FindRawStringEnd( source, i + quoteCount, quoteCount );
					MaskRange( masked, i, rawStringEnd );
					i = rawStringEnd;
					continue;
				}

				bool verbatim =
					( i > 0 && source[i - 1] == '@' )
					|| ( i > 1 && source[i - 1] == '$' && source[i - 2] == '@' );
				int stringEnd = FindQuotedStringEnd( source, i + 1, verbatim );
				MaskRange( masked, i, stringEnd );
				i = stringEnd;
				continue;
			}

			if ( source[i] == '\'' ) {
				int characterEnd = FindCharacterLiteralEnd( source, i + 1 );
				MaskRange( masked, i, characterEnd );
				i = characterEnd;
				continue;
			}

			i++;
		}

		return new string( masked );
	}

	private static int FindQuotedStringEnd(
		string source,
		int startIndex,
		bool verbatim
	) {
		for ( int i = startIndex; i < source.Length; i++ ) {
			if ( source[i] != '"' ) {
				continue;
			}

			if ( verbatim && i + 1 < source.Length && source[i + 1] == '"' ) {
				i++;
				continue;
			}

			if ( !verbatim && IsEscaped( source, i ) ) {
				continue;
			}

			return i + 1;
		}

		return source.Length;
	}

	private static int FindCharacterLiteralEnd(
		string source,
		int startIndex
	) {
		for ( int i = startIndex; i < source.Length; i++ ) {
			if ( source[i] == '\'' && !IsEscaped( source, i ) ) {
				return i + 1;
			}
		}

		return source.Length;
	}

	private static int FindRawStringEnd(
		string source,
		int startIndex,
		int quoteCount
	) {
		for ( int i = startIndex; i < source.Length; i++ ) {
			if ( source[i] == '"' && CountConsecutiveQuotes( source, i ) >= quoteCount ) {
				return Math.Min( i + quoteCount, source.Length );
			}
		}

		return source.Length;
	}

	private static int CountConsecutiveQuotes(
		string source,
		int startIndex
	) {
		int count = 0;
		while ( startIndex + count < source.Length && source[startIndex + count] == '"' ) {
			count++;
		}
		return count;
	}

	private static bool IsEscaped(
		string source,
		int index
	) {
		int backslashes = 0;
		for ( int i = index - 1; i >= 0 && source[i] == '\\'; i-- ) {
			backslashes++;
		}
		return ( backslashes % 2 ) != 0;
	}

	private static void MaskRange(
		char[] masked,
		int startIndex,
		int endIndex
	) {
		for ( int i = startIndex; i < endIndex; i++ ) {
			if ( masked[i] != '\r' && masked[i] != '\n' ) {
				masked[i] = ' ';
			}
		}
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

	private static string GetRelativePath(
		string repositoryRoot,
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( repositoryRoot );
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		return Path.GetRelativePath( repositoryRoot, path )
			.Replace( Path.DirectorySeparatorChar, '/' );
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
