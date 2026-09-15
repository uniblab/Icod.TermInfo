/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.BerkeleyDb;

/// <summary>Loads terminal descriptions through hashed-aware system discovery.</summary>
public sealed class BerkeleyDbSystemTerminalDescriptionProvider
	: ITerminalDescriptionProvider {
	/// <summary>Initializes a provider from a snapshot of permitted host discovery inputs.</summary>
	public BerkeleyDbSystemTerminalDescriptionProvider(
		BerkeleyDbSystemTerminalDescriptionProviderOptions? options = null
	) {
	}

	internal BerkeleyDbSystemTerminalDescriptionProvider(
		BerkeleyDbSystemTerminalDescriptionProviderOptions options,
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots
	) {
	}

	/// <inheritdoc/>
	public bool TryLoad(
		string name,
		[NotNullWhen( true )] out TerminalDescription? terminal
	) {
		throw new NotImplementedException();
	}
}
