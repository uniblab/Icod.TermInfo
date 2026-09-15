/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one explicit provider/name pair to inspect.
/// </summary>
/// <remarks>
/// The optional display label is caller-owned diagnostic context. It does not
/// assert provider provenance and is never inferred from provider internals.
/// </remarks>
public sealed class TermInfoInspectionTarget {
	/// <summary>
	/// Initializes an explicit inspection target.
	/// </summary>
	/// <param name="provider">The provider used to acquire the terminal.</param>
	/// <param name="requestedName">The exact terminal name requested from the provider.</param>
	/// <param name="displayLabel">
	/// Optional caller-owned text used to identify the target in diagnostics.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="provider"/> or <paramref name="requestedName"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="requestedName"/> is empty or whitespace, or
	/// <paramref name="displayLabel"/> is supplied as empty or whitespace.
	/// </exception>
	public TermInfoInspectionTarget(
		ITerminalDescriptionProvider provider,
		string requestedName,
		string? displayLabel = null
	) {
		ArgumentNullException.ThrowIfNull( provider );
		ArgumentNullException.ThrowIfNull( requestedName );

		if ( string.IsNullOrWhiteSpace( requestedName ) ) {
			throw new ArgumentException(
				"The requested terminal name cannot be empty or whitespace.",
				nameof( requestedName )
			);
		}
		if ( displayLabel is not null
			&& string.IsNullOrWhiteSpace( displayLabel ) ) {
			throw new ArgumentException(
				"The display label cannot be empty or whitespace when supplied.",
				nameof( displayLabel )
			);
		}

		Provider = provider;
		RequestedName = requestedName;
		DisplayLabel = displayLabel;
	}

	/// <summary>
	/// Gets the explicit terminal-description provider.
	/// </summary>
	public ITerminalDescriptionProvider Provider { get; }

	/// <summary>
	/// Gets the exact name requested from <see cref="Provider"/>.
	/// </summary>
	public string RequestedName { get; }

	/// <summary>
	/// Gets the optional caller-owned display/source label.
	/// </summary>
	public string? DisplayLabel { get; }

	/// <summary>
	/// Gets the caller-owned display label when present, otherwise the requested
	/// terminal name.
	/// </summary>
	public string DisplayName =>
		DisplayLabel
		?? RequestedName;
}
