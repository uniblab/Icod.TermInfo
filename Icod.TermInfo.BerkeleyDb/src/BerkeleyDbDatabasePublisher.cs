/*
	Icod.TermInfo.BerkeleyDb
	Managed read and write support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
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


namespace Icod.TermInfo.BerkeleyDb;

internal static class BerkeleyDbDatabasePublisher {
	internal static void Publish(
		string fullPath, byte[] image, IReadOnlyList<BerkeleyDbHashRecord> records,
		IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
		BerkeleyDbTerminalDatabaseWriterOptions options,
		BerkeleyDbDatabasePublicationFileSystem fileSystem, CancellationToken cancellationToken
	) => throw new NotImplementedException();
}
