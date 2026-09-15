/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the provenance class of a raster-backend availability assertion.
/// </summary>
public enum RasterBackendEvidenceKind {
	/// <summary>Evidence recognized from static terminal capability metadata.</summary>
	CapabilityDerived = 0,

	/// <summary>Evidence explicitly declared by the caller without live verification.</summary>
	Declared = 1,

	/// <summary>Evidence supplied as the result of verification outside Icod.TermInfo.</summary>
	Verified = 2,
}
