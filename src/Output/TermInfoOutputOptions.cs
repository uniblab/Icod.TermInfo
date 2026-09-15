/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
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

namespace Icod.TermInfo;

/// <summary>
/// Supplies immutable terminal and transport facts for terminal-aware terminfo
/// output.
/// </summary>
/// <remarks>
/// The library never discovers a baud rate, owns an output descriptor, or
/// mutates terminal modes. <see cref="BaudRate"/> is supplied explicitly by the
/// caller when character padding or <c>pb</c> threshold evaluation needs it.
/// </remarks>
public sealed class TermInfoOutputOptions {
	/// <summary>
	/// Initializes terminal-aware output options.
	/// </summary>
	public TermInfoOutputOptions(
		TerminalDescription terminal,
		int? baudRate = null,
		PaddingMode paddingMode = PaddingMode.Delay,
		ITermInfoDelayProvider? delayProvider = null
	) {
		ArgumentNullException.ThrowIfNull( terminal );

		if ( baudRate.HasValue && baudRate.Value <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( baudRate ),
				"A supplied baud rate must be positive."
			);
		}

		if ( !Enum.IsDefined( typeof( PaddingMode ), paddingMode ) ) {
			throw new ArgumentOutOfRangeException( nameof( paddingMode ) );
		}

		Terminal = terminal;
		BaudRate = baudRate;
		PaddingMode = paddingMode;
		DelayProvider = delayProvider;
	}

	/// <summary>
	/// Gets the immutable terminal description whose padding capabilities are
	/// applied.
	/// </summary>
	public TerminalDescription Terminal { get; }

	/// <summary>
	/// Gets the caller-supplied output speed in bits per second, when known.
	/// </summary>
	public int? BaudRate { get; }

	/// <summary>
	/// Gets how padding directives are honored.
	/// </summary>
	public PaddingMode PaddingMode { get; }

	/// <summary>
	/// Gets the optional delay provider used for timed delays.
	/// </summary>
	public ITermInfoDelayProvider? DelayProvider { get; }
}
