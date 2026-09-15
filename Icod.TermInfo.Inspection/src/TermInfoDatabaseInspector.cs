/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Provides read-only inspection of the system terminfo database locations
/// considered by the Runtime discovery model.
/// </summary>
public static partial class TermInfoDatabaseInspector {
	/// <summary>
	/// Captures and returns the ordered terminfo locations which a newly-created
	/// <see cref="SystemTerminalDescriptionProvider"/> would consider under the
	/// same options.
	/// </summary>
	/// <param name="options">
	/// Optional system discovery policy. A <see langword="null"/> value uses the
	/// Runtime defaults.
	/// </param>
	/// <returns>
	/// An immutable snapshot in Runtime discovery precedence order.
	/// </returns>
	/// <remarks>
	/// This operation reads discovery inputs but does not load terminal entries,
	/// enumerate database contents, mutate the filesystem, or expose encoded
	/// <c>TERMINFO</c> bytes.
	/// </remarks>
	public static IReadOnlyList<TermInfoDatabaseLocation> GetSystemLocations(
		SystemTerminalDescriptionProviderOptions? options = null
	) {
		SystemTerminalDescriptionProviderOptions effectiveOptions =
			SystemTerminalDescriptionProvider.SnapshotOptions( options );
		SystemTerminalDiscoverySnapshot snapshot =
			SystemTerminalDiscoverySnapshot.Capture( effectiveOptions );

		return GetSystemLocations(
			effectiveOptions,
			snapshot,
			SystemTerminalDescriptionProvider.GetDefaultRoots( snapshot.Platform )
		);
	}

	internal static IReadOnlyList<TermInfoDatabaseLocation> GetSystemLocations(
		SystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( snapshot );
		ArgumentNullException.ThrowIfNull( defaultRoots );

		IReadOnlyList<SystemTerminalDatabaseLocation> runtimeLocations =
			SystemTerminalDescriptionProvider.GetDatabaseLocations(
				options,
				snapshot,
				defaultRoots
			);
		TermInfoDatabaseLocation[] result =
			new TermInfoDatabaseLocation[runtimeLocations.Count];

		for ( int index = 0; index < runtimeLocations.Count; index++ ) {
			SystemTerminalDatabaseLocation location = runtimeLocations[index];
			result[index] =
				new TermInfoDatabaseLocation(
					MapKind( location.Kind ),
					location.Path
				);
		}

		return Array.AsReadOnly( result );
	}

	private static TermInfoDatabaseLocationKind MapKind(
		SystemTerminalDatabaseLocationKind kind
	) {
		return kind switch {
			SystemTerminalDatabaseLocationKind.EncodedTermInfo
				=> TermInfoDatabaseLocationKind.EncodedTermInfo,
			SystemTerminalDatabaseLocationKind.TermInfoDirectory
				=> TermInfoDatabaseLocationKind.TermInfoDirectory,
			SystemTerminalDatabaseLocationKind.UserDatabase
				=> TermInfoDatabaseLocationKind.UserDatabase,
			SystemTerminalDatabaseLocationKind.TermInfoDirsDirectory
				=> TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
			SystemTerminalDatabaseLocationKind.PlatformDefaultDirectory
				=> TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"Unknown Runtime terminfo database location kind."
			),
		};
	}
}
