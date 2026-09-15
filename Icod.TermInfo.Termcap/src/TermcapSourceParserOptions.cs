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
/// Controls resource limits for termcap source parsing.
/// </summary>
public sealed class TermcapSourceParserOptions {
	/// <summary>
	/// Gets the default maximum accepted source length in UTF-16 code units.
	/// </summary>
	public const int DefaultMaximumSourceLength = 4 * 1024 * 1024;

	/// <summary>
	/// Gets the largest maximum source length accepted by the parser.
	/// </summary>
	public const int MaximumSupportedSourceLength = 64 * 1024 * 1024;

	/// <summary>
	/// Initializes parser resource limits.
	/// </summary>
	/// <param name="maximumSourceLength">Maximum accepted source length in UTF-16 code units.</param>
	public TermcapSourceParserOptions(
		int maximumSourceLength = DefaultMaximumSourceLength
	) {
		if (
			maximumSourceLength < 1
			|| maximumSourceLength > MaximumSupportedSourceLength
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSourceLength ),
				maximumSourceLength,
				$"The maximum source length must be between 1 and {MaximumSupportedSourceLength}."
			);
		}

		MaximumSourceLength = maximumSourceLength;
	}

	/// <summary>
	/// Gets the maximum accepted source length in UTF-16 code units.
	/// </summary>
	public int MaximumSourceLength { get; }
}
