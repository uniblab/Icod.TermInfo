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
/// Defines stable diagnostic codes emitted by termcap representability analysis
/// and reverse rendering.
/// </summary>
public static class TermcapRenderDiagnosticCodes {
	/// <summary>Terminal header metadata cannot be represented without changing its meaning.</summary>
	public const string HeaderNotRepresentable = "TREN0001";
	/// <summary>A Runtime standard capability has no unambiguous adopted termcap spelling.</summary>
	public const string StandardCapabilityNotRepresentable = "TREN0002";
	/// <summary>An extended capability name cannot be represented as an unmapped two-character termcap code.</summary>
	public const string ExtendedCapabilityNameNotRepresentable = "TREN0003";
	/// <summary>A numeric value is outside the numeric syntax accepted by the termcap parser.</summary>
	public const string NumericValueNotRepresentable = "TREN0004";
	/// <summary>A string contains bytes or delay syntax which cannot be represented faithfully.</summary>
	public const string StringValueNotRepresentable = "TREN0005";
	/// <summary>A string parameter program cannot be expressed by the adopted TC04 classic termcap operator subset.</summary>
	public const string ParameterProgramNotRepresentable = "TREN0006";
	/// <summary>An extended capability code would be interpreted as standard or reserved termcap syntax.</summary>
	public const string ExtendedCapabilityCollision = "TREN0007";
}
