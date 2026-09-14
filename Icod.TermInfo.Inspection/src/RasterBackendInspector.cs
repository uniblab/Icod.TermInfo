namespace Icod.TermInfo.Inspection;

/// <summary>
/// Derives deterministic raster-backend availability evidence from explicit
/// terminal-description metadata without terminal-brand heuristics or live I/O.
/// </summary>
public static class RasterBackendInspector {
	/// <summary>
	/// The explicit extended Boolean capability advertising Sixel availability.
	/// </summary>
	public const string SixelCapabilityName = "Sixel";

	/// <summary>
	/// Inspects explicit availability metadata for one concrete raster backend.
	/// </summary>
	/// <param name="description">The immutable terminal description to inspect.</param>
	/// <param name="backend">The concrete backend being inspected.</param>
	/// <param name="options">Optional deterministic evidence resource bounds.</param>
	/// <returns>An immutable classified raster-backend profile.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="backend"/> is not a defined backend value.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The reserved Sixel availability capability exists with a non-Boolean value
	/// while Sixel is being inspected.
	/// </exception>
	public static RasterBackendProfile Inspect(
		TerminalDescription description,
		RasterBackendKind backend,
		RasterBackendEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( description );
		if ( !Enum.IsDefined( backend ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( backend ),
				backend,
				"The raster backend must be a defined value."
			);
		}

		List<RasterBackendEvidence> evidence = [];
		if ( backend == RasterBackendKind.Sixel ) {
			if (
				description.ExtendedCapabilities.TryGetValue(
					SixelCapabilityName,
					out TermInfoCapabilityValue value
				)
			) {
				if ( !value.IsBoolean ) {
					throw new InvalidOperationException(
						$"Extended capability '{SixelCapabilityName}' on terminal '{description.Name}' must be Boolean to declare Sixel backend availability."
					);
				}
				if ( value.BooleanValue ) {
					evidence.Add(
						new RasterBackendEvidence(
							RasterBackendKind.Sixel,
							true,
							RasterBackendEvidenceKind.CapabilityDerived,
							SixelCapabilityName,
							0
						)
					);
				}
			}
		}

		return RasterBackendClassifier.Classify(
			backend,
			evidence,
			options
		);
	}
}
