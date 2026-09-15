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
/// Identifies a contiguous span of caller-supplied termcap source text.
/// </summary>
/// <remarks>
/// <see cref="Offset"/> and <see cref="Length"/> are measured in UTF-16 code
/// units. <see cref="Line"/> and <see cref="Column"/> are one-based.
/// </remarks>
public sealed class TermcapSourceSpan {
	/// <summary>
	/// Initializes a termcap source span.
	/// </summary>
	/// <param name="sourceName">Optional caller-supplied source identity.</param>
	/// <param name="offset">Zero-based UTF-16 source offset.</param>
	/// <param name="line">One-based source line.</param>
	/// <param name="column">One-based source column.</param>
	/// <param name="length">Span length in UTF-16 code units.</param>
	public TermcapSourceSpan(
		string? sourceName,
		int offset,
		int line,
		int column,
		int length
	) {
		if ( offset < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( offset ),
				offset,
				"The source offset cannot be negative."
			);
		}
		if ( line < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( line ),
				line,
				"The source line must be at least one."
			);
		}
		if ( column < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( column ),
				column,
				"The source column must be at least one."
			);
		}
		if (
			length < 0
			|| offset > int.MaxValue - length
		) {
			throw new ArgumentOutOfRangeException(
				nameof( length ),
				length,
				"The source span length must be non-negative and fit within the source offset range."
			);
		}

		SourceName = sourceName;
		Offset = offset;
		Line = line;
		Column = column;
		Length = length;
	}

	/// <summary>
	/// Gets the optional caller-supplied source identity.
	/// </summary>
	public string? SourceName { get; }

	/// <summary>
	/// Gets the zero-based UTF-16 source offset.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// Gets the one-based source line.
	/// </summary>
	public int Line { get; }

	/// <summary>
	/// Gets the one-based source column.
	/// </summary>
	public int Column { get; }

	/// <summary>
	/// Gets the span length in UTF-16 code units.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// Gets the exclusive zero-based offset at the end of the span.
	/// </summary>
	public int EndOffset => checked( Offset + Length );
}
