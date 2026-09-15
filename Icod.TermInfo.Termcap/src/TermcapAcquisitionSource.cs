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
/// Describes the configured source which supplied an acquired root termcap
/// entry.
/// </summary>
public sealed class TermcapAcquisitionSource {
	internal TermcapAcquisitionSource(
		TermcapAcquisitionSourceKind kind,
		string identifier
	) {
		if ( !Enum.IsDefined( typeof( TermcapAcquisitionSourceKind ), kind ) ) {
			throw new ArgumentOutOfRangeException( nameof( kind ) );
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( identifier );

		Kind = kind;
		Identifier = identifier;
	}

	/// <summary>Gets the configured source category.</summary>
	public TermcapAcquisitionSourceKind Kind { get; }

	/// <summary>
	/// Gets the deterministic source identifier used for diagnostics. Inline
	/// acquisition uses a symbolic name; database acquisition uses the configured
	/// path.
	/// </summary>
	public string Identifier { get; }
}
