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

/// <summary>
/// Owns persistent uppercase terminfo variables for a sequence of expansions.
/// </summary>
/// <remarks>
/// Lowercase variables are dynamic and are reset for every expansion. Uppercase
/// variables persist only when callers explicitly reuse the same context.
/// </remarks>
public sealed class TermInfoExpansionContext {
	private readonly TermInfoParameter[] _staticVariables =
		new TermInfoParameter[26];

	internal object SyncRoot { get; } = new();

	/// <summary>
	/// Resets all persistent uppercase variables to integer zero.
	/// </summary>
	public void Reset() {
		lock ( SyncRoot ) {
			Array.Clear( _staticVariables );
		}
	}

	internal TermInfoParameter GetStaticVariable( char name ) {
		ValidateStaticVariableName( name );
		return _staticVariables[name - 'A'];
	}

	internal void SetStaticVariable(
		char name,
		TermInfoParameter value
	) {
		ValidateStaticVariableName( name );
		_staticVariables[name - 'A'] = value;
	}

	private static void ValidateStaticVariableName( char name ) {
		if ( name is < 'A' or > 'Z' ) {
			throw new ArgumentOutOfRangeException( nameof( name ) );
		}
	}
}
