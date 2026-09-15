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
/// Controls publication of compiled entries into a conventional terminfo
/// directory tree.
/// </summary>
public sealed class CompiledTermInfoDatabaseWriterOptions {
	/// <summary>
	/// Initializes the default database writer policy.
	/// </summary>
	public CompiledTermInfoDatabaseWriterOptions()
		: this(
			overwriteExisting: false
		) {
	}

	/// <summary>
	/// Initializes the database writer policy.
	/// </summary>
	/// <param name="overwriteExisting">
	/// <see langword="true"/> to replace existing compiled entry files;
	/// otherwise an existing destination causes the write to fail.
	/// </param>
	public CompiledTermInfoDatabaseWriterOptions(
		bool overwriteExisting
	) {
		OverwriteExisting = overwriteExisting;
	}

	/// <summary>
	/// Gets whether existing compiled entry files may be replaced.
	/// </summary>
	public bool OverwriteExisting { get; }
}
