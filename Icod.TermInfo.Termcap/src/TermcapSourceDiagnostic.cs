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
/// Describes a deterministic termcap source parsing diagnostic.
/// </summary>
public sealed class TermcapSourceDiagnostic {
	internal TermcapSourceDiagnostic(
		string code,
		TermcapSourceDiagnosticSeverity severity,
		string message,
		TermcapSourceSpan? span
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );

		Code = code;
		Severity = severity;
		Message = message;
		Span = span;
	}

	/// <summary>Gets the stable diagnostic code.</summary>
	public string Code { get; }

	/// <summary>Gets the diagnostic severity.</summary>
	public TermcapSourceDiagnosticSeverity Severity { get; }

	/// <summary>Gets the diagnostic message.</summary>
	public string Message { get; }

	/// <summary>Gets the related source span, when one is available.</summary>
	public TermcapSourceSpan? Span { get; }
}
