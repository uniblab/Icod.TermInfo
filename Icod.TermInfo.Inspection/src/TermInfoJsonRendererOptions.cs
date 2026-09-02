namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic, bounded machine-readable JSON rendering of
/// Inspection values.
/// </summary>
public sealed class TermInfoJsonRendererOptions {
	/// <summary>
	/// The default maximum rendered JSON size in UTF-8 bytes.
	/// </summary>
	public const int DefaultMaximumOutputByteCount = 4_194_304;

	/// <summary>
	/// The largest supported caller-selected rendered JSON size in UTF-8 bytes.
	/// </summary>
	public const int MaximumSupportedOutputByteCount = 67_108_864;

	/// <summary>
	/// Initializes the canonical compact and bounded JSON policy.
	/// </summary>
	public TermInfoJsonRendererOptions()
		: this(
			DefaultMaximumOutputByteCount,
			writeIndented: false
		) {
	}

	/// <summary>
	/// Initializes an explicit deterministic and bounded JSON policy.
	/// </summary>
	/// <param name="maximumOutputByteCount">
	/// The maximum accepted rendered JSON size in UTF-8 bytes.
	/// </param>
	/// <param name="writeIndented">
	/// Whether the renderer uses its frozen indented presentation instead of the
	/// canonical compact presentation.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="maximumOutputByteCount"/> is not between one and
	/// <see cref="MaximumSupportedOutputByteCount"/>.
	/// </exception>
	public TermInfoJsonRendererOptions(
		int maximumOutputByteCount,
		bool writeIndented = false
	) {
		if ( maximumOutputByteCount < 1
			|| maximumOutputByteCount > MaximumSupportedOutputByteCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumOutputByteCount ),
				maximumOutputByteCount,
				$"The maximum JSON output size must be between 1 and {MaximumSupportedOutputByteCount} UTF-8 bytes."
			);
		}

		MaximumOutputByteCount = maximumOutputByteCount;
		WriteIndented = writeIndented;
	}

	/// <summary>
	/// Gets the maximum accepted rendered JSON size in UTF-8 bytes.
	/// </summary>
	public int MaximumOutputByteCount {
		get;
	}

	/// <summary>
	/// Gets whether the frozen indented JSON presentation is requested.
	/// </summary>
	public bool WriteIndented {
		get;
	}
}
