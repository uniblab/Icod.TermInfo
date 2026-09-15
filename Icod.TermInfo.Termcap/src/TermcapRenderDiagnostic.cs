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

using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes one deterministic termcap representability or rendering decision.
/// </summary>
public sealed class TermcapRenderDiagnostic {
	internal TermcapRenderDiagnostic(
		string code,
		TermcapRenderDiagnosticSeverity severity,
		string message,
		string? capabilityName = null,
		TermInfoCapabilityValueKind? valueKind = null
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );

		Code = code;
		Severity = severity;
		Message = message;
		CapabilityName = capabilityName;
		ValueKind = valueKind;
	}

	/// <summary>Gets the stable diagnostic code.</summary>
	public string Code { get; }

	/// <summary>Gets the diagnostic severity.</summary>
	public TermcapRenderDiagnosticSeverity Severity { get; }

	/// <summary>Gets the deterministic diagnostic message.</summary>
	public string Message { get; }

	/// <summary>
	/// Gets the Runtime short name or extended capability name associated with the
	/// diagnostic, when applicable.
	/// </summary>
	public string? CapabilityName { get; }

	/// <summary>Gets the capability value kind associated with the diagnostic, when applicable.</summary>
	public TermInfoCapabilityValueKind? ValueKind { get; }
}
