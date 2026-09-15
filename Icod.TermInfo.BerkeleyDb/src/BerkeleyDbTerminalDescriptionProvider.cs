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
using System.Text;
using System.Threading;

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Loads terminal descriptions from one explicit ncurses-compatible Berkeley DB Hash-v9 file.</summary>
/// <remarks>
/// Lookup uses exact ordinal UTF-8 keys. Successful descriptions are cached for
/// this provider instance; clean misses and failures remain retryable. Construct
/// a new provider to observe changed content after a successful lookup.
/// </remarks>
public sealed class BerkeleyDbTerminalDescriptionProvider
	: ITerminalDescriptionProvider {
	private const int MaximumStoredItemSize =
		CompiledTermInfoParserOptions.MaximumSupportedEntrySize + 1;

	private static readonly UTF8Encoding StrictUtf8 =
		new(
			encoderShouldEmitUTF8Identifier: false,
			throwOnInvalidBytes: true
		);

	private readonly ConcurrentDictionary<string, Lazy<TerminalDescription?>> _cache =
		new( StringComparer.Ordinal );
	private readonly int _maximumDatabaseSize;
	private readonly int _maximumIndexHops;
	private readonly CompiledTermInfoParserOptions _parserOptions;

	/// <summary>Initializes an explicit hashed-database provider.</summary>
	public BerkeleyDbTerminalDescriptionProvider(
		string databasePath,
		BerkeleyDbTerminalDescriptionProviderOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( databasePath );

		if ( string.IsNullOrWhiteSpace( databasePath ) ) {
			throw new ArgumentException(
				"The Berkeley DB path cannot be empty or whitespace.",
				nameof( databasePath )
			);
		}

		DatabasePath = Path.GetFullPath( databasePath );

		BerkeleyDbTerminalDescriptionProviderOptions effectiveOptions =
			options ?? new BerkeleyDbTerminalDescriptionProviderOptions();
		_maximumDatabaseSize = effectiveOptions.MaximumDatabaseSize;
		_maximumIndexHops = effectiveOptions.MaximumIndexHops;
		_parserOptions =
			new CompiledTermInfoParserOptions(
				effectiveOptions.ParserOptions.MaximumEntrySize
			);
	}

	/// <summary>Gets the canonical absolute database path.</summary>
	public string DatabasePath { get; }

	/// <inheritdoc/>
	/// <remarks>
	/// An absent initial key is a clean miss. Container and ncurses-envelope
	/// failures throw <see cref="BerkeleyDbDatabaseFormatException"/>. Compiled
	/// parser, identity, and I/O failures retain their original exception types.
	/// </remarks>
	public bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	) {
		ValidateTerminalName( name );

		Lazy<TerminalDescription?> load =
			_cache.GetOrAdd(
				name,
				CreateLoad
			);

		try {
			terminal =
				load.Value;
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
		byte[] compiledEntry;

		try {
			if (
				!NcursesRecordReader.TryReadCompiledEntry(
					DatabasePath,
					StrictUtf8.GetBytes( name ),
					out compiledEntry,
					_maximumDatabaseSize,
					MaximumStoredItemSize,
					_maximumIndexHops
				)
			) {
				return null;
			}
		} catch ( InvalidDataException exception ) {
			throw new BerkeleyDbDatabaseFormatException(
				"The Berkeley DB file or ncurses record envelope is malformed or unsupported.",
				exception
			);
		}

		TerminalDescription terminal =
			CompiledTermInfoParser.Parse(
				compiledEntry,
				_parserOptions
			);

		VerifyIdentity(
			name,
			terminal
		);
		return terminal;
	}

	private static void VerifyIdentity(
		string requestedName,
		TerminalDescription terminal
	) {
		if (
			string.Equals(
				requestedName,
				terminal.Name,
				StringComparison.Ordinal
			)
		) {
			return;
		}

		foreach ( string alias in terminal.Aliases ) {
			if (
				string.Equals(
					requestedName,
					alias,
					StringComparison.Ordinal
				)
			) {
				return;
			}
		}

		throw new InvalidDataException(
			$"Compiled terminfo entry in the Berkeley DB identifies terminal '{terminal.Name}' and does not declare requested name '{requestedName}'."
		);
	}

	private static void ValidateTerminalName( string name ) {
		ArgumentNullException.ThrowIfNull( name );

		if ( string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"The terminal name cannot be empty or whitespace.",
				nameof( name )
			);
		}

		if (
			string.Equals(
				name,
				".",
				StringComparison.Ordinal
			)
			|| string.Equals(
				name,
				"..",
				StringComparison.Ordinal
			)
		) {
			throw new ArgumentException(
				"The terminal name must be an exact database key.",
				nameof( name )
			);
		}

		foreach ( char character in name ) {
			if (
				character == '\0'
				|| character == '/'
				|| character == '\\'
				|| char.IsControl( character )
				|| char.IsSurrogate( character )
			) {
				throw new ArgumentException(
					"The terminal name contains unsafe key syntax.",
					nameof( name )
				);
			}
		}
	}
}
