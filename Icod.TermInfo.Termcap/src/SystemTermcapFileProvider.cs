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
/// Opens explicitly selected termcap database paths from the host filesystem.
/// </summary>
public sealed class SystemTermcapFileProvider : ITermcapFileProvider {
	/// <inheritdoc/>
	public bool TryOpenText(
		string path,
		[NotNullWhen( true )] out TextReader? reader
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			reader = File.OpenText( path );
			return true;
		} catch ( FileNotFoundException ) {
			reader = null;
			return false;
		} catch ( DirectoryNotFoundException ) {
			reader = null;
			return false;
		}
	}
}
