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
/// Defines explicit caller-owned raster-backend preference policy.
/// </summary>
public sealed class RasterBackendSelectionOptions {
	/// <summary>Gets the largest supported preference-order length.</summary>
	public const int MaximumSupportedPreferenceCount = 2;

	/// <summary>Initializes options with no caller ranking.</summary>
	public RasterBackendSelectionOptions()
		: this( Array.Empty<RasterBackendKind>() ) {
	}

	/// <summary>Initializes options with an explicit ordered backend preference.</summary>
	public RasterBackendSelectionOptions(
		IEnumerable<RasterBackendKind> preferenceOrder
	) {
		ArgumentNullException.ThrowIfNull( preferenceOrder );

		List<RasterBackendKind> snapshot = [];
		HashSet<RasterBackendKind> seen = [];
		foreach ( RasterBackendKind backend in preferenceOrder ) {
			if ( !Enum.IsDefined( backend ) ) {
				throw new ArgumentOutOfRangeException(
					nameof( preferenceOrder ),
					backend,
					"Raster-backend preference values must be defined."
				);
			}
			snapshot.Add( backend );
			if ( snapshot.Count > MaximumSupportedPreferenceCount ) {
				throw new ArgumentException(
					$"Raster-backend preference order cannot exceed {MaximumSupportedPreferenceCount} entries.",
					nameof( preferenceOrder )
				);
			}
			if ( !seen.Add( backend ) ) {
				throw new ArgumentException(
					"Raster-backend preference order cannot contain duplicate backends.",
					nameof( preferenceOrder )
				);
			}
		}

		PreferenceOrder = Array.AsReadOnly( snapshot.ToArray() );
	}

	/// <summary>
	/// Gets the explicit caller preference order. An empty collection means no
	/// caller ranking has been supplied.
	/// </summary>
	public IReadOnlyList<RasterBackendKind> PreferenceOrder {
		get;
	}
}
