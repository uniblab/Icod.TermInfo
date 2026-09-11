namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one bounded protocol-neutral persistent-raster lifecycle request.
/// </summary>
public sealed class PersistentRasterLifecycleRequest {
	/// <summary>
	/// Gets the largest supported placement count in one lifecycle request.
	/// </summary>
	public const int MaximumSupportedPlacementCount = 256;

	/// <summary>
	/// Initializes one lifecycle request.
	/// </summary>
	/// <param name="displayEphemeral">Whether to request one ephemeral raster display.</param>
	/// <param name="uploadResource">Whether to request persistent resource upload.</param>
	/// <param name="placementCount">The number of placements to create.</param>
	/// <param name="updatePlacement">Whether to request placement update.</param>
	/// <param name="deletePlacement">Whether to request targeted placement deletion.</param>
	/// <param name="deleteResource">Whether to request persistent resource deletion.</param>
	/// <param name="requireAcknowledgedUpload">
	/// Whether persistent upload must have acknowledged-upload support.
	/// </param>
	/// <exception cref="ArgumentException">
	/// The request is empty, mixes ephemeral display with persistent lifecycle intent,
	/// or requires upload acknowledgement without requesting upload.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="placementCount"/> is outside the supported range.
	/// </exception>
	public PersistentRasterLifecycleRequest(
		bool displayEphemeral = false,
		bool uploadResource = false,
		int placementCount = 0,
		bool updatePlacement = false,
		bool deletePlacement = false,
		bool deleteResource = false,
		bool requireAcknowledgedUpload = false
	) {
		if (
			placementCount < 0
			|| placementCount > MaximumSupportedPlacementCount
		) {
			throw new ArgumentOutOfRangeException(
				nameof( placementCount ),
				placementCount,
				$"The placement count must be between 0 and {MaximumSupportedPlacementCount}."
			);
		}

		bool hasPersistentIntent =
			uploadResource
			|| placementCount > 0
			|| updatePlacement
			|| deletePlacement
			|| deleteResource
			|| requireAcknowledgedUpload;
		if ( displayEphemeral && hasPersistentIntent ) {
			throw new ArgumentException(
				"Ephemeral display cannot be combined with persistent lifecycle intent in one request."
			);
		}
		if ( requireAcknowledgedUpload && !uploadResource ) {
			throw new ArgumentException(
				"Acknowledged upload can only be required when persistent upload is requested.",
				nameof( requireAcknowledgedUpload )
			);
		}
		if ( !displayEphemeral && !hasPersistentIntent ) {
			throw new ArgumentException(
				"A lifecycle request must contain at least one semantic outcome."
			);
		}

		DisplayEphemeral = displayEphemeral;
		UploadResource = uploadResource;
		PlacementCount = placementCount;
		UpdatePlacement = updatePlacement;
		DeletePlacement = deletePlacement;
		DeleteResource = deleteResource;
		RequireAcknowledgedUpload = requireAcknowledgedUpload;
	}

	/// <summary>Gets whether one ephemeral raster display is requested.</summary>
	public bool DisplayEphemeral {
		get;
	}

	/// <summary>Gets whether persistent resource upload is requested.</summary>
	public bool UploadResource {
		get;
	}

	/// <summary>Gets the number of placements requested for creation.</summary>
	public int PlacementCount {
		get;
	}

	/// <summary>Gets whether placement update is requested.</summary>
	public bool UpdatePlacement {
		get;
	}

	/// <summary>Gets whether targeted placement deletion is requested.</summary>
	public bool DeletePlacement {
		get;
	}

	/// <summary>Gets whether persistent resource deletion is requested.</summary>
	public bool DeleteResource {
		get;
	}

	/// <summary>Gets whether persistent upload must be acknowledged.</summary>
	public bool RequireAcknowledgedUpload {
		get;
	}
}
