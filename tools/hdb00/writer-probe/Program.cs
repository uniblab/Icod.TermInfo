/*
	Icod.TermInfo.Hw00.ManagedWriterProbe
	Exercises the research-only managed Berkeley DB Hash-v9 writer.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using Icod.TermInfo.Hw00.ManagedWriterProbe;

if ( args.Length < 2 ) {
	Console.Error.WriteLine(
		"Usage: Hw00.ManagedWriterProbe OUTPUT COMPILED_ENTRY [...]"
	);
	return 64;
}

string outputPath = Path.GetFullPath( args[0] );
ReadOnlyMemory<byte>[] compiledEntries = args[1..]
	.Select( static path => (ReadOnlyMemory<byte>)File.ReadAllBytes( path ) )
	.ToArray()
;

await using FileStream output = new(
	outputPath,
	FileMode.Create,
	FileAccess.Write,
	FileShare.None
);
Hw00HashV9Writer.WriteNcursesCatalog( output, compiledEntries );
Console.WriteLine( $"HW00 managed candidate: {outputPath}" );
return 0;
