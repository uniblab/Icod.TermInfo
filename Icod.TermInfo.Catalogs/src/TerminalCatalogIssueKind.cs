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

/// <summary>Identifies a stable catalog diagnostic category.</summary>
public enum TerminalCatalogIssueKind {
	/// <summary>A directory candidate is not a valid compiled entry.</summary>
	MalformedEntry = 0,
	/// <summary>A parsed file does not occupy a supported publication location.</summary>
	InvalidPlacement = 1,
	/// <summary>Filesystem access was denied.</summary>
	PermissionFailure = 2,
	/// <summary>A filesystem operation failed.</summary>
	IoFailure = 3,
	/// <summary>A child link or reparse point was skipped.</summary>
	LinkSkipped = 4,
	/// <summary>More than one directory occurrence publishes this name.</summary>
	DuplicatePublication = 5,
	/// <summary>Hashed data is malformed or unsupported.</summary>
	InvalidHashedStore = 6,
	/// <summary>The source does not match the explicitly selected kind.</summary>
	UnsupportedSource = 7,
	/// <summary>The source was absent.</summary>
	MissingSource = 8,
}
