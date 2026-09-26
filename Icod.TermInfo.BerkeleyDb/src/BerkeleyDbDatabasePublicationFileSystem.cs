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

internal abstract class BerkeleyDbDatabasePublicationFileSystem {
	internal abstract void ValidatePaths( string destinationPath, string lockPath, bool overwriteExisting );
	internal abstract Stream OpenLock( string lockPath, bool createIfMissing );
	internal abstract Stream CreateTemporary( string temporaryPath );
	internal abstract void FlushToDisk( Stream stream );
	internal abstract Stream OpenRead( string temporaryPath );
	internal abstract void Move( string temporaryPath, string destinationPath, bool overwriteExisting );
	internal abstract void DeleteTemporary( string temporaryPath );
}
