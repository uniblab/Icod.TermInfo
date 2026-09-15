/*
	Icod.TermInfo.Compiler
	Compiles terminfo source and terminal descriptions into deterministic terminfo databases.
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

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Selects the conventional compiled-terminfo numeric representation emitted by
/// <see cref="CompiledTermInfoWriter"/>.
/// </summary>
public enum CompiledTermInfoFormat {
	/// <summary>
	/// Prefer legacy <c>0432</c> and select wide <c>01036</c> only when a
	/// representable present numeric value requires it.
	/// </summary>
	Automatic = 0,

	/// <summary>
	/// Emit legacy <c>0432</c> exactly, failing when the description requires the
	/// wide numeric representation.
	/// </summary>
	Legacy = 1,

	/// <summary>
	/// Emit wide-numeric <c>01036</c> exactly, including when all present numeric
	/// values would fit the legacy representation.
	/// </summary>
	Wide = 2,
}