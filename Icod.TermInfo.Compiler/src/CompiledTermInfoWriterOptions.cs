namespace Icod.TermInfo.Compiler;

/// <summary>
/// Configures deterministic compiled-terminfo format selection.
/// </summary>
public sealed class CompiledTermInfoWriterOptions {
	/// <summary>
	/// Initializes the default automatic writer policy.
	/// </summary>
	public CompiledTermInfoWriterOptions()
		: this(
			CompiledTermInfoFormat.Automatic,
			includeExtendedCapabilities: true
		) {
	}

	/// <summary>
	/// Initializes explicit writer policy.
	/// </summary>
	/// <param name="format">
	/// The conventional numeric representation policy.
	/// </param>
	/// <param name="includeExtendedCapabilities">
	/// Whether the selected representation permits the ncurses extended section.
	/// When <see langword="false"/>, a description containing extended
	/// capabilities is rejected rather than silently truncated.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="format"/> is not a defined <see cref="CompiledTermInfoFormat"/>.
	/// </exception>
	public CompiledTermInfoWriterOptions(
		CompiledTermInfoFormat format,
		bool includeExtendedCapabilities = true
	) {
		if ( !Enum.IsDefined(
			typeof( CompiledTermInfoFormat ),
			format
		) ) {
			throw new ArgumentOutOfRangeException(
				nameof( format ),
				format,
				"The compiled terminfo format must be Automatic, Legacy, or Wide."
			);
		}

		Format = format;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
	}

	/// <summary>
	/// Gets the conventional numeric representation policy.
	/// </summary>
	public CompiledTermInfoFormat Format { get; }

	/// <summary>
	/// Gets whether the selected representation permits an ncurses extended
	/// section.
	/// </summary>
	public bool IncludeExtendedCapabilities { get; }
}