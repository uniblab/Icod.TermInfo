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
/// Represents one unresolved termcap capability field.
/// </summary>
public sealed class TermcapSourceField {
	internal TermcapSourceField(
		TermcapSourceFieldKind kind,
		string capabilityName,
		int? numericValue,
		string? stringValue,
		string? referenceName,
		string text,
		TermcapSourceSpan span
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );
		ArgumentNullException.ThrowIfNull( text );
		ArgumentNullException.ThrowIfNull( span );

		Kind = kind;
		CapabilityName = capabilityName;
		NumericValue = numericValue;
		StringValue = stringValue;
		ReferenceName = referenceName;
		Text = text;
		Span = span;
	}

	/// <summary>Gets the unresolved field kind.</summary>
	public TermcapSourceFieldKind Kind { get; }

	/// <summary>Gets the exact two-character termcap capability name.</summary>
	public string CapabilityName { get; }

	/// <summary>Gets the decoded numeric value, when this is a numeric field.</summary>
	public int? NumericValue { get; }

	/// <summary>Gets the decoded string value, when this is a string field.</summary>
	public string? StringValue { get; }

	/// <summary>Gets the referenced terminal name for a <c>tc=</c> field.</summary>
	public string? ReferenceName { get; }

	/// <summary>Gets the logical field text after line-continuation removal.</summary>
	public string Text { get; }

	/// <summary>Gets the source span occupied by this field.</summary>
	public TermcapSourceSpan Span { get; }
}
