/*
	Icod.TermInfo.Tests
	Characterizes HDB07 direct-provider and TerminalDatabase routing boundaries.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Hdb07RouterFailureBoundaryTests {
	[Fact]
	public void RoutedValidResultMatchesDirectProvider() {
		using TemporaryRoot temporary = new();
		string name = "hdb07-routed-valid";
		string path = temporary.Write(
			"valid.db",
			CreateProviderDatabase( name, "HDB07 routed valid" )
		);
		BerkeleyDbTerminalDescriptionProvider direct = new( path );
		TerminalDatabase routed = new( [ direct ] );

		Assert.True(
			direct.TryLoad(
				name,
				out TerminalDescription? directTerminal
			)
		);
		Assert.True(
			routed.TryLoad(
				name,
				out TerminalDescription? routedTerminal
			)
		);
		Assert.Same( directTerminal, routedTerminal );
		Assert.Equal( "HDB07 routed valid", routedTerminal!.Description );
	}

	[Fact]
	public void RoutedMissingResultMatchesDirectProvider() {
		using TemporaryRoot temporary = new();
		string path = temporary.Write(
			"missing.db",
			CreateProviderDatabase(
				"hdb07-present",
				"HDB07 present"
			)
		);
		BerkeleyDbTerminalDescriptionProvider direct = new( path );
		TerminalDatabase routed = new( [ direct ] );

		Assert.False( direct.TryLoad( "hdb07-missing", out _ ) );
		Assert.False( routed.TryLoad( "hdb07-missing", out _ ) );
	}

	[Fact]
	public void RoutedMalformedFailureMatchesDirectProviderCategory() {
		using TemporaryRoot temporary = new();
		string name = "hdb07-malformed";
		string path = temporary.Write(
			"malformed.db",
			CreateCorruptProviderDatabase( name )
		);
		BerkeleyDbTerminalDescriptionProvider direct = new( path );

		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => direct.TryLoad( name, out _ )
		);
		TerminalDatabase routed = new( [ direct ] );
		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => routed.TryLoad( name, out _ )
		);
	}

	[Fact]
	public void MixedDirectoryRoutingPreservesProviderOrder() {
		using TemporaryRoot temporary = new();
		string name = "hdb07-mixed";
		string directoryRoot = temporary.CreateDirectory( "directory" );
		WriteDirectoryEntry(
			directoryRoot,
			name,
			"HDB07 directory"
		);
		string corruptPath = temporary.Write(
			"corrupt.db",
			CreateCorruptProviderDatabase( name )
		);
		DirectoryTerminalDescriptionProvider directory =
			new( directoryRoot );
		BerkeleyDbTerminalDescriptionProvider corrupt =
			new( corruptPath );
		Assert.True(
			directory.TryLoad(
				name,
				out TerminalDescription? directDirectory
			)
		);

		TerminalDatabase directoryFirst =
			new(
				new ITerminalDescriptionProvider[] {
					directory,
					corrupt,
				}
			);
		Assert.True(
			directoryFirst.TryLoad(
				name,
				out TerminalDescription? routedDirectory
			)
		);
		Assert.Same( directDirectory, routedDirectory );

		TerminalDatabase corruptFirst =
			new(
				new ITerminalDescriptionProvider[] {
					corrupt,
					directory,
				}
			);
		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => corruptFirst.TryLoad( name, out _ )
		);
	}

	[Fact]
	public void RuntimeRouterAddsNoBerkeleyDbSpecificPublicSurface() {
		Assert.DoesNotContain(
			typeof( TerminalDatabase ).Assembly.GetExportedTypes(),
			type => type.FullName?.Contains(
				"BerkeleyDb",
				StringComparison.Ordinal
			) == true
		);
		Assert.All(
			typeof( TerminalDatabase ).GetConstructors(),
			constructor => Assert.DoesNotContain(
				constructor.GetParameters(),
				parameter => parameter.ParameterType.FullName?.Contains(
					"BerkeleyDb",
					StringComparison.Ordinal
				) == true
			)
		);
	}

	private static byte[] CreateProviderDatabase(
		string name,
		string description
	) {
		byte[] payload = Hdb07HashV9FixtureBuilder.NcursesData(
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				name,
				description
			)
		);
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( Encoding.UTF8.GetBytes( name ) ),
				Hdb07ItemSpec.Inline( payload )
			)
		);
	}

	private static byte[] CreateCorruptProviderDatabase( string name ) {
		byte[] payload = Hdb07HashV9FixtureBuilder.NcursesData(
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				name,
				"HDB07 corrupt"
			)
		);
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( Encoding.UTF8.GetBytes( name ) ),
				Hdb07ItemSpec.OffPage(
					payload,
					[ payload.Length ],
					appendEmptyOverflowPage: true
				)
			)
		);
	}

	private static void WriteDirectoryEntry(
		string root,
		string name,
		string description
	) {
		string directory = Path.Combine(
			root,
			name[0].ToString()
		);
		Directory.CreateDirectory( directory );
		File.WriteAllBytes(
			Path.Combine( directory, name ),
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				name,
				description
			)
		);
	}

	private sealed class TemporaryRoot : IDisposable {
		internal TemporaryRoot() {
			Root = Path.Combine(
				Path.GetTempPath(),
				$"icod-terminfo-router-hdb07-{Guid.NewGuid():N}"
			);
			Directory.CreateDirectory( Root );
		}

		internal string Root { get; }

		internal string CreateDirectory( string name ) {
			string path = Path.Combine( Root, name );
			Directory.CreateDirectory( path );
			return path;
		}

		internal string Write( string name, byte[] data ) {
			string path = Path.Combine( Root, name );
			File.WriteAllBytes( path, data );
			return path;
		}

		public void Dispose() {
			Directory.Delete( Root, recursive: true );
		}
	}
}
