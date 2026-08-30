namespace Icod.TermInfo.Termcap;

/// <summary>
/// Controls deterministic physical-line wrapping for rendered termcap source.
/// </summary>
public sealed class TermcapRenderOptions
{
	/// <summary>Gets the default maximum preferred physical-line length.</summary>
	public const int DefaultMaximumLineLength = 80;

	/// <summary>Gets the largest accepted preferred physical-line length.</summary>
	public const int MaximumSupportedLineLength = 4096;

	/// <summary>
	/// Initializes termcap rendering options.
	/// </summary>
	/// <param name="maximumLineLength">
	/// Preferred maximum physical-line length. Individual headers or fields which
	/// cannot be split safely may exceed this value.
	/// </param>
	public TermcapRenderOptions(
		int maximumLineLength = DefaultMaximumLineLength
	) {
		if (
			maximumLineLength < 16
			|| maximumLineLength > MaximumSupportedLineLength
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumLineLength ),
				maximumLineLength,
				$"The maximum line length must be between 16 and {MaximumSupportedLineLength}."
			);
		}

		MaximumLineLength = maximumLineLength;
	}

	/// <summary>Gets the preferred maximum physical-line length.</summary>
	public int MaximumLineLength { get; }
}
