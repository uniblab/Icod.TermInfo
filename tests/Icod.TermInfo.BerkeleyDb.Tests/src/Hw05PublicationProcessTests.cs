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


using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw05PublicationProcessTests {
	[Fact]
	public async Task IndependentProcessHonorsHeldLock() => await Run( killHolder: false );

	[Fact]
	public async Task KilledHolderReleasesLock() => await Run( killHolder: true );

	[Fact]
	public async Task PersistentLockIsReusable() {
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.LockPath, [ 3, 1, 4 ] );
		for ( int attempt = 0; attempt < 2; attempt++ ) {
			using var holder = Start( "hold-lock", scope.LockPath );
			try {
				Assert.Equal( "acquired", await holder.Process.StandardOutput.ReadLineAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) ) );
				AssertContention( scope.LockPath );
				await holder.Process.StandardInput.WriteLineAsync( "release" );
				await AssertExit( holder );
			}
			finally {
				await holder.Stop();
			}
		}
		Assert.Equal( new byte[] { 3, 1, 4 }, File.ReadAllBytes( scope.LockPath ) );
	}

	private static async Task Run( bool killHolder ) {
		using var scope = new Hw05PublicationTestSupport();
		var data = Hw05PublicationVerificationTests.Prepare();
		string payload = Path.Combine( scope.DirectoryPath, "compiled" );
		File.WriteAllBytes( payload, data.Publications[0].Data );
		using var holder = Start( "hold-lock", scope.LockPath );
		Child? writer = null;
		try {
			Assert.Equal( "acquired", await holder.Process.StandardOutput.ReadLineAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) ) );
			writer = Start( "write", scope.Destination, payload, "hw05-primary", "hw05-alias", "true" );
			Assert.Equal( "ready", await writer.Process.StandardOutput.ReadLineAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) ) );
			AssertContention( scope.LockPath );
			Assert.False( File.Exists( scope.Destination ) );
			if ( killHolder ) {
				holder.Process.Kill( entireProcessTree: true );
				await holder.Process.WaitForExitAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) );
			}
			else {
				await holder.Process.StandardInput.WriteLineAsync( "release" );
				await AssertExit( holder );
			}
			Assert.Equal( "published", await writer.Process.StandardOutput.ReadLineAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) ) );
			await AssertExit( writer );
			Assert.Equal( data.Image, File.ReadAllBytes( scope.Destination ) );
			BerkeleyDbDatabasePublicationVerifier.Verify( File.ReadAllBytes( scope.Destination ), data.Image, data.Records, data.Publications, new(), default );
			using var reacquired = scope.Acquire();
		}
		finally {
			if ( writer is not null ) {
				await writer.Stop();
				writer.Dispose();
			}
			await holder.Stop();
		}
	}
	private static void AssertContention( string path ) {
		var error = Assert.Throws<IOException>( () => {
			using var probe = new FileStream( path, FileMode.Open, FileAccess.ReadWrite, FileShare.None );
		}
		);
		Assert.True( BerkeleyDbDatabasePublicationLock.IsContention( error ), $"Unexpected contention HRESULT: {error.HResult:X8}" );
	}
	private static Child Start( params string[] arguments ) {
		string host = Environment.GetEnvironmentVariable( "DOTNET_HOST_PATH" )
			?? Path.Combine( Path.GetDirectoryName( RuntimeEnvironment.GetRuntimeDirectory().TrimEnd( Path.DirectorySeparatorChar ) )!, "..", "..", ( OperatingSystem.IsWindows() ) ? "dotnet.exe" : "dotnet" );
		var start = new ProcessStartInfo( Path.GetFullPath( host ) ) {
			RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
			UseShellExecute = false
		};
		start.ArgumentList.Add( Path.Combine( AppContext.BaseDirectory, "publication-probe", "Hw05.PublicationProbe.dll" ) );
		foreach ( string argument in arguments ) {
			start.ArgumentList.Add( argument );
		}
		return new Child( Process.Start( start ) ?? throw new InvalidOperationException( "Could not start publication probe." ) );
	}
	private static async Task AssertExit( Child child ) {
		await child.Process.WaitForExitAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) );
		Assert.True( child.Process.ExitCode == 0, await child.Error );
	}
	private sealed class Child( Process process ) : IDisposable {
		internal Process Process { get; } = process;
		internal Task<string> Error { get; } = process.StandardError.ReadToEndAsync();
		internal async Task Stop() {
			if ( !Process.HasExited ) {
				Process.Kill( entireProcessTree: true );
			}
			await Process.WaitForExitAsync().WaitAsync( TimeSpan.FromSeconds( 15 ) );
			await Error;
		}
		public void Dispose() => Process.Dispose();
	}
}
