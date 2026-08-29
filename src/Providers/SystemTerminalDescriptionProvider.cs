using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Loads compiled terminal descriptions through a deterministic snapshot of
/// host environment, user, and platform database locations.
/// </summary>
/// <remarks>
/// Discovery precedence is encoded <c>TERMINFO</c>, directory
/// <c>TERMINFO</c>, non-Windows user <c>.terminfo</c>,
/// <c>TERMINFO_DIRS</c>, then enabled platform defaults. The environment, home,
/// current directory, and host platform are snapshotted at construction.
/// Successful results are cached for this provider instance; clean misses and
/// failures remain retryable. Construct a new provider to refresh successful
/// entries or recapture discovery inputs. This provider never mutates
/// <see cref="TerminalDatabase.BuiltIn"/>.
/// </remarks>
public sealed class SystemTerminalDescriptionProvider
	: ITerminalDescriptionProvider
{
	private const string HexPrefix = "hex:";
	private const string Base64Prefix = "b64:";

	private static readonly string[] LinuxDefaultRoots =
	[
		"/etc/terminfo",
		"/lib/terminfo",
		"/usr/share/terminfo",
	];

	private static readonly string[] MacOSDefaultRoots =
	[
		"/usr/share/terminfo",
	];

	private readonly ConcurrentDictionary<string, Lazy<TerminalDescription?>> _cache =
		new(StringComparer.Ordinal);
	private readonly SystemTerminalDescriptionProviderOptions _options;
	private readonly SystemTerminalDiscoverySnapshot _snapshot;
	private readonly DirectorySource[] _directorySources;

	/// <summary>
	/// Initializes a provider using one immutable snapshot of the permitted
	/// host discovery inputs.
	/// </summary>
	/// <param name="options">
	/// Optional trust/search policy and parser limits. Default options permit
	/// environment, user, and platform-system discovery.
	/// </param>
	/// <remarks>
	/// Construction captures discovery inputs but does not create process-global
	/// terminal state. The provider owns its snapshot and successful-entry cache.
	/// </remarks>
	public SystemTerminalDescriptionProvider(
		SystemTerminalDescriptionProviderOptions? options = null)
	{
		_options =
			SnapshotOptions(
				options);
		_snapshot =
			SystemTerminalDiscoverySnapshot.Capture(
				_options);
		_directorySources =
			BuildDirectorySources(
				_options,
				_snapshot,
				GetDefaultRoots(
					_snapshot.Platform));
	}

	internal SystemTerminalDescriptionProvider(
		SystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(defaultRoots);

		_options =
			SnapshotOptions(
				options);
		_snapshot = snapshot;
		_directorySources =
			BuildDirectorySources(
				_options,
				_snapshot,
				defaultRoots);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Exhausting enabled sources produces a clean miss. Malformed encoded
	/// transport, malformed compiled data, permission/I/O failures, unsafe names,
	/// and reached unsupported file/hashed database sources propagate as errors.
	/// </remarks>
	public bool TryLoad(
		string name,
		[NotNullWhen(true)] out TerminalDescription? terminal)
	{
		DirectoryTerminalDescriptionProvider.ValidateTerminalName(
			name);

		Lazy<TerminalDescription?> load =
			_cache.GetOrAdd(
				name,
				CreateLoad);

		try
		{
			terminal =
				load.Value;
		}
		catch
		{
			_cache.TryRemove(
				new KeyValuePair<string, Lazy<TerminalDescription?>>(
					name,
					load));
			throw;
		}

		if (terminal is null)
		{
			_cache.TryRemove(
				new KeyValuePair<string, Lazy<TerminalDescription?>>(
					name,
					load));
			return false;
		}

		return true;
	}

	private Lazy<TerminalDescription?> CreateLoad(
		string name)
	{
		return new Lazy<TerminalDescription?>(
			() => LoadUncached(
				name),
			LazyThreadSafetyMode.ExecutionAndPublication);
	}

	private TerminalDescription? LoadUncached(
		string name)
	{
		if (_options.UseEnvironment
			&& IsEncodedTermInfo(
				_snapshot.TermInfo))
		{
			if (SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
					_snapshot.TermInfo,
					name,
					_options.ParserOptions,
					out TerminalDescription? encodedTerminal))
			{
				return encodedTerminal;
			}
		}

		foreach (DirectorySource source in _directorySources)
		{
			ValidateDirectorySource(
				source);

			if (source.Provider.TryLoad(
					name,
					out TerminalDescription? terminal))
			{
				return terminal;
			}
		}

		return null;
	}

	internal static IReadOnlyList<string> GetDefaultRoots(
		TerminalHostPlatform platform)
	{
		return platform switch
		{
			TerminalHostPlatform.Linux => LinuxDefaultRoots,
			TerminalHostPlatform.MacOS => MacOSDefaultRoots,
			_ => Array.Empty<string>(),
		};
	}

	internal static IReadOnlyList<SystemTerminalDatabaseLocation> GetDatabaseLocations(
		SystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots
	) {
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(defaultRoots);

		List<SystemTerminalDatabaseLocation> locations = [];

		if (options.UseEnvironment
			&& IsEncodedTermInfo(
				snapshot.TermInfo
			)
		) {
			locations.Add(
				new SystemTerminalDatabaseLocation(
					SystemTerminalDatabaseLocationKind.EncodedTermInfo,
					null
				)
			);
		}

		DirectorySource[] directorySources =
			BuildDirectorySources(
				options,
				snapshot,
				defaultRoots
			);

		foreach (DirectorySource source in directorySources) {
			locations.Add(
				new SystemTerminalDatabaseLocation(
					source.Kind,
					source.Provider.Root
				)
			);
		}

		return locations.ToArray();
	}

	internal static SystemTerminalDescriptionProviderOptions SnapshotOptions(
		SystemTerminalDescriptionProviderOptions? options
	) {
		SystemTerminalDescriptionProviderOptions source =
			options
			?? new SystemTerminalDescriptionProviderOptions();

		return new SystemTerminalDescriptionProviderOptions(
			source.UseEnvironment,
			source.UseUserDatabase,
			source.UseSystemDatabases,
			source.ParserOptions);
	}

	private static DirectorySource[] BuildDirectorySources(
		SystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(defaultRoots);

		StringComparer comparer =
			(snapshot.Platform == TerminalHostPlatform.Windows)
				? StringComparer.OrdinalIgnoreCase
				: StringComparer.Ordinal
		;
		HashSet<string> seen =
			new(comparer);
		List<DirectorySource> sources = [];

		if (options.UseEnvironment
			&& snapshot.TermInfo is { Length: > 0 }
			&& !IsEncodedTermInfo(
				snapshot.TermInfo))
		{
			AddDirectorySource(
				snapshot.TermInfo,
				"TERMINFO",
				SystemTerminalDatabaseLocationKind.TermInfoDirectory,
				snapshot.CurrentDirectory,
				options.ParserOptions,
				seen,
				sources);
		}

		if (options.UseUserDatabase
			&& snapshot.Platform != TerminalHostPlatform.Windows
			&& snapshot.HomeDirectory is { Length: > 0 })
		{
			string homeDirectory =
				Path.GetFullPath(
					snapshot.HomeDirectory,
					snapshot.CurrentDirectory);
			string userRoot =
				Path.Combine(
					homeDirectory,
					".terminfo");

			AddDirectorySource(
				userRoot,
				"user database",
				SystemTerminalDatabaseLocationKind.UserDatabase,
				snapshot.CurrentDirectory,
				options.ParserOptions,
				seen,
				sources);
		}

		IReadOnlyList<string> emptyComponentDefaults =
			options.UseSystemDatabases
				? defaultRoots
				: Array.Empty<string>()
		;

		if (options.UseEnvironment
			&& snapshot.TermInfoDirs is not null)
		{
			IReadOnlyList<string> termInfoDirs =
				SystemTerminalDiscoveryInputs.ResolveTermInfoDirs(
					snapshot,
					emptyComponentDefaults);

			foreach (string root in termInfoDirs)
			{
				AddDirectorySource(
					root,
					"TERMINFO_DIRS",
					SystemTerminalDatabaseLocationKind.TermInfoDirsDirectory,
					snapshot.CurrentDirectory,
					options.ParserOptions,
					seen,
					sources);
			}
		}

		if (options.UseSystemDatabases)
		{
			foreach (string root in defaultRoots)
			{
				AddDirectorySource(
					root,
					"platform default",
					SystemTerminalDatabaseLocationKind.PlatformDefaultDirectory,
					snapshot.CurrentDirectory,
					options.ParserOptions,
					seen,
					sources);
			}
		}

		return sources.ToArray();
	}

	private static void AddDirectorySource(
		string root,
		string sourceName,
		SystemTerminalDatabaseLocationKind kind,
		string currentDirectory,
		CompiledTermInfoParserOptions parserOptions,
		ISet<string> seen,
		ICollection<DirectorySource> sources)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(sourceName);
		ArgumentNullException.ThrowIfNull(currentDirectory);
		ArgumentNullException.ThrowIfNull(parserOptions);
		ArgumentNullException.ThrowIfNull(seen);
		ArgumentNullException.ThrowIfNull(sources);

		if (root.Length == 0)
		{
			throw new ArgumentException(
				"A terminfo search root cannot be empty.",
				nameof(root));
		}

		string fullPath =
			Path.GetFullPath(
				root,
				currentDirectory);

		if (!seen.Add(fullPath))
		{
			return;
		}

		sources.Add(
			new DirectorySource(
				sourceName,
				kind,
				new DirectoryTerminalDescriptionProvider(
					fullPath,
					parserOptions)));
	}

	private static void ValidateDirectorySource(
		DirectorySource source)
	{
		FileAttributes attributes;

		try
		{
			attributes =
				File.GetAttributes(
					source.Provider.Root);
		}
		catch (FileNotFoundException)
		{
			return;
		}
		catch (DirectoryNotFoundException)
		{
			return;
		}

		if ((attributes & FileAttributes.Directory) != 0)
		{
			return;
		}

		throw new NotSupportedException(
			$"The {source.SourceName} terminfo location '{source.Provider.Root}' is not a directory tree. Hashed terminfo databases are outside the 0.9 contract.");
	}

	private static bool IsEncodedTermInfo(
		string? termInfo)
	{
		return termInfo is not null
			&& (termInfo.StartsWith(
					HexPrefix,
					StringComparison.Ordinal)
				|| termInfo.StartsWith(
					Base64Prefix,
					StringComparison.Ordinal));
	}

	private sealed class DirectorySource
	{
		internal DirectorySource(
			string sourceName,
			SystemTerminalDatabaseLocationKind kind,
			DirectoryTerminalDescriptionProvider provider)
		{
			ArgumentNullException.ThrowIfNull(sourceName);
			ArgumentNullException.ThrowIfNull(provider);

			SourceName = sourceName;
			Kind = kind;
			Provider = provider;
		}

		internal string SourceName
		{
			get;
		}

		internal SystemTerminalDatabaseLocationKind Kind
		{
			get;
		}

		internal DirectoryTerminalDescriptionProvider Provider
		{
			get;
		}
	}
}
