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

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Supplies text readers for explicitly selected termcap database paths.
/// </summary>
/// <remarks>
/// A clean missing path returns <see langword="false"/> and a null reader.
/// Provider failures other than a clean miss propagate to the caller. A reader
/// returned on success is owned and disposed by the acquisition operation.
/// </remarks>
public interface ITermcapFileProvider {
	/// <summary>
	/// Attempts to open one termcap database path for bounded parser input.
	/// </summary>
	bool TryOpenText(
		string path,
		[NotNullWhen( true )] out TextReader? reader
	);
}
