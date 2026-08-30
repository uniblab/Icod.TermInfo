namespace Icod.TermInfo.Termcap;

/// <summary>
/// Controls resource limits for termcap source parsing.
/// </summary>
public sealed class TermcapSourceParserOptions
{
	/// <summary>
	/// Gets the default maximum accepted source length in UTF-16 code units.
	/// </summary>
	public const int DefaultMaximumSourceLength = 4 * 1024 * 1024;

	/// <summary>
	/// Gets the largest maximum source length accepted by the parser.
	/// </summary>
	public const int MaximumSupportedSourceLength = 64 * 1024 * 1024;

	/// <summary>
	/// Initializes parser resource limits.
	/// </summary>
	/// <param name="maximumSourceLength">Maximum accepted source length in UTF-16 code units.</param>
	public TermcapSourceParserOptions(
		int maximumSourceLength = DefaultMaximumSourceLength
	) {
		if (
			maximumSourceLength < 1
			|| maximumSourceLength > MaximumSupportedSourceLength
		) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumSourceLength ),
				maximumSourceLength,
				$"The maximum source length must be between 1 and {MaximumSupportedSourceLength}."
			);
		}

		MaximumSourceLength = maximumSourceLength;
	}

	/// <summary>
	/// Gets the maximum accepted source length in UTF-16 code units.
	/// </summary>
	public int MaximumSourceLength { get; }
}
