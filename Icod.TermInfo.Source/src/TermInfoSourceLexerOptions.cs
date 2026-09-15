/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

namespace Icod.TermInfo.Source;

/// <summary>
/// Configures immutable resource limits for terminfo source tokenization.
/// </summary>
public sealed class TermInfoSourceLexerOptions {
	/// <summary>
	/// The default maximum source length: 4 Mi UTF-16 code units.
	/// </summary>
	public const int DefaultMaximumSourceLength = 4_194_304;

	/// <summary>
	/// The largest configurable source length: 64 Mi UTF-16 code units.
	/// </summary>
	public const int MaximumSupportedSourceLength = 67_108_864;

	/// <summary>
	/// Initializes immutable lexer options.
	/// </summary>
	/// <param name="maximumSourceLength">
	/// The largest complete source document, in UTF-16 code units, the lexer
	/// will accept.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="maximumSourceLength"/> is less than one or greater than
	/// <see cref="MaximumSupportedSourceLength"/>.
	/// </exception>
	public TermInfoSourceLexerOptions(
		int maximumSourceLength = DefaultMaximumSourceLength
	) {
		if (
			( maximumSourceLength <= 0 )
			|| ( maximumSourceLength > MaximumSupportedSourceLength )
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSourceLength ),
				maximumSourceLength,
				$"The maximum source length must be between 1 and {MaximumSupportedSourceLength} UTF-16 code units."
			);
		}

		MaximumSourceLength = maximumSourceLength;
	}

	/// <summary>
	/// Gets the largest source document accepted by the lexer, in UTF-16 code
	/// units.
	/// </summary>
	public int MaximumSourceLength { get; }
}
