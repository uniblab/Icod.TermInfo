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
/// Configures deterministic resource bounds for database-set semantic analysis.
/// </summary>
public sealed class TermInfoDatabaseSetSemanticAnalysisOptions {
	/// <summary>
	/// The default maximum number of alias declarations scanned across all
	/// physical occurrences.
	/// </summary>
	public const int DefaultMaximumAliasOccurrenceCount = 1_048_576;

	/// <summary>
	/// The largest supported caller-selected alias declaration bound.
	/// </summary>
	public const int MaximumSupportedAliasOccurrenceCount = 4_194_304;

	/// <summary>
	/// Initializes the canonical semantic-analysis resource policy.
	/// </summary>
	public TermInfoDatabaseSetSemanticAnalysisOptions()
		: this(
			DefaultMaximumAliasOccurrenceCount
		) {
	}

	/// <summary>
	/// Initializes an explicit deterministic alias-scan bound.
	/// </summary>
	/// <param name="maximumAliasOccurrenceCount">
	/// The maximum number of alias declarations scanned across all physical
	/// occurrences.
	/// </param>
	public TermInfoDatabaseSetSemanticAnalysisOptions(
		int maximumAliasOccurrenceCount
	) {
		if ( maximumAliasOccurrenceCount < 1
			|| maximumAliasOccurrenceCount > MaximumSupportedAliasOccurrenceCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumAliasOccurrenceCount ),
				maximumAliasOccurrenceCount,
				$"The maximum alias occurrence count must be between 1 and {MaximumSupportedAliasOccurrenceCount}."
			);
		}

		MaximumAliasOccurrenceCount = maximumAliasOccurrenceCount;
	}

	/// <summary>
	/// Gets the maximum number of alias declarations scanned during one analysis.
	/// </summary>
	public int MaximumAliasOccurrenceCount {
		get;
	}
}
