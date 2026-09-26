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

internal static class BerkeleyDbDatabasePublicationLock {
	internal static IDisposable Acquire(
		string destinationPath, string lockPath, bool overwriteExisting,
		BerkeleyDbDatabasePublicationFileSystem fileSystem, CancellationToken cancellationToken
	) {
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			fileSystem.ValidatePaths( destinationPath, lockPath, overwriteExisting );
			Stream held;
			try {
				held = fileSystem.OpenLock( lockPath, true );
			}
			catch ( IOException exception ) when ( IsContention( exception ) ) {
				cancellationToken.WaitHandle.WaitOne( 50 );
				continue;
			}
			try {
				bool enforced = false;
				try {
					using Stream probe = fileSystem.OpenLock( lockPath, false );
				}
				catch ( IOException exception ) when ( IsContention( exception ) ) {
					enforced = true;
				}
				if ( !enforced ) {
					throw new NotSupportedException( "The filesystem does not enforce exclusive publication locks." );
				}
				cancellationToken.ThrowIfCancellationRequested();
				fileSystem.ValidatePaths( destinationPath, lockPath, overwriteExisting );
				return held;
			}
			catch {
				try {
					held.Dispose();
				}
				catch ( IOException ) { }
				catch ( UnauthorizedAccessException ) { }
				throw;
			}
		}
	}

	internal static bool IsContention( IOException exception ) {
		if ( OperatingSystem.IsWindows() ) {
			return exception.HResult is unchecked( (int)0x80070020 ) or unchecked( (int)0x80070021 );
		}
		return ( OperatingSystem.IsMacOS() )
			? exception.HResult == 35
			: OperatingSystem.IsLinux() && exception.HResult == 11
		;
	}
}
