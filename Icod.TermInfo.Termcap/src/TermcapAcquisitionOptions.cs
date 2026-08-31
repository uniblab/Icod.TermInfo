using System.Collections.ObjectModel;
using System.Text;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Configures explicit termcap acquisition without changing Runtime terminal
/// discovery.
/// </summary>
public sealed class TermcapAcquisitionOptions
{
	private const string DefaultInlineSourceName = "<inline-termcap>";
	private readonly IReadOnlyList<string> _termPath;

	/// <summary>
	/// Initializes explicit termcap acquisition inputs.
	/// </summary>
	/// <param name="inlineTermcap">
	/// Optional inline termcap source searched before file-backed databases.
	/// </param>
	/// <param name="termcapDatabasePath">
	/// Optional explicit termcap database path searched after inline source.
	/// </param>
	/// <param name="termPath">
	/// Optional ordered database paths searched after the explicit termcap path.
	/// </param>
	/// <param name="defaultPathPolicy">
	/// Optional conventional path policy appended after all explicit paths.
	/// </param>
	/// <param name="homeDirectory">
	/// Optional home directory used only by a selected conventional default path
	/// policy.
	/// </param>
	/// <param name="fileProvider">
	/// Explicit filesystem abstraction. It is required whenever acquisition may
	/// inspect a file-backed database.
	/// </param>
	/// <param name="parserOptions">Optional bounded parser limits.</param>
	/// <param name="resolverOptions">Optional bounded inheritance limits.</param>
	public TermcapAcquisitionOptions(
		string? inlineTermcap = null,
		string? termcapDatabasePath = null,
		IEnumerable<string>? termPath = null,
		TermcapDefaultPathPolicy defaultPathPolicy = TermcapDefaultPathPolicy.None,
		string? homeDirectory = null,
		ITermcapFileProvider? fileProvider = null,
		TermcapSourceParserOptions? parserOptions = null,
		TermcapSourceResolverOptions? resolverOptions = null
	) : this(
		inlineTermcap,
		DefaultInlineSourceName,
		termcapDatabasePath,
		termPath,
		defaultPathPolicy,
		homeDirectory,
		fileProvider,
		parserOptions,
		resolverOptions
	) {
	}

	private TermcapAcquisitionOptions(
		string? inlineTermcap,
		string inlineSourceName,
		string? termcapDatabasePath,
		IEnumerable<string>? termPath,
		TermcapDefaultPathPolicy defaultPathPolicy,
		string? homeDirectory,
		ITermcapFileProvider? fileProvider,
		TermcapSourceParserOptions? parserOptions,
		TermcapSourceResolverOptions? resolverOptions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( inlineSourceName );
		if (
			!Enum.IsDefined(
				typeof( TermcapDefaultPathPolicy ),
				defaultPathPolicy
			)
		) {
			throw new ArgumentOutOfRangeException( nameof( defaultPathPolicy ) );
		}
		if ( inlineTermcap is not null && string.IsNullOrWhiteSpace( inlineTermcap ) ) {
			throw new ArgumentException(
				"Inline termcap source cannot be empty or whitespace.",
				nameof( inlineTermcap )
			);
		}
		if (
			termcapDatabasePath is not null
			&& string.IsNullOrWhiteSpace( termcapDatabasePath )
		) {
			throw new ArgumentException(
				"The termcap database path cannot be empty or whitespace.",
				nameof( termcapDatabasePath )
			);
		}
		if ( homeDirectory is not null && string.IsNullOrWhiteSpace( homeDirectory ) ) {
			throw new ArgumentException(
				"The home directory cannot be empty or whitespace.",
				nameof( homeDirectory )
			);
		}

		string[] termPathArray =
			termPath?.ToArray()
			?? Array.Empty<string>();
		for ( int index = 0; index < termPathArray.Length; index++ ) {
			if ( string.IsNullOrWhiteSpace( termPathArray[index] ) ) {
				throw new ArgumentException(
					"TERMPATH entries cannot be null, empty, or whitespace.",
					nameof( termPath )
				);
			}
		}

		bool usesFiles =
			termcapDatabasePath is not null
			|| termPathArray.Length != 0
			|| defaultPathPolicy != TermcapDefaultPathPolicy.None;
		if ( usesFiles && fileProvider is null ) {
			throw new ArgumentNullException(
				nameof( fileProvider ),
				"A file provider is required when termcap acquisition may inspect database paths."
			);
		}

		TermcapSourceParserOptions effectiveParserOptions =
			parserOptions ?? new TermcapSourceParserOptions();
		TermcapSourceResolverOptions effectiveResolverOptions =
			resolverOptions ?? new TermcapSourceResolverOptions();

		InlineTermcap = inlineTermcap;
		InlineSourceName = inlineSourceName;
		TermcapDatabasePath = termcapDatabasePath;
		_termPath =
			new ReadOnlyCollection<string>(
				termPathArray
			);
		DefaultPathPolicy = defaultPathPolicy;
		HomeDirectory = homeDirectory;
		FileProvider = fileProvider;
		ParserOptions =
			new TermcapSourceParserOptions(
				effectiveParserOptions.MaximumSourceLength
			);
		ResolverOptions =
			new TermcapSourceResolverOptions(
				effectiveResolverOptions.MaximumInheritanceDepth
			);
	}

	/// <summary>Gets optional inline termcap source.</summary>
	public string? InlineTermcap { get; }

	/// <summary>Gets the optional explicit termcap database path.</summary>
	public string? TermcapDatabasePath { get; }

	/// <summary>Gets ordered TERMPATH database paths.</summary>
	public IReadOnlyList<string> TermPath => _termPath;

	/// <summary>Gets the explicitly selected conventional default policy.</summary>
	public TermcapDefaultPathPolicy DefaultPathPolicy { get; }

	/// <summary>
	/// Gets the optional home directory used by conventional default discovery.
	/// </summary>
	public string? HomeDirectory { get; }

	/// <summary>
	/// Gets the explicit filesystem provider, or <see langword="null"/> when this
	/// options instance can use only inline source.
	/// </summary>
	public ITermcapFileProvider? FileProvider { get; }

	/// <summary>Gets the immutable parser limits snapshot.</summary>
	public TermcapSourceParserOptions ParserOptions { get; }

	/// <summary>Gets the immutable resolver limits snapshot.</summary>
	public TermcapSourceResolverOptions ResolverOptions { get; }

	internal string InlineSourceName { get; }

	/// <summary>
	/// Snapshots historical <c>TERMCAP</c>, <c>TERMPATH</c>, and <c>HOME</c>
	/// values through a caller-supplied environment provider.
	/// </summary>
	/// <remarks>
	/// A non-empty <c>TERMCAP</c> beginning with a rooted Unix or Windows path
	/// marker is interpreted as a database path; otherwise it is treated as an
	/// inline termcap description. <c>TERMPATH</c> uses colon/whitespace
	/// separators, or semicolon separators when semicolons are present. No
	/// conventional default paths are added unless <paramref name="defaultPathPolicy"/>
	/// explicitly requests them.
	/// </remarks>
	public static TermcapAcquisitionOptions FromEnvironment(
		ITermcapEnvironmentProvider environmentProvider,
		ITermcapFileProvider fileProvider,
		TermcapDefaultPathPolicy defaultPathPolicy = TermcapDefaultPathPolicy.None,
		TermcapSourceParserOptions? parserOptions = null,
		TermcapSourceResolverOptions? resolverOptions = null
	) {
		ArgumentNullException.ThrowIfNull( environmentProvider );
		ArgumentNullException.ThrowIfNull( fileProvider );

		string? termcap =
			NormalizeEnvironmentValue(
				environmentProvider.GetEnvironmentVariable( "TERMCAP" )
			);
		string? termPath =
			NormalizeEnvironmentValue(
				environmentProvider.GetEnvironmentVariable( "TERMPATH" )
			);
		string? homeDirectory =
			NormalizeEnvironmentValue(
				environmentProvider.GetEnvironmentVariable( "HOME" )
			);

		string? inlineTermcap = null;
		string? termcapDatabasePath = null;
		if ( termcap is not null ) {
			if ( LooksLikeDatabasePath( termcap ) ) {
				termcapDatabasePath = termcap;
			} else {
				inlineTermcap = termcap;
			}
		}

		return new TermcapAcquisitionOptions(
			inlineTermcap,
			"TERMCAP",
			termcapDatabasePath,
			SplitTermPath( termPath ),
			defaultPathPolicy,
			homeDirectory,
			fileProvider,
			parserOptions,
			resolverOptions
		);
	}

	private static string? NormalizeEnvironmentValue(
		string? value
	) {
		return string.IsNullOrWhiteSpace( value )
			? null
			: value
		;
	}

	private static bool LooksLikeDatabasePath(
		string value
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( value );

		if ( value[0] == '/' || value[0] == '\\' ) {
			return true;
		}
		return value.Length >= 3
			&& char.IsLetter( value[0] )
			&& value[1] == ':'
			&& ( value[2] == '\\' || value[2] == '/' )
		;
	}

	private static IReadOnlyList<string> SplitTermPath(
		string? value
	) {
		if ( value is null ) {
			return Array.Empty<string>();
		}

		if (
			value.Length >= 3
			&& char.IsLetter( value[0] )
			&& value[1] == ':'
			&& ( value[2] == '\\' || value[2] == '/' )
			&& !value.Contains( ';' )
		) {
			return new[] { value };
		}

		if ( value.Contains( ';' ) ) {
			return value
				.Split(
					';',
					StringSplitOptions.RemoveEmptyEntries
						| StringSplitOptions.TrimEntries
				)
				.Where( component => component.Length != 0 )
				.ToArray();
		}

		List<string> paths = [];
		StringBuilder current = new();
		foreach ( char valueCharacter in value ) {
			if ( valueCharacter == ':' || char.IsWhiteSpace( valueCharacter ) ) {
				AddPathComponent(
					current,
					paths
				);
				continue;
			}
			current.Append( valueCharacter );
		}
		AddPathComponent(
			current,
			paths
		);
		return paths;
	}

	private static void AddPathComponent(
		StringBuilder current,
		ICollection<string> paths
	) {
		ArgumentNullException.ThrowIfNull( current );
		ArgumentNullException.ThrowIfNull( paths );

		if ( current.Length == 0 ) {
			return;
		}
		paths.Add( current.ToString() );
		current.Clear();
	}
}
