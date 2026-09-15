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
/// Provides managed equivalents of the traditional terminfo compatibility
/// operations without introducing process-global terminal state.
/// </summary>
/// <remarks>
/// The compatibility names intentionally resemble <c>tigetflag</c>,
/// <c>tigetnum</c>, <c>tigetstr</c>, <c>tparm</c>/<c>tiparm</c>,
/// <c>tputs</c>, and <c>putp</c>. Managed nullability and exceptions replace
/// the sentinel pointer and integer error values used by native terminfo APIs.
/// </remarks>
public static class TermInfoCompatibility {
	/// <summary>
	/// Gets a boolean capability by its traditional short name.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> when the capability is present; otherwise
	/// <see langword="false"/>. Unknown capability names are rejected.
	/// </returns>
	public static bool TiGetFlag(
		TerminalDescription terminal,
		string name
	) {
		ArgumentNullException.ThrowIfNull( terminal );
		ArgumentNullException.ThrowIfNull( name );

		terminal.TryGetBoolean( name, out bool value );
		return value;
	}

	/// <summary>
	/// Gets a numeric capability by its traditional short name.
	/// </summary>
	/// <returns>
	/// The capability value, or <see langword="null"/> when the known
	/// capability is absent. Unknown capability names are rejected.
	/// </returns>
	public static int? TiGetNum(
		TerminalDescription terminal,
		string name
	) {
		ArgumentNullException.ThrowIfNull( terminal );
		ArgumentNullException.ThrowIfNull( name );

		return ( terminal.TryGetNumber( name, out int value ) )
			? value
			: null
		;
	}

	/// <summary>
	/// Gets a string capability by its traditional short name.
	/// </summary>
	/// <returns>
	/// The capability string, or <see langword="null"/> when the known
	/// capability is absent. Unknown capability names are rejected.
	/// </returns>
	public static string? TiGetStr(
		TerminalDescription terminal,
		string name
	) {
		ArgumentNullException.ThrowIfNull( terminal );
		ArgumentNullException.ThrowIfNull( name );

		terminal.TryGetString( name, out string? value );
		return value;
	}

	/// <summary>
	/// Expands a terminfo parameter program with isolated variable storage.
	/// </summary>
	public static string TParm(
		string source,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( parameters );

		return TermInfoParameterExpander.Expand( source, parameters );
	}

	/// <summary>
	/// Expands a terminfo parameter program with an explicit persistent-variable
	/// context.
	/// </summary>
	public static string TParm(
		string source,
		TermInfoExpansionContext context,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( context );
		ArgumentNullException.ThrowIfNull( parameters );

		return TermInfoParameterExpander.Expand(
			source,
			context,
			parameters
		);
	}

	/// <summary>
	/// Expands a terminfo parameter program using the managed typed-parameter
	/// representation.
	/// </summary>
	public static string TiParm(
		string source,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( parameters );

		return TParm( source, parameters );
	}

	/// <summary>
	/// Expands a terminfo parameter program using the managed typed-parameter
	/// representation and an explicit persistent-variable context.
	/// </summary>
	public static string TiParm(
		string source,
		TermInfoExpansionContext context,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( context );
		ArgumentNullException.ThrowIfNull( parameters );

		return TParm( source, context, parameters );
	}

	/// <summary>
	/// Emits a terminfo string through a character callback using
	/// <c>tputs</c>-style affected-line semantics.
	/// </summary>
	public static void TPuts(
		string value,
		int affectedLines,
		Action<char> output,
		PaddingMode paddingMode = PaddingMode.Ignore,
		ITermInfoDelayProvider? delayProvider = null
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentNullException.ThrowIfNull( output );

		TermInfoOutput.TPuts(
			value,
			affectedLines,
			output,
			paddingMode,
			delayProvider
		);
	}

	/// <summary>
	/// Emits a terminfo string through a character callback using
	/// <c>putp</c>-style one-line semantics.
	/// </summary>
	public static void PutP(
		string value,
		Action<char> output,
		PaddingMode paddingMode = PaddingMode.Ignore,
		ITermInfoDelayProvider? delayProvider = null
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentNullException.ThrowIfNull( output );

		TPuts(
			value,
			1,
			output,
			paddingMode,
			delayProvider
		);
	}
}
