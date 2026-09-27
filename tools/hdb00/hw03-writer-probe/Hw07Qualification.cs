using System.Globalization;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Tests.Shared;

internal static class Hw07Qualification {
	internal static void Generate( string root, string nativeDirectory ) {
		Directory.CreateDirectory( root );
		foreach ( int seed in Hw07WriterMatrix.Seeds ) {
			byte[] image = BerkeleyDbHashV9ImageBuilder.Build( Hw07WriterMatrix.CreateRecords( seed ), 2 * 1024 * 1024, CancellationToken.None );
			File.WriteAllBytes( Path.Combine( root, "hw07-matrix-" + seed.ToString( CultureInfo.InvariantCulture ) + ".db" ), image );
		}
		var entries = new Dictionary<string, BerkeleyDbTerminalDatabaseEntry>( StringComparer.Ordinal );
		foreach ( string path in Directory.EnumerateFiles( nativeDirectory, "*", SearchOption.AllDirectories ).OrderBy( path => path, StringComparer.Ordinal ) ) {
			byte[] data = File.ReadAllBytes( path );
			TerminalDescription description = CompiledTermInfoParser.Parse( data );
			if ( entries.TryGetValue( description.Name, out var existing ) ) {
				if ( !existing.Data.SequenceEqual( data ) ) {
					throw new InvalidDataException( "Native aliases disagree on compiled bytes." );
				}
			} else {
				entries.Add( description.Name, new( description.Name, description.Aliases, data ) );
			}
		}
		if ( entries.Count != 8 ) {
			throw new InvalidDataException( $"Expected eight representative native terminals; found {entries.Count}." );
		}
		WriteCatalog( root, "hw07-native", entries.Values.OrderBy( entry => entry.CanonicalName, StringComparer.Ordinal ).ToArray() );
		WriteCatalog( root, "hw07-utf8", Hw07WriterMatrix.CreateUtf8Entries() );
	}

	private static void WriteCatalog( string root, string stem, BerkeleyDbTerminalDatabaseEntry[] entries ) {
		BerkeleyDbTerminalDatabaseWriter.Write( Path.Combine( root, stem + ".db" ), entries );
		List<string> manifest = [];
		for ( int index = 0; index < entries.Length; index++ ) {
			var entry = entries[index];
			string payload = stem + "-" + index.ToString( CultureInfo.InvariantCulture ) + ".bin";
			File.WriteAllBytes( Path.Combine( root, payload ), entry.Data );
			foreach ( string name in entry.Aliases.Prepend( entry.CanonicalName ) ) {
				manifest.Add( name + "\t" + payload );
			}
		}
		File.WriteAllLines( Path.Combine( root, stem + ".names" ), manifest, new UTF8Encoding( false ) );
		Console.WriteLine( $"{stem}: {entries.Length} canonical entries; {manifest.Count} logical keys." );
	}

	internal static void Verify( string root ) {
		foreach ( int seed in Hw07WriterMatrix.Seeds ) {
			string stem = "hw07-matrix-" + seed.ToString( CultureInfo.InvariantCulture );
			string[] expected = Hw07WriterMatrix.CreateRecords( seed )
				.Select( record => Convert.ToHexString( record.Key.Span ) + ":" + Convert.ToHexString( record.Value.Span ) )
				.OrderBy( record => record, StringComparer.Ordinal ).ToArray();
			foreach ( string suffix in new[] { ".dump", "-repacked.dump" } ) {
				string[] lines = File.ReadAllLines( Path.Combine( root, stem + suffix ) );
				int header = Array.IndexOf( lines, "HEADER=END" );
				if ( header < 0 || lines[^1] != "DATA=END" || ( lines.Length - header - 2 ) % 2 != 0 ) {
					throw new InvalidDataException( "Invalid native dump envelope." );
				}
				List<string> actual = [];
				for ( int index = header + 1; index < lines.Length - 1; index += 2 ) {
					actual.Add( lines[index].Trim().ToUpperInvariant() + ":" + lines[index + 1].Trim().ToUpperInvariant() );
				}
				if ( !expected.SequenceEqual( actual.OrderBy( record => record, StringComparer.Ordinal ) ) ) {
					throw new InvalidDataException( $"Native records differ from generated input: {stem}{suffix}." );
				}
			}
		}
		Console.WriteLine( "HW07 native dump/reload: all 384 generated records preserved exactly." );
	}
}
