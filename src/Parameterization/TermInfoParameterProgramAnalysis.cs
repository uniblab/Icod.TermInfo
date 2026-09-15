/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
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

namespace Icod.TermInfo;

[Flags]
internal enum TermInfoParameterUsage {
	None = 0,
	Integer = 1,
	String = 2,
}

internal sealed class TermInfoParameterProgramAnalysis {
	internal TermInfoParameterProgramAnalysis(
		bool usesImplicitFormatParameters,
		IReadOnlyList<int> referencedParameterIndices,
		int highestParameterIndex,
		IReadOnlyList<TermInfoParameterUsage> parameterUsages,
		IReadOnlyList<char> dynamicVariables,
		IReadOnlyList<char> staticVariables,
		int instructionCount,
		int maximumConditionalNesting,
		int maximumStackDepth,
		bool mayUnderflow
	) {
		ArgumentNullException.ThrowIfNull( referencedParameterIndices );
		ArgumentNullException.ThrowIfNull( parameterUsages );
		ArgumentNullException.ThrowIfNull( dynamicVariables );
		ArgumentNullException.ThrowIfNull( staticVariables );

		if ( highestParameterIndex is < -1 or > 8 ) {
			throw new ArgumentOutOfRangeException( nameof( highestParameterIndex ) );
		}

		ArgumentOutOfRangeException.ThrowIfNegative( instructionCount );
		ArgumentOutOfRangeException.ThrowIfNegative( maximumConditionalNesting );
		ArgumentOutOfRangeException.ThrowIfNegative( maximumStackDepth );

		if ( parameterUsages.Count != 9 ) {
			throw new ArgumentException(
				"Terminfo parameter analysis must contain exactly nine usage slots.",
				nameof( parameterUsages )
			);
		}

		UsesImplicitFormatParameters = usesImplicitFormatParameters;
		ReferencedParameterIndices =
			Array.AsReadOnly( referencedParameterIndices.ToArray() );
		HighestParameterIndex = highestParameterIndex;
		ParameterUsages =
			Array.AsReadOnly( parameterUsages.ToArray() );
		DynamicVariables =
			Array.AsReadOnly( dynamicVariables.ToArray() );
		StaticVariables =
			Array.AsReadOnly( staticVariables.ToArray() );
		InstructionCount = instructionCount;
		MaximumConditionalNesting = maximumConditionalNesting;
		MaximumStackDepth = maximumStackDepth;
		MayUnderflow = mayUnderflow;
	}

	internal bool UsesImplicitFormatParameters { get; }

	internal IReadOnlyList<int> ReferencedParameterIndices { get; }

	internal int HighestParameterIndex { get; }

	internal IReadOnlyList<TermInfoParameterUsage> ParameterUsages { get; }

	internal IReadOnlyList<char> DynamicVariables { get; }

	internal IReadOnlyList<char> StaticVariables { get; }

	internal int InstructionCount { get; }

	internal int MaximumConditionalNesting { get; }

	internal int MaximumStackDepth { get; }

	internal bool MayUnderflow { get; }
}
