/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Loads terminal descriptions through hashed-aware system discovery.</summary>
/// <remarks>
/// Runtime owns discovery input capture, precedence, path expansion, and logical
/// location deduplication. This provider adds storage-shape selection for
/// conventional directories, exact hashed files, and ncurses .db companions.
/// Successful results are cached; clean misses and failures remain retryable.
/// </remarks>
public sealed class BerkeleyDbSystemTerminalDescriptionProvider
	: ITerminalDescriptionProvider {
	private const string DatabaseSuffix = ".db";

	private readonly ConcurrentDictionary<string, Lazy<TerminalDescription?>> _cache =
		new( StringComparer.Ordinal );
	private readonly bool _hasEncodedTermInfo;
	private readonly BerkeleyDbSystemTerminalDescriptionProviderOptions _options;
	private readonly SystemTerminalDiscoverySnapshot _snapshot;
	private readonly LocationSource[] _sources;

	/// <summary>Initializes a provider from a snapshot of permitted host discovery inputs.</summary>
	public BerkeleyDbSystemTerminalDescriptionProvider(
		BerkeleyDbSystemTerminalDescriptionProviderOptions? options = null
	) : this(
		CreateCapturedInitialization( options )
	) {
	}

	internal BerkeleyDbSystemTerminalDescriptionProvider(
		BerkeleyDbSystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots
	) : this(
		CreateInitialization(
			options,
			snapshot,
			defaultRoots
		)
	) {
	}

	private BerkeleyDbSystemTerminalDescriptionProvider(
		Initialization initialization
	) {
		_options = initialization.Options;
		_snapshot = initialization.Snapshot;
		_hasEncodedTermInfo = initialization.HasEncodedTermInfo;
		_sources = initialization.Sources;
	}

	/// <inheritdoc/>
	public bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	) {
		DirectoryTerminalDescriptionProvider.ValidateTerminalName( name );

		Lazy<TerminalDescription?> load =
			_cache.GetOrAdd(
				name,
				CreateLoad
			);
		try {
			terminal = load.Value;
		} catch {
			_cache.TryRemove(
				new KeyValuePair<string, Lazy<TerminalDescription?>>(
					name,
					load
				)
			);
			throw;
		}

		if ( terminal is null ) {
			_cache.TryRemove(
				new KeyValuePair<string, Lazy<TerminalDescription?>>(
					name,
					load
				)
			);
			return false;
		}
		return true;
	}

	private Lazy<TerminalDescription?> CreateLoad( string name ) {
		return new Lazy<TerminalDescription?>(
			() => LoadUncached( name ),
			LazyThreadSafetyMode.ExecutionAndPublication
		);
	}

	private TerminalDescription? LoadUncached( string name ) {
		if (
			_hasEncodedTermInfo
			&& SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
				_snapshot.TermInfo,
				name,
				_options.ParserOptions,
				out TerminalDescription? encoded
			)
		) {
			return encoded;
		}

		foreach ( LocationSource source in _sources ) {
			if (
				source.TryLoad(
					name,
					out TerminalDescription? terminal
				)
			) {
				return terminal;
			}
		}
		return null;
	}

	private static Initialization CreateCapturedInitialization(
		BerkeleyDbSystemTerminalDescriptionProviderOptions? options
	) {
		BerkeleyDbSystemTerminalDescriptionProviderOptions effectiveOptions =
			SnapshotOptions( options );
		SystemTerminalDescriptionProviderOptions runtimeOptions =
			CreateRuntimeOptions( effectiveOptions );
		SystemTerminalDiscoverySnapshot snapshot =
			SystemTerminalDiscoverySnapshot.Capture( runtimeOptions );

		return CreateInitialization(
			effectiveOptions,
			snapshot,
			SystemTerminalDescriptionProvider.GetDefaultRoots(
				snapshot.Platform
			)
		);
	}

	private static Initialization CreateInitialization(
		BerkeleyDbSystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( snapshot );
		ArgumentNullException.ThrowIfNull( defaultRoots );

		BerkeleyDbSystemTerminalDescriptionProviderOptions effectiveOptions =
			SnapshotOptions( options );
		IReadOnlyList<SystemTerminalDatabaseLocation> locations =
			SystemTerminalDescriptionProvider.GetDatabaseLocations(
				CreateRuntimeOptions( effectiveOptions ),
				snapshot,
				defaultRoots
			);
		bool hasEncodedTermInfo = false;
		List<LocationSource> sources = [];

		foreach ( SystemTerminalDatabaseLocation location in locations ) {
			if (
				location.Kind
				== SystemTerminalDatabaseLocationKind.EncodedTermInfo
			) {
				hasEncodedTermInfo = true;
				continue;
			}

			if ( location.Path is null ) {
				throw new InvalidOperationException(
					"A filesystem discovery location has no path."
				);
			}
			sources.Add(
				new LocationSource(
					location.Path,
					effectiveOptions
				)
			);
		}

		return new Initialization(
			effectiveOptions,
			snapshot,
			hasEncodedTermInfo,
			sources.ToArray()
		);
	}

	private static BerkeleyDbSystemTerminalDescriptionProviderOptions SnapshotOptions(
		BerkeleyDbSystemTerminalDescriptionProviderOptions? options
	) {
		BerkeleyDbSystemTerminalDescriptionProviderOptions source =
			options
			?? new BerkeleyDbSystemTerminalDescriptionProviderOptions();

		return new BerkeleyDbSystemTerminalDescriptionProviderOptions(
			source.UseEnvironment,
			source.UseUserDatabase,
			source.UseSystemDatabases,
			source.ParserOptions,
			source.MaximumDatabaseSize,
			source.MaximumIndexHops
		);
	}

	private static SystemTerminalDescriptionProviderOptions CreateRuntimeOptions(
		BerkeleyDbSystemTerminalDescriptionProviderOptions options
	) {
		return new SystemTerminalDescriptionProviderOptions(
			options.UseEnvironment,
			options.UseUserDatabase,
			options.UseSystemDatabases,
			options.ParserOptions
		);
	}

	private static bool TryGetAttributes(
		string path,
		out FileAttributes attributes
	) {
		try {
			attributes = File.GetAttributes( path );
			return true;
		} catch ( FileNotFoundException ) {
			attributes = default;
			return false;
		} catch ( DirectoryNotFoundException ) {
			attributes = default;
			return false;
		}
	}

	private sealed class LocationSource {
		private readonly string _path;
		private readonly string _companionPath;
		private readonly DirectoryTerminalDescriptionProvider _directoryProvider;
		private readonly BerkeleyDbTerminalDescriptionProvider _hashedProvider;
		private readonly BerkeleyDbTerminalDescriptionProvider _companionProvider;

		internal LocationSource(
			string path,
			BerkeleyDbSystemTerminalDescriptionProviderOptions options
		) {
			ArgumentNullException.ThrowIfNull( path );
			ArgumentNullException.ThrowIfNull( options );

			_path = path;
			_companionPath = path + DatabaseSuffix;
			_directoryProvider =
				new DirectoryTerminalDescriptionProvider(
					path,
					options.ParserOptions
				);
			BerkeleyDbTerminalDescriptionProviderOptions hashedOptions =
				new(
					options.ParserOptions,
					options.MaximumDatabaseSize,
					options.MaximumIndexHops
				);
			_hashedProvider =
				new BerkeleyDbTerminalDescriptionProvider(
					path,
					hashedOptions
				);
			_companionProvider =
				new BerkeleyDbTerminalDescriptionProvider(
					_companionPath,
					hashedOptions
				);
		}

		internal bool TryLoad(
			string name,
			[NotNullWhen( true )] out TerminalDescription? terminal
		) {
			if (
				TryGetAttributes(
					_path,
					out FileAttributes exactAttributes
				)
			) {
				if (
					( exactAttributes & FileAttributes.Directory )
					!= 0
				) {
					return _directoryProvider.TryLoad(
						name,
						out terminal
					);
				}
				return _hashedProvider.TryLoad(
					name,
					out terminal
				);
			}

			if (
				!TryGetAttributes(
					_companionPath,
					out FileAttributes companionAttributes
				)
			) {
				terminal = null;
				return false;
			}
			if (
				( companionAttributes & FileAttributes.Directory )
				!= 0
			) {
				throw new NotSupportedException(
					$"The ncurses hashed-database companion '{_companionPath}' is not a regular file."
				);
			}
			return _companionProvider.TryLoad(
				name,
				out terminal
			);
		}
	}

	private sealed record Initialization(
		BerkeleyDbSystemTerminalDescriptionProviderOptions Options,
		SystemTerminalDiscoverySnapshot Snapshot,
		bool HasEncodedTermInfo,
		LocationSource[] Sources
	);
}
