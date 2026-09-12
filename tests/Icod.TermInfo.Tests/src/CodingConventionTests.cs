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
	private static readonly Regex MultilineTernaryQuestionPattern = new(
		@"(?m)^(?<indent>[\t ]*)\?[\t ]+",
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
			string source = File.ReadAllText( path );
			string maskedSource = MaskNonCode( source );
			int violationIndex = FindParenthesisSpacingViolation(
				maskedSource,
				source
			);

			if ( violationIndex >= 0 ) {
				violations.Add(
					$"{GetRelativePath( repositoryRoot, path )}:{GetLineNumber( source, violationIndex )}"
				);
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files using non-repository call/control parenthesis spacing:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	[Fact]
	public void RepositoryCSharpMultilineCallsAndDeclarationsCloseParenthesisOnOwnLine() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = File.ReadAllText( path );
			string maskedSource = MaskNonCode( source );
			int violationIndex = FindMultilineClosingParenthesisViolation(
				maskedSource,
				source
			);

			if ( violationIndex >= 0 ) {
				violations.Add(
					$"{GetRelativePath( repositoryRoot, path )}:{GetLineNumber( source, violationIndex )}"
				);
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files with multiline calls/declarations whose closing ')' shares the final argument line:"
				+ Environment.NewLine
				+ string.Join( Environment.NewLine, violations )
		);
	}

	[Fact]
	public void RepositoryCSharpMultilineTernariesUseRepositoryLayout() {
		string repositoryRoot = FindRepositoryRoot();
		List<string> violations = [];

		foreach ( string path in EnumerateCSharpFiles( repositoryRoot ) ) {
			string source = File.ReadAllText( path );
			string maskedSource = MaskNonCode( source );
			int violationIndex = FindMultilineTernaryViolation(
				maskedSource,
				source
			);

			if ( violationIndex >= 0 ) {
				violations.Add(
					$"{GetRelativePath( repositoryRoot, path )}:{GetLineNumber( source, violationIndex )}"
				);
			}
		}

		Assert.True(
			violations.Count == 0,
			"C# files with multiline ternaries that do not use a parenthesized condition, aligned branches, and an own-line semicolon:"
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

	private static int FindParenthesisSpacingViolation(
		string maskedSource,
		string source
	) {
		ArgumentNullException.ThrowIfNull( maskedSource );
		ArgumentNullException.ThrowIfNull( source );

		Match controlKeywordMatch =
			ControlKeywordWithoutSpaceBeforeParenthesisPattern.Match( maskedSource );
		if ( controlKeywordMatch.Success ) {
			return controlKeywordMatch.Index;
		}

		foreach ( Match match in ParenthesizedControlFlowPattern.Matches( maskedSource ) ) {
			int openParenthesis =
				match.Index + match.Value.LastIndexOf( '(' );
			if (
				!ParenthesisInteriorUsesRepositorySpacing(
					maskedSource,
					source,
					openParenthesis
				)
			) {
				return openParenthesis;
			}
		}

		for ( int i = 0; i < maskedSource.Length; i++ ) {
			if (
				maskedSource[i] != '('
				|| !IsCallLikeOpenParenthesis( maskedSource, i )
			) {
				continue;
			}

			if (
				!ParenthesisInteriorUsesRepositorySpacing(
					maskedSource,
					source,
					i
				)
			) {
				return i;
			}
		}

		return -1;
	}

	private static int FindMultilineClosingParenthesisViolation(
		string maskedSource,
		string source
	) {
		ArgumentNullException.ThrowIfNull( maskedSource );
		ArgumentNullException.ThrowIfNull( source );

		for ( int i = 0; i < maskedSource.Length; i++ ) {
			if (
				maskedSource[i] != '('
				|| !IsCallLikeOpenParenthesis( maskedSource, i )
			) {
				continue;
			}

			int closeParenthesis = FindMatchingParenthesis( maskedSource, i );
			if ( closeParenthesis < 0 ) {
				continue;
			}

			int firstLineBreak = maskedSource.IndexOf( '\n', i + 1 );
			if ( firstLineBreak < 0 || firstLineBreak > closeParenthesis ) {
				continue;
			}

			int lineStart = maskedSource.LastIndexOf( '\n', closeParenthesis - 1 ) + 1;
			for ( int j = lineStart; j < closeParenthesis; j++ ) {
				if ( !char.IsWhiteSpace( source[j] ) ) {
					return closeParenthesis;
				}
			}
		}

		return -1;
	}

	private static int FindMultilineTernaryViolation(
		string maskedSource,
		string source
	) {
		ArgumentNullException.ThrowIfNull( maskedSource );
		ArgumentNullException.ThrowIfNull( source );

		foreach ( Match match in MultilineTernaryQuestionPattern.Matches( maskedSource ) ) {
			int questionIndex =
				match.Index + match.Value.IndexOf( '?' );
			int previous = FindPreviousNonWhitespace( maskedSource, questionIndex - 1 );
			if ( previous < 0 || maskedSource[previous] != ')' ) {
				return questionIndex;
			}

			int conditionOpen = FindMatchingOpenParenthesis( maskedSource, previous );
			if (
				conditionOpen < 0
				|| IsCallLikeOpenParenthesis( maskedSource, conditionOpen )
			) {
				return questionIndex;
			}

			string indent = match.Groups[ "indent" ].Value;
			int colonIndex = FindTernaryBranchMarker(
				maskedSource,
				questionIndex + 1,
				indent,
				':'
			);
			if ( colonIndex < 0 ) {
				return questionIndex;
			}

			int semicolonIndex = maskedSource.IndexOf( ';', colonIndex + 1 );
			if ( semicolonIndex < 0 ) {
				return colonIndex;
			}

			int semicolonLineStart =
				maskedSource.LastIndexOf( '\n', semicolonIndex - 1 ) + 1;
			string semicolonIndent =
				maskedSource[ semicolonLineStart..semicolonIndex ];
			if (
				semicolonIndent != indent
				|| semicolonIndent.Any( character => !char.IsWhiteSpace( character ) )
			) {
				return semicolonIndex;
			}
		}

		return -1;
	}

	private static int FindTernaryBranchMarker(
		string source,
		int startIndex,
		string indent,
		char marker
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( indent );

		int lineStart = source.IndexOf( '\n', startIndex );
		if ( lineStart < 0 ) {
			return -1;
		}
		lineStart++;

		while ( lineStart < source.Length ) {
			int lineEnd = source.IndexOf( '\n', lineStart );
			if ( lineEnd < 0 ) {
				lineEnd = source.Length;
			}

			int first = lineStart;
			while ( first < lineEnd && char.IsWhiteSpace( source[first] ) ) {
				first++;
			}

			if ( first < lineEnd && source[first] == ';' ) {
				return -1;
			}

			if ( first < lineEnd && source[first] == marker ) {
				string markerIndent = source[ lineStart..first ];
				return ( markerIndent == indent )
					? first
					: -1
				;
			}

			lineStart = lineEnd + 1;
		}

		return -1;
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
		if (
			char.IsLetterOrDigit( previous )
			|| previous == '_'
			|| previous == '>'
			|| previous == ']'
		) {
			return true;
		}

		if ( previous == '!' ) {
			return openParenthesis > 1
				&& IsInvocationReceiverCharacter( source[openParenthesis - 2] );
		}

		if ( previous != ')' ) {
			return false;
		}

		int precedingOpenParenthesis =
			FindMatchingOpenParenthesis( source, openParenthesis - 1 );
		return precedingOpenParenthesis >= 0
			&& IsCallLikeOpenParenthesis( source, precedingOpenParenthesis );
	}

	private static bool IsInvocationReceiverCharacter( char value ) {
		return char.IsLetterOrDigit( value )
			|| value == '_'
			|| value == '>'
			|| value == ']'
			|| value == ')';
	}

	private static bool ParenthesisInteriorUsesRepositorySpacing(
		string maskedSource,
		string source,
		int openParenthesis
	) {
		ArgumentNullException.ThrowIfNull( maskedSource );
		ArgumentNullException.ThrowIfNull( source );

		int closeParenthesis =
			FindMatchingParenthesis( maskedSource, openParenthesis );
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

	private static int FindMatchingOpenParenthesis(
		string source,
		int closeParenthesis
	) {
		ArgumentNullException.ThrowIfNull( source );

		int depth = 0;
		for ( int i = closeParenthesis; i >= 0; i-- ) {
			if ( source[i] == ')' ) {
				depth++;
			} else if ( source[i] == '(' ) {
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

	private static int GetLineNumber(
		string source,
		int index
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentOutOfRangeException.ThrowIfNegative( index );

		int lineNumber = 1;
		int end = Math.Min( index, source.Length );
		for ( int i = 0; i < end; i++ ) {
			if ( source[i] == '\n' ) {
				lineNumber++;
			}
		}
		return lineNumber;
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
