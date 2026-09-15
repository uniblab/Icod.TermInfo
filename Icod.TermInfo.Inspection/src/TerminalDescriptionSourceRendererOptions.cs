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
/// Configures deterministic presentation of an effective terminal description
/// as terminfo source without changing its semantic content.
/// </summary>
public sealed class TerminalDescriptionSourceRendererOptions {
	/// <summary>
	/// Initializes the frozen canonical renderer policy.
	/// </summary>
	public TerminalDescriptionSourceRendererOptions()
		: this(
			80,
			TerminalDescriptionSourceLayout.Canonical,
			TerminalDescriptionSourceCapabilityOrder.Database,
			includeExtendedCapabilities: true
		) {
	}

	/// <summary>
	/// Initializes explicit effective-source presentation policy.
	/// </summary>
	/// <param name="lineWidth">
	/// Requested maximum physical line width for canonical string-capability
	/// wrapping. Unsplittable source tokens may exceed this value. Non-canonical
	/// layouts do not wrap.
	/// </param>
	/// <param name="layout">The physical source layout.</param>
	/// <param name="capabilityOrder">
	/// Ordering for standard capabilities within each value-kind group.
	/// </param>
	/// <param name="includeExtendedCapabilities">
	/// Whether effective extended capabilities are rendered.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="lineWidth"/> is not positive, <paramref name="layout"/> is
	/// not defined, or <paramref name="capabilityOrder"/> is not defined.
	/// </exception>
	public TerminalDescriptionSourceRendererOptions(
		int lineWidth,
		TerminalDescriptionSourceLayout layout =
			TerminalDescriptionSourceLayout.Canonical,
		TerminalDescriptionSourceCapabilityOrder capabilityOrder =
			TerminalDescriptionSourceCapabilityOrder.Database,
		bool includeExtendedCapabilities = true
	) {
		if ( lineWidth <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( lineWidth ),
				lineWidth,
				"The source-rendering line width must be positive."
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

		LineWidth = lineWidth;
		Layout = layout;
		CapabilityOrder = capabilityOrder;
		IncludeExtendedCapabilities = includeExtendedCapabilities;
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
	/// Gets whether effective extended capabilities are rendered.
	/// </summary>
	public bool IncludeExtendedCapabilities {
		get;
	}
}
