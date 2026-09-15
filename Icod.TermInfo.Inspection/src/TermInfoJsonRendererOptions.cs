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
/// Configures deterministic, bounded machine-readable JSON rendering of
/// Inspection values.
/// </summary>
public sealed class TermInfoJsonRendererOptions {
	/// <summary>
	/// The default maximum rendered JSON size in UTF-8 bytes.
	/// </summary>
	public const int DefaultMaximumOutputByteCount = 4_194_304;

	/// <summary>
	/// The largest supported caller-selected rendered JSON size in UTF-8 bytes.
	/// </summary>
	public const int MaximumSupportedOutputByteCount = 67_108_864;

	/// <summary>
	/// Initializes the canonical compact and bounded JSON policy.
	/// </summary>
	public TermInfoJsonRendererOptions()
		: this(
			DefaultMaximumOutputByteCount,
			writeIndented: false
		) {
	}

	/// <summary>
	/// Initializes an explicit deterministic and bounded JSON policy.
	/// </summary>
	/// <param name="maximumOutputByteCount">
	/// The maximum accepted rendered JSON size in UTF-8 bytes.
	/// </param>
	/// <param name="writeIndented">
	/// Whether the renderer uses its frozen indented presentation instead of the
	/// canonical compact presentation.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="maximumOutputByteCount"/> is not between one and
	/// <see cref="MaximumSupportedOutputByteCount"/>.
	/// </exception>
	public TermInfoJsonRendererOptions(
		int maximumOutputByteCount,
		bool writeIndented = false
	) {
		if ( maximumOutputByteCount < 1
			|| maximumOutputByteCount > MaximumSupportedOutputByteCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumOutputByteCount ),
				maximumOutputByteCount,
				$"The maximum JSON output size must be between 1 and {MaximumSupportedOutputByteCount} UTF-8 bytes."
			);
		}

		MaximumOutputByteCount = maximumOutputByteCount;
		WriteIndented = writeIndented;
	}

	/// <summary>
	/// Gets the maximum accepted rendered JSON size in UTF-8 bytes.
	/// </summary>
	public int MaximumOutputByteCount {
		get;
	}

	/// <summary>
	/// Gets whether the frozen indented JSON presentation is requested.
	/// </summary>
	public bool WriteIndented {
		get;
	}
}
