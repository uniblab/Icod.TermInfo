/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
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

namespace Icod.TermInfo;

internal readonly record struct TermInfoOutputSegment {
	private TermInfoOutputSegment(
		string? text,
		decimal milliseconds,
		bool multiplyByAffectedLines,
		bool isMandatory
	) {
		Text = text;
		Milliseconds = milliseconds;
		MultiplyByAffectedLines = multiplyByAffectedLines;
		IsMandatory = isMandatory;
	}

	internal string? Text { get; }

	internal decimal Milliseconds { get; }

	internal bool MultiplyByAffectedLines { get; }

	internal bool IsMandatory { get; }

	internal bool IsPadding => Text is null;

	internal static TermInfoOutputSegment CreateText( string text ) {
		ArgumentNullException.ThrowIfNull( text );

		return new TermInfoOutputSegment(
			text,
			0m,
			false,
			false
		);
	}

	internal static TermInfoOutputSegment CreatePadding(
		decimal milliseconds,
		bool multiplyByAffectedLines,
		bool isMandatory
	) {
		if ( milliseconds < 0m ) {
			throw new ArgumentOutOfRangeException( nameof( milliseconds ) );
		}

		return new TermInfoOutputSegment(
			null,
			milliseconds,
			multiplyByAffectedLines,
			isMandatory
		);
	}
}
