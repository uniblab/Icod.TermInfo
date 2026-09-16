/*
	toe
	Projects conventional and Hash-v9 catalogs into human listing entries.
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
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Toe;

internal static class ToeListingAdapter {
	internal static IReadOnlyList<ToeListingEntry> ReadHashStore(
		string path,
		bool sortByName,
		CancellationToken cancellationToken
	) {
		BerkeleyDbTerminalCatalogReader reader = new( path );
		IEnumerable<BerkeleyDbTerminalCatalogEntry> entries =
			reader.Read( cancellationToken );
		if ( sortByName ) {
			entries = entries.OrderBy(
				entry => entry.Name,
				StringComparer.Ordinal
			);
		}

		return entries
			.Select(
				entry => new ToeListingEntry(
					entry.Name,
					reader.DatabasePath,
					entry.Terminal
				)
			)
			.ToArray();
	}

	internal static IReadOnlyList<ToeListingEntry> ReadConventional(
		TermInfoDatabaseCatalog catalog,
		bool sortByName
	) {
		ArgumentNullException.ThrowIfNull( catalog );

		IEnumerable<TermInfoDatabaseCatalogEntry> entries = catalog.Entries;
		if ( sortByName ) {
			entries = entries
				.OrderBy(
					entry => entry.Name,
					StringComparer.Ordinal
				)
				.ThenBy(
					entry => entry.Path,
					StringComparer.Ordinal
				);
		}

		return entries
			.Select(
				entry => new ToeListingEntry(
					entry.Name,
					catalog.Root,
					entry.Terminal
				)
			)
			.ToArray();
	}
}

internal sealed record ToeListingEntry(
	string PublicationName,
	string Root,
	TerminalDescription Terminal
);
