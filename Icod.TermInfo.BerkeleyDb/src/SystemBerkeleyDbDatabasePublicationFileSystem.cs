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

internal sealed class SystemBerkeleyDbDatabasePublicationFileSystem : BerkeleyDbDatabasePublicationFileSystem {
	internal override void ValidatePaths( string destinationPath, string lockPath, bool overwriteExisting ) {
		string parent = Path.GetDirectoryName( destinationPath )
			?? throw new IOException( "A database destination must have a parent directory." );
		FileAttributes parentAttributes = File.GetAttributes( parent );
		if ( ( parentAttributes & FileAttributes.Directory ) == 0
			|| ( parentAttributes & FileAttributes.ReparsePoint ) != 0 ) {
			throw new IOException( "The immediate parent must be an ordinary directory." );
		}
		bool destinationExists = ValidateFile( destinationPath );
		ValidateFile( lockPath );
		if ( destinationExists && !overwriteExisting ) {
			throw new IOException( "The database destination already exists." );
		}
	}

	private static bool ValidateFile( string path ) {
		FileAttributes attributes;
		try {
			attributes = File.GetAttributes( path );
		}
		catch ( FileNotFoundException ) {
			return false;
		}
		if ( ( attributes & ( FileAttributes.Directory | FileAttributes.ReparsePoint ) ) != 0 ) {
			throw new IOException( "Publication paths must not be directories or symbolic links/reparse points." );
		}
		return true;
	}

	internal override Stream OpenLock( string lockPath, bool createIfMissing ) {
		FileMode mode = ( createIfMissing )
			? FileMode.OpenOrCreate
			: FileMode.Open
		;
		return new FileStream( lockPath, mode, FileAccess.ReadWrite, FileShare.None );
	}

	internal override Stream CreateTemporary( string temporaryPath ) => throw new NotImplementedException();
	internal override void FlushToDisk( Stream stream ) => throw new NotImplementedException();
	internal override Stream OpenRead( string temporaryPath ) => throw new NotImplementedException();
	internal override void Move( string temporaryPath, string destinationPath, bool overwriteExisting ) => throw new NotImplementedException();
	internal override void DeleteTemporary( string temporaryPath ) => throw new NotImplementedException();
}
