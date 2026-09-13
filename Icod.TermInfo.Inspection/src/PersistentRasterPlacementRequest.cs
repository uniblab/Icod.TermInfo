namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one bounded protocol-neutral advanced persistent-raster placement
/// requirement request.
/// </summary>
public sealed class PersistentRasterPlacementRequest {
	/// <summary>
	/// Initializes one advanced persistent-raster placement requirement request.
	/// </summary>
	/// <param name="requireSourceRectangle">
	/// Whether pixel-space source-rectangle semantics are required.
	/// </param>
	/// <param name="requireSignedZOrder">
	/// Whether signed stacking-order semantics are required.
	/// </param>
	/// <exception cref="ArgumentException">
	/// Neither advanced placement semantic is required.
	/// </exception>
	public PersistentRasterPlacementRequest(
		bool requireSourceRectangle = false,
		bool requireSignedZOrder = false
	) {
		if ( !requireSourceRectangle && !requireSignedZOrder ) {
			throw new ArgumentException(
				"A placement request must require at least one advanced placement semantic."
			);
		}

		RequireSourceRectangle = requireSourceRectangle;
		RequireSignedZOrder = requireSignedZOrder;
	}

	/// <summary>
	/// Gets whether pixel-space source-rectangle semantics are required.
	/// </summary>
	public bool RequireSourceRectangle {
		get;
	}

	/// <summary>
	/// Gets whether signed stacking-order semantics are required.
	/// </summary>
	public bool RequireSignedZOrder {
		get;
	}
}
