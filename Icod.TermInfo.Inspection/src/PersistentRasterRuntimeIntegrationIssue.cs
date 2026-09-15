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
/// Represents one immutable structured limitation encountered while integrating
/// persistent-raster runtime observations.
/// </summary>
public sealed class PersistentRasterRuntimeIntegrationIssue {
	internal PersistentRasterRuntimeIntegrationIssue(
		PersistentRasterRuntimeIntegrationIssueKind kind,
		int existingEvidenceCount,
		int requestedImportCount
	) {
		if ( !Enum.IsDefined( kind ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The runtime integration issue kind must be a defined value."
			);
		}
		if ( existingEvidenceCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( existingEvidenceCount ),
				existingEvidenceCount,
				"The existing evidence count cannot be negative."
			);
		}
		if ( requestedImportCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( requestedImportCount ),
				requestedImportCount,
				"The requested import count cannot be negative."
			);
		}

		Kind = kind;
		ExistingEvidenceCount = existingEvidenceCount;
		RequestedImportCount = requestedImportCount;
	}

	/// <summary>Gets the represented integration limitation.</summary>
	public PersistentRasterRuntimeIntegrationIssueKind Kind {
		get;
	}

	/// <summary>
	/// Gets the number of existing evidence assertions in the affected family.
	/// </summary>
	public int ExistingEvidenceCount {
		get;
	}

	/// <summary>
	/// Gets the number of conclusive runtime observations requested for import into
	/// the affected family.
	/// </summary>
	public int RequestedImportCount {
		get;
	}
}
