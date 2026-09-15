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
/// Configures deterministic compiled-terminfo format selection.
/// </summary>
public sealed class CompiledTermInfoWriterOptions {
	/// <summary>
	/// Initializes the default automatic writer policy.
	/// </summary>
	public CompiledTermInfoWriterOptions()
		: this(
			CompiledTermInfoFormat.Automatic,
			includeExtendedCapabilities: true
		) {
	}

	/// <summary>
	/// Initializes explicit writer policy.
	/// </summary>
	/// <param name="format">
	/// The conventional numeric representation policy.
	/// </param>
	/// <param name="includeExtendedCapabilities">
	/// Whether the selected representation permits the ncurses extended section.
	/// When <see langword="false"/>, a description containing extended
	/// capabilities is rejected rather than silently truncated.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="format"/> is not a defined <see cref="CompiledTermInfoFormat"/>.
	/// </exception>
	public CompiledTermInfoWriterOptions(
		CompiledTermInfoFormat format,
		bool includeExtendedCapabilities = true
	) {
		if ( !Enum.IsDefined(
			typeof( CompiledTermInfoFormat ),
			format
		) ) {
			throw new ArgumentOutOfRangeException(
				nameof( format ),
				format,
				"The compiled terminfo format must be Automatic, Legacy, or Wide."
			);
		}

		Format = format;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
	}

	/// <summary>
	/// Gets the conventional numeric representation policy.
	/// </summary>
	public CompiledTermInfoFormat Format { get; }

	/// <summary>
	/// Gets whether the selected representation permits an ncurses extended
	/// section.
	/// </summary>
	public bool IncludeExtendedCapabilities { get; }
}