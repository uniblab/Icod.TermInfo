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
/// Identifies how one termcap source construct was represented during conversion.
/// </summary>
public enum TermcapConversionDecision {
	/// <summary>The source construct maps directly to the canonical Runtime model.</summary>
	Exact = 0,

	/// <summary>An adopted historical alias maps exactly to its canonical Runtime identity.</summary>
	HistoricalAlias = 1,

	/// <summary>An unmapped two-character field is preserved as a Runtime extended capability.</summary>
	Extended = 2,

	/// <summary>The source construct required a deterministic but non-exact choice.</summary>
	Approximation = 3,

	/// <summary>The source construct is understood but is not supported by this conversion tranche.</summary>
	Unsupported = 4,

	/// <summary>The source value cannot be represented faithfully by the Runtime model.</summary>
	Unrepresentable = 5,
}
