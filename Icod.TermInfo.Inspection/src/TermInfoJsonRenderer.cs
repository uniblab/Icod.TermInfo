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
	/// <exception cref="NotSupportedException">
	/// Comparison JSON rendering begins in MI03.
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
	/// <exception cref="NotSupportedException">
	/// Comparison JSON rendering begins in MI03.
	/// </exception>
	public static string Render(
		TermInfoComparisonResult comparison,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( comparison );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		throw new NotSupportedException(
			"Structured comparison JSON rendering begins in MI03."
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
	/// <exception cref="NotSupportedException">
	/// Planning-result JSON rendering begins in MI03.
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
	/// <exception cref="NotSupportedException">
	/// Planning-result JSON rendering begins in MI03.
	/// </exception>
	public static string Render(
		TerminalDescriptionSourcePlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		throw new NotSupportedException(
			"Relative-source planning-result JSON rendering begins in MI03."
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
	/// <exception cref="NotSupportedException">
	/// Database-catalog JSON rendering begins in MI04.
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
	/// <exception cref="NotSupportedException">
	/// Database-catalog JSON rendering begins in MI04.
	/// </exception>
	public static string Render(
		TermInfoDatabaseCatalog catalog,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( catalog );
		ArgumentNullException.ThrowIfNull( options );
		cancellationToken.ThrowIfCancellationRequested();

		throw new NotSupportedException(
			"Database-catalog JSON rendering begins in MI04."
		);
	}
}
