/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HW05 safe Hash-v9 filesystem publication.
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

namespace Icod.TermInfo.BerkeleyDb.Tests;

internal sealed class Hw05PublicationTestSupport : IDisposable {
	internal string DirectoryPath { get; } = Path.Combine( Path.GetTempPath(), "icod-hw05-" + Guid.NewGuid().ToString( "N" ) );
	internal string Destination => Path.Combine( DirectoryPath, "terminfo.db" );
	internal string LockPath => Path.Combine( DirectoryPath, ".terminfo.db.icod-terminfo.lock" );

	internal Hw05PublicationTestSupport() {
		Directory.CreateDirectory( DirectoryPath );
	}

	internal IDisposable Acquire( BerkeleyDbDatabasePublicationFileSystem? fs = null, CancellationToken token = default ) {
		return BerkeleyDbDatabasePublicationLock.Acquire(
			Destination, LockPath, true, fs ?? new SystemBerkeleyDbDatabasePublicationFileSystem(), token
		);
	}

	public void Dispose() {
		if ( Directory.Exists( DirectoryPath ) ) {
			Directory.Delete( DirectoryPath, true );
		}
	}

	internal class FileSystem : BerkeleyDbDatabasePublicationFileSystem {
		internal readonly SystemBerkeleyDbDatabasePublicationFileSystem Inner = new();
		internal Action? OnContention { get; init; }
		internal Exception? LockError { get; init; }
		internal bool IgnoreLocking { get; init; }
		internal int LockAttempts { get; private set; }
		internal List<MemoryStream> UnlockedStreams { get; } = [];

		internal override void ValidatePaths( string destination, string lockPath, bool overwrite ) => Inner.ValidatePaths( destination, lockPath, overwrite );
		internal override Stream OpenLock( string path, bool create ) {
			LockAttempts++;
			if ( LockError is not null ) {
				throw LockError;
			}
			if ( IgnoreLocking ) {
				var stream = new MemoryStream();
				UnlockedStreams.Add( stream );
				return stream;
			}
			try {
				return Inner.OpenLock( path, create );
			}
			catch ( IOException ) {
				OnContention?.Invoke();
				throw;
			}
		}
		internal override Stream CreateTemporary( string path ) => Inner.CreateTemporary( path );
		internal override void FlushToDisk( Stream stream ) => Inner.FlushToDisk( stream );
		internal override Stream OpenRead( string path ) => Inner.OpenRead( path );
		internal override void Move( string source, string destination, bool overwrite ) => Inner.Move( source, destination, overwrite );
		internal override void DeleteTemporary( string path ) => Inner.DeleteTemporary( path );
	}
}
