/*
	Icod.TermInfo.Termcap
	Provides managed termcap parsing, conversion, rendering, and interoperability.
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

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Controls deterministic physical-line wrapping for rendered termcap source.
/// </summary>
public sealed class TermcapRenderOptions {
	/// <summary>Gets the default maximum preferred physical-line length.</summary>
	public const int DefaultMaximumLineLength = 80;

	/// <summary>Gets the largest accepted preferred physical-line length.</summary>
	public const int MaximumSupportedLineLength = 4096;

	/// <summary>
	/// Initializes termcap rendering options.
	/// </summary>
	/// <param name="maximumLineLength">
	/// Preferred maximum physical-line length. Individual headers or fields which
	/// cannot be split safely may exceed this value.
	/// </param>
	public TermcapRenderOptions(
		int maximumLineLength = DefaultMaximumLineLength
	) {
		if (
			maximumLineLength < 16
			|| maximumLineLength > MaximumSupportedLineLength
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumLineLength ),
				maximumLineLength,
				$"The maximum line length must be between 16 and {MaximumSupportedLineLength}."
			);
		}

		MaximumLineLength = maximumLineLength;
	}

	/// <summary>Gets the preferred maximum physical-line length.</summary>
	public int MaximumLineLength { get; }
}
