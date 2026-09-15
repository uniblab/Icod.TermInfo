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

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies how a parsed termcap capability code relates to the canonical
/// Runtime standard-capability catalog.
/// </summary>
public enum TermcapCapabilityClassification {
	/// <summary>
	/// The code maps uniquely to a current standard Runtime capability.
	/// </summary>
	Standard = 0,

	/// <summary>
	/// The code maps uniquely to an obsolete termcap compatibility capability
	/// retained by the Runtime standard catalog.
	/// </summary>
	ObsoleteStandard = 1,

	/// <summary>
	/// The code is an explicitly recognized obsolete non-standard alias which
	/// maps to a current Runtime standard capability.
	/// </summary>
	ObsoleteAlias = 2,

	/// <summary>
	/// The code has more than one semantic mapping and therefore cannot be
	/// classified as one Runtime capability without additional policy.
	/// </summary>
	Ambiguous = 3,

	/// <summary>
	/// The syntactically valid two-character code is not currently mapped.
	/// </summary>
	Unmapped = 4,

	/// <summary>
	/// The field is the termcap <c>tc=</c> inheritance reference rather than a
	/// capability value.
	/// </summary>
	Reference = 5,
}
