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
/// Represents one immutable protocol-neutral runtime observation about a
/// persistent-raster lifecycle semantic.
/// </summary>
public sealed class PersistentRasterRuntimeLifecycleObservation {
	/// <summary>
	/// Initializes one immutable lifecycle runtime observation.
	/// </summary>
	/// <param name="subject">The lifecycle semantic being observed.</param>
	/// <param name="outcome">The caller-owned runtime observation outcome.</param>
	/// <param name="sourceLabel">
	/// A bounded caller-owned provenance label preserved exactly and compared
	/// ordinally for deterministic ordering.
	/// </param>
	/// <param name="sourceOrdinal">
	/// A non-negative deterministic source-local ordinal supplied by the runtime
	/// verifier.
	/// </param>
	public PersistentRasterRuntimeLifecycleObservation(
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterRuntimeObservationOutcome outcome,
		string sourceLabel,
		int sourceOrdinal
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle runtime-observation subject must be a defined value."
			);
		}
		if ( !Enum.IsDefined( outcome ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( outcome ),
				outcome,
				"The runtime-observation outcome must be a defined value."
			);
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceLabel );
		if (
			sourceLabel.Length
				> PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength
		) {
			throw new ArgumentException(
				$"The runtime-observation source label cannot exceed {PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength} UTF-16 code units.",
				nameof( sourceLabel )
			);
		}
		if ( sourceOrdinal < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sourceOrdinal ),
				sourceOrdinal,
				"The lifecycle runtime-observation source ordinal cannot be negative."
			);
		}

		Subject = subject;
		Outcome = outcome;
		SourceLabel = sourceLabel;
		SourceOrdinal = sourceOrdinal;
	}

	/// <summary>Gets the lifecycle semantic being observed.</summary>
	public PersistentRasterLifecycleEvidenceSubject Subject {
		get;
	}

	/// <summary>Gets the caller-owned runtime observation outcome.</summary>
	public PersistentRasterRuntimeObservationOutcome Outcome {
		get;
	}

	/// <summary>Gets the exact bounded provenance label supplied by the caller.</summary>
	public string SourceLabel {
		get;
	}

	/// <summary>
	/// Gets the non-negative source-local ordinal supplied by the runtime verifier.
	/// </summary>
	public int SourceOrdinal {
		get;
	}
}
