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

/// <summary>
/// Represents a parsed, reusable terminfo parameter program.
/// </summary>
public sealed class TermInfoParameterProgram {
	private readonly IReadOnlyList<TermInfoInstruction> _instructions;
	private readonly TermInfoParameterProgramAnalysis _analysis;

	private TermInfoParameterProgram(
		string source,
		IReadOnlyList<TermInfoInstruction> instructions,
		TermInfoParameterProgramAnalysis analysis
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( instructions );
		ArgumentNullException.ThrowIfNull( analysis );

		Source = source;
		_instructions = instructions;
		_analysis = analysis;
	}

	/// <summary>
	/// Gets the original terminfo parameter string.
	/// </summary>
	public string Source { get; }

	internal TermInfoParameterProgramAnalysis Analysis => _analysis;

	/// <summary>
	/// Parses a terminfo parameter string into a reusable program.
	/// </summary>
	public static TermInfoParameterProgram Parse( string source ) {
		ArgumentNullException.ThrowIfNull( source );

		TermInfoParameterParser parser = new( source );
		IReadOnlyList<TermInfoInstruction> instructions =
			parser.Parse();
		TermInfoParameterProgramAnalysis analysis =
			TermInfoParameterProgramAnalyzer.Analyze( instructions );

		return new TermInfoParameterProgram(
			source,
			instructions,
			analysis
		);
	}

	/// <summary>
	/// Expands the program with isolated uppercase-variable storage.
	/// </summary>
	public string Expand( params TermInfoParameter[] parameters ) {
		ArgumentNullException.ThrowIfNull( parameters );

		return Expand(
			new TermInfoExpansionContext(),
			parameters
		);
	}

	/// <summary>
	/// Expands the program using the supplied context for persistent uppercase variables.
	/// </summary>
	public string Expand(
		TermInfoExpansionContext context,
		params TermInfoParameter[] parameters
	) {
		ArgumentNullException.ThrowIfNull( context );
		ArgumentNullException.ThrowIfNull( parameters );

		return TermInfoParameterEvaluator.Evaluate(
			_instructions,
			_analysis,
			parameters,
			context
		);
	}
}
