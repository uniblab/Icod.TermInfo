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
/// Associates one effective parent terminal description with the exact source
/// reference name to emit in a synthesized <c>use=</c> field.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UseName"/> is source-reference identity and is preserved exactly;
/// it may intentionally differ from <see cref="TerminalDescription.Name"/>,
/// for example when the caller wants to emit an alias.
/// </para>
/// <para>
/// The same effective description may be supplied more than once when each
/// occurrence has a distinct valid <see cref="UseName"/>.
/// </para>
/// </remarks>
public sealed class TerminalDescriptionSourceSynthesisParent {
	/// <summary>
	/// Initializes one ordered synthesis parent.
	/// </summary>
	/// <param name="useName">
	/// The exact terminal name or alias to emit after <c>use=</c>.
	/// </param>
	/// <param name="description">
	/// The effective semantics contributed by the referenced parent.
	/// </param>
	/// <exception cref="ArgumentException">
	/// <paramref name="useName"/> is empty, whitespace, or cannot be represented
	/// losslessly as a terminfo source terminal name.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="useName"/> or <paramref name="description"/> is
	/// <see langword="null"/>.
	/// </exception>
	public TerminalDescriptionSourceSynthesisParent(
		string useName,
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( useName );
		ArgumentNullException.ThrowIfNull( description );
		if ( string.IsNullOrWhiteSpace( useName ) ) {
			throw new ArgumentException(
				"The use= reference name cannot be empty or whitespace.",
				nameof( useName )
			);
		}

		ValidateUseName( useName );

		UseName = useName;
		Description = description;
	}

	/// <summary>
	/// Gets the exact source reference name to emit after <c>use=</c>.
	/// </summary>
	public string UseName {
		get;
	}

	/// <summary>
	/// Gets the effective parent terminal description.
	/// </summary>
	public TerminalDescription Description {
		get;
	}

	private static void ValidateUseName(
		string useName
	) {
		foreach ( char character in useName ) {
			if ( char.IsWhiteSpace( character )
				|| char.IsControl( character )
				|| character == '|'
				|| character == ',' ) {
				throw new ArgumentException(
					$"The use= reference name '{useName}' contains a character "
						+ "which cannot be represented losslessly in terminfo source.",
					nameof( useName )
				);
			}
		}

		int trailingBackslashes = 0;
		for (
			int index = useName.Length - 1;
			index >= 0 && useName[ index ] == '\\';
			index--
		) {
			trailingBackslashes++;
		}
		if ( ( trailingBackslashes & 1 ) != 0 ) {
			throw new ArgumentException(
				$"The use= reference name '{useName}' ends with an unpaired "
					+ "backslash which would escape the source field terminator.",
				nameof( useName )
			);
		}
	}
}
