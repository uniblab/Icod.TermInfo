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
	) {
		string parent = Path.GetDirectoryName( fullPath )!;
		string name = Path.GetFileName( fullPath );
		string lockPath = Path.Combine( parent, $".{name}.icod-terminfo.lock" );
		string temporaryPath = Path.Combine( parent, $".{name}.icod-terminfo-{Guid.NewGuid():N}.tmp" );
		IDisposable held = BerkeleyDbDatabasePublicationLock.Acquire(
			fullPath, lockPath, options.OverwriteExisting, fileSystem, cancellationToken
		);
		bool ownsTemporary = false;
		Exception? failure = null;
		try {
			cancellationToken.ThrowIfCancellationRequested();
			Stream staged = fileSystem.CreateTemporary( temporaryPath );
			ownsTemporary = true;
			WithOwnedStream( staged, () => {
				for ( int offset = 0; offset < image.Length; ) {
					cancellationToken.ThrowIfCancellationRequested();
					int count = Math.Min( 81_920, image.Length - offset );
					staged.Write( image, offset, count );
					offset += count;
				}
				cancellationToken.ThrowIfCancellationRequested();
				fileSystem.FlushToDisk( staged );
			} );
			cancellationToken.ThrowIfCancellationRequested();
			byte[] reopened = [];
			Stream source = fileSystem.OpenRead( temporaryPath );
			WithOwnedStream( source, () => {
				using var cancellable = new BerkeleyDbCancellationReadStream( source, cancellationToken );
				reopened = BerkeleyDbHashReader.ReadStableDatabase( cancellable, options.MaximumDatabaseSize );
			} );
			BerkeleyDbDatabasePublicationVerifier.Verify(
				reopened, image, records, publications, options, cancellationToken
			);
			fileSystem.ValidatePaths( fullPath, lockPath, options.OverwriteExisting );
			cancellationToken.ThrowIfCancellationRequested();
			// Commit is irreversible: cancellation is deliberately not observed from here.
			fileSystem.Move( temporaryPath, fullPath, options.OverwriteExisting );
			ownsTemporary = false;
		}
		catch ( Exception exception ) {
			failure = exception;
			throw;
		}
		finally {
			try {
				if ( ownsTemporary ) {
					try {
						fileSystem.DeleteTemporary( temporaryPath );
					}
					catch ( IOException ) when ( failure is not null ) { }
					catch ( UnauthorizedAccessException ) when ( failure is not null ) { }
				}
			}
			finally {
				try {
					held.Dispose();
				}
				catch ( IOException ) when ( failure is not null ) { }
				catch ( UnauthorizedAccessException ) when ( failure is not null ) { }
			}
		}
	}

	private static void WithOwnedStream( Stream stream, Action operation ) {
		Exception? failure = null;
		try {
			operation();
		}
		catch ( Exception exception ) {
			failure = exception;
			throw;
		}
		finally {
			try {
				stream.Dispose();
			}
			catch ( Exception ) when ( failure is not null ) {
				// A secondary close error must not replace the operation's failure.
			}
		}
	}
}
