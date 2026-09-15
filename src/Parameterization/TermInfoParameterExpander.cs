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
/// Expands terminfo parameter strings.
/// </summary>
public static class TermInfoParameterExpander {
	/// <summary>
	/// Parses and expands a terminfo parameter string with isolated variable storage.
	/// </summary>
	public static string Expand(
		string source,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( parameters );

		return TermInfoParameterProgram
			.Parse( source )
			.Expand( parameters );
	}

	/// <summary>
	/// Parses and expands a terminfo parameter string using the supplied context.
	/// </summary>
	public static string Expand(
		string source,
		TermInfoExpansionContext context,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( context );
		ArgumentNullException.ThrowIfNull( parameters );

		return TermInfoParameterProgram
			.Parse( source )
			.Expand(
				context,
				parameters
			);
	}
}
