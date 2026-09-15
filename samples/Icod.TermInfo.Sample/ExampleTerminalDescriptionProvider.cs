/*
	Icod.TermInfo.Sample
	Demonstrates Icod.TermInfo APIs and integration patterns.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Sample;

internal sealed class ExampleTerminalDescriptionProvider : ITerminalDescriptionProvider {
	private readonly InMemoryTerminalDescriptionProvider _inner;

	internal ExampleTerminalDescriptionProvider() {
		TerminalDescription terminal =
			new TerminalDescriptionBuilder( "example-terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString(
					StringCapability.CursorAddress,
					"\x1b[%i%p1%d;%p2%dH"
				)
				.SetString(
					StringCapability.ClearScreen,
					"\x1b[H\x1b[J"
				)
				.Build();

		_inner =
			new InMemoryTerminalDescriptionProvider(
				[terminal]
			);
	}

	public bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	) {
		ArgumentNullException.ThrowIfNull( name );

		return _inner.TryLoad( name, out terminal );
	}
}
