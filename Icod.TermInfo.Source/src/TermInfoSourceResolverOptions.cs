/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

namespace Icod.TermInfo.Source;

/// <summary>
/// Configures bounded <c>use=</c> inheritance resolution.
/// </summary>
public sealed class TermInfoSourceResolverOptions {
	/// <summary>
	/// The default maximum number of inheritance edges from the requested root.
	/// </summary>
	public const int DefaultMaximumInheritanceDepth = 64;

	/// <summary>
	/// The largest inheritance-depth limit accepted by the resolver.
	/// </summary>
	public const int MaximumSupportedInheritanceDepth = 256;

	/// <summary>
	/// Initializes resolver options.
	/// </summary>
	/// <param name="maximumInheritanceDepth">
	/// The maximum number of <c>use=</c> edges permitted from the requested
	/// root. Zero permits only entries which require no parent resolution.
	/// </param>
	public TermInfoSourceResolverOptions(
		int maximumInheritanceDepth = DefaultMaximumInheritanceDepth
	) {
		if (
			( maximumInheritanceDepth < 0 )
			|| ( maximumInheritanceDepth > MaximumSupportedInheritanceDepth )
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumInheritanceDepth ),
				maximumInheritanceDepth,
				$"The maximum inheritance depth must be between 0 and {MaximumSupportedInheritanceDepth}, inclusive."
			);
		}

		MaximumInheritanceDepth = maximumInheritanceDepth;
	}

	/// <summary>
	/// Gets the maximum number of inheritance edges from the requested root.
	/// </summary>
	public int MaximumInheritanceDepth { get; }
}
