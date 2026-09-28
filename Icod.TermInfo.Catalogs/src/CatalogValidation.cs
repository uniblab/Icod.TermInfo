/*
	Icod.TermInfo.Catalogs
	Provides unified read-only directory and hashed terminfo catalogs.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo.Catalogs;

internal static class CatalogValidation {
	internal static void AbsolutePath( string path, string parameterName ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path, parameterName );
		if ( !System.IO.Path.IsPathFullyQualified( path ) ) {
			throw new ArgumentException( "The path must be fully qualified.", parameterName );
		}
	}
	internal static void Defined<T>( T value, string parameterName ) where T : struct, Enum {
		if ( !Enum.IsDefined( value ) ) {
			throw new ArgumentOutOfRangeException( parameterName );
		}
	}
}
