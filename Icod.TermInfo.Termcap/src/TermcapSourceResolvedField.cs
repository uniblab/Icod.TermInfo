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
/// Represents one effective termcap field after <c>tc=</c> inheritance has been
/// resolved.
/// </summary>
public sealed class TermcapSourceResolvedField {
	internal TermcapSourceResolvedField(
		TermcapSourceEntry sourceEntry,
		TermcapSourceField sourceField,
		int inheritanceDepth
	) {
		ArgumentNullException.ThrowIfNull( sourceEntry );
		ArgumentNullException.ThrowIfNull( sourceField );
		ArgumentOutOfRangeException.ThrowIfNegative( inheritanceDepth );

		SourceEntry = sourceEntry;
		SourceField = sourceField;
		InheritanceDepth = inheritanceDepth;
	}

	/// <summary>
	/// Gets the unresolved source entry which supplied this effective field.
	/// </summary>
	public TermcapSourceEntry SourceEntry { get; }

	/// <summary>
	/// Gets the original unresolved source field, including its source span.
	/// </summary>
	public TermcapSourceField SourceField { get; }

	/// <summary>
	/// Gets the exact two-character termcap capability name.
	/// </summary>
	public string CapabilityName => SourceField.CapabilityName;

	/// <summary>
	/// Gets the number of <c>tc=</c> edges between the requested root and the
	/// entry which supplied this field.
	/// </summary>
	public int InheritanceDepth { get; }

	/// <summary>
	/// Gets whether this field came from an inherited entry.
	/// </summary>
	public bool IsInherited => InheritanceDepth != 0;
}
