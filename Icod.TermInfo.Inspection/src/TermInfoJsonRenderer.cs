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
/// Renders immutable Inspection values through the versioned deterministic JSON
/// contract.
/// </summary>
public static partial class TermInfoJsonRenderer {
	/// <summary>
	/// The exact schema identifier emitted by the version-1 JSON envelope.
	/// </summary>
	public const string SchemaIdentifier =
		"urn:icod:terminfo:inspection:json:1";

	/// <summary>
	/// The current machine-readable Inspection schema version.
	/// </summary>
	public const int SchemaVersion = 1;

	/// <summary>
	/// Renders an effective terminal description with the canonical compact JSON
	/// policy.
	/// </summary>
	/// <param name="description">The effective terminal description.</param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains text which JSON cannot represent, or the
	/// rendered UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TerminalDescription description
	) =>
		Render(
			description,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders an effective terminal description with explicit deterministic JSON
	/// policy.
	/// </summary>
	/// <param name="description">The effective terminal description.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is canceled.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The effective description contains text which JSON cannot represent, or the
	/// rendered UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TerminalDescription description,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderTerminalDescription(
			description,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders a structured terminfo comparison with the canonical compact JSON
	/// policy.
	/// </summary>
	/// <param name="comparison">The structured comparison result.</param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="comparison"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The comparison contains text which JSON cannot represent, or the rendered
	/// UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TermInfoComparisonResult comparison
	) =>
		Render(
			comparison,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders a structured terminfo comparison with explicit deterministic JSON
	/// policy.
	/// </summary>
	/// <param name="comparison">The structured comparison result.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="comparison"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is canceled.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The comparison contains text which JSON cannot represent, or the rendered
	/// UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TermInfoComparisonResult comparison,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( comparison );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderComparison(
			comparison,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders a relative-source planning result with the canonical compact JSON
	/// policy.
	/// </summary>
	/// <param name="plan">The immutable planning result and evidence.</param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="plan"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The plan contains text which JSON cannot represent, or the rendered UTF-8
	/// document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TerminalDescriptionSourcePlan plan
	) =>
		Render(
			plan,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders a relative-source planning result with explicit deterministic JSON
	/// policy.
	/// </summary>
	/// <param name="plan">The immutable planning result and evidence.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="plan"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is canceled.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The plan contains text which JSON cannot represent, or the rendered UTF-8
	/// document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TerminalDescriptionSourcePlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderSourcePlan(
			plan,
			options,
			cancellationToken
		);
	}

	/// <summary>
	/// Renders an explicit terminfo database catalog with the canonical compact
	/// JSON policy.
	/// </summary>
	/// <param name="catalog">The immutable explicit database catalog.</param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="catalog"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The catalog contains text which JSON cannot represent, or the rendered
	/// UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TermInfoDatabaseCatalog catalog
	) =>
		Render(
			catalog,
			new TermInfoJsonRendererOptions(),
			CancellationToken.None
		);

	/// <summary>
	/// Renders an explicit terminfo database catalog with explicit deterministic
	/// JSON policy.
	/// </summary>
	/// <param name="catalog">The immutable explicit database catalog.</param>
	/// <param name="options">The immutable JSON rendering policy.</param>
	/// <param name="cancellationToken">
	/// A token observed at deterministic rendering boundaries.
	/// </param>
	/// <returns>The deterministic JSON document.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="catalog"/> or <paramref name="options"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> is canceled.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The catalog contains text which JSON cannot represent, or the rendered
	/// UTF-8 document exceeds the configured output bound.
	/// </exception>
	public static string Render(
		TermInfoDatabaseCatalog catalog,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( catalog );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		return RenderDatabaseCatalog(
			catalog,
			options,
			cancellationToken
		);
	}
}
