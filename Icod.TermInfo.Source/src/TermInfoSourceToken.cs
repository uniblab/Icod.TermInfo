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
/// Represents one semantic lexical unit from terminfo source.
/// </summary>
public sealed class TermInfoSourceToken {
	internal TermInfoSourceToken(
		TermInfoSourceTokenKind kind,
		string text,
		TermInfoSourceSpan span
	) {
		ArgumentNullException.ThrowIfNull( text );
		ArgumentNullException.ThrowIfNull( span );

		Kind = kind;
		Text = text;
		Span = span;
	}

	/// <summary>
	/// Gets the lexical classification.
	/// </summary>
	public TermInfoSourceTokenKind Kind { get; }

	/// <summary>
	/// Gets the exact source text covered by this token.
	/// </summary>
	/// <remarks>
	/// Capability token text remains encoded exactly as supplied. Escape,
	/// numeric, and string-value interpretation begins in S03.
	/// </remarks>
	public string Text { get; }

	/// <summary>
	/// Gets the token's location in the original source.
	/// </summary>
	public TermInfoSourceSpan Span { get; }
}
