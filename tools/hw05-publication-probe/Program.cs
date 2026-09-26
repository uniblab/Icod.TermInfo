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


using Icod.TermInfo.BerkeleyDb;

if ( args.Length == 2 && args[0] == "hold-lock" ) {
	using var held = new FileStream( args[1], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None );
	Console.WriteLine( "acquired" );
	Console.Out.Flush();
	Console.ReadLine();
	return 0;
}
if ( args.Length == 6 && args[0] == "write" ) {
	Console.WriteLine( "ready" );
	Console.Out.Flush();
	BerkeleyDbTerminalDatabaseWriter.Write(
		args[1], [ new( args[3], [ args[4] ], File.ReadAllBytes( args[2] ) ) ],
		new( overwriteExisting: Boolean.Parse( args[5] ) )
	);
	Console.WriteLine( "published" );
	return 0;
}
Console.Error.WriteLine( "Expected hold-lock <lockPath> or write <destination> <payload> <canonical> <alias> <overwrite>." );
return 2;
