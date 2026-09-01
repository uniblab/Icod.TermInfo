namespace Icod.TermInfo.Inspection;

/// <summary>
/// Configures deterministic relative terminfo source synthesis.
/// </summary>
public sealed class TerminalDescriptionSourceSynthesisOptions {
	/// <summary>
	/// The default maximum number of explicitly supplied ordered parents.
	/// </summary>
	public const int DefaultMaximumParentCount = 64;

	/// <summary>
	/// The largest supported caller-selected parent-count limit.
	/// </summary>
	public const int MaximumSupportedParentCount = 256;

	/// <summary>
	/// Initializes the canonical relative-source synthesis policy.
	/// </summary>
	public TerminalDescriptionSourceSynthesisOptions()
		: this(
			80,
			TerminalDescriptionSourceLayout.Canonical,
			TerminalDescriptionSourceCapabilityOrder.Database,
			DefaultMaximumParentCount
		) {
	}

	/// <summary>
	/// Initializes explicit relative-source synthesis policy.
	/// </summary>
	/// <param name="lineWidth">
	/// Requested maximum physical line width for canonical source wrapping.
	/// Unsplittable source tokens may exceed this value. Non-canonical layouts do
	/// not wrap.
	/// </param>
	/// <param name="layout">The physical source layout.</param>
	/// <param name="capabilityOrder">
	/// Ordering for standard capabilities within each value-kind group.
	/// </param>
	/// <param name="maximumParentCount">
	/// The maximum number of ordered parents accepted for one synthesis request.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="lineWidth"/> is not positive,
	/// <paramref name="layout"/> or <paramref name="capabilityOrder"/> is not
	/// defined, or <paramref name="maximumParentCount"/> lies outside the
	/// supported range.
	/// </exception>
	public TerminalDescriptionSourceSynthesisOptions(
		int lineWidth,
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical,
		TerminalDescriptionSourceCapabilityOrder capabilityOrder =
			TerminalDescriptionSourceCapabilityOrder.Database,
		int maximumParentCount = DefaultMaximumParentCount
	) {
		if ( lineWidth <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( lineWidth ),
				lineWidth,
				"The source-synthesis line width must be positive."
			);
		}
		if ( !Enum.IsDefined(
			typeof( TerminalDescriptionSourceLayout ),
			layout
		) ) {
			throw new ArgumentOutOfRangeException(
				nameof( layout ),
				layout,
				"The source layout is not defined."
			);
		}
		if ( !Enum.IsDefined(
			typeof( TerminalDescriptionSourceCapabilityOrder ),
			capabilityOrder
		) ) {
			throw new ArgumentOutOfRangeException(
				nameof( capabilityOrder ),
				capabilityOrder,
				"The capability order is not defined."
			);
		}
		if ( maximumParentCount < 0
			|| maximumParentCount > MaximumSupportedParentCount ) {
			throw new ArgumentOutOfRangeException(
				nameof( maximumParentCount ),
				maximumParentCount,
				$"The maximum parent count must be between 0 and {MaximumSupportedParentCount}."
			);
		}

		LineWidth = lineWidth;
		Layout = layout;
		CapabilityOrder = capabilityOrder;
		MaximumParentCount = maximumParentCount;
	}

	/// <summary>
	/// Gets the requested canonical wrapping width.
	/// </summary>
	public int LineWidth {
		get;
	}

	/// <summary>
	/// Gets the physical source layout.
	/// </summary>
	public TerminalDescriptionSourceLayout Layout {
		get;
	}

	/// <summary>
	/// Gets the standard-capability ordering policy.
	/// </summary>
	public TerminalDescriptionSourceCapabilityOrder CapabilityOrder {
		get;
	}

	/// <summary>
	/// Gets the maximum accepted number of ordered parents.
	/// </summary>
	public int MaximumParentCount {
		get;
	}

	internal TerminalDescriptionSourceRendererOptions CreateRendererOptions() {
		return new TerminalDescriptionSourceRendererOptions(
			LineWidth,
			Layout,
			CapabilityOrder,
			includeExtendedCapabilities: true
		);
	}
}
