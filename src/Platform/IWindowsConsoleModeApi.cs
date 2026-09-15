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

using System.Runtime.InteropServices;

namespace Icod.TermInfo;

internal interface IWindowsConsoleModeApi {
	bool TryGetStandardHandle(
		TerminalStandardStream stream,
		out nint handle
	);

	bool TryGetConsoleMode(
		nint handle,
		out uint mode
	);

	bool TrySetConsoleMode(
		nint handle,
		uint mode
	);
}

internal sealed class WindowsConsoleModeApi : IWindowsConsoleModeApi {
	private const int StandardOutputHandle = -11;
	private const int StandardErrorHandle = -12;

	internal static WindowsConsoleModeApi Instance { get; } = new();

	private WindowsConsoleModeApi() {
	}

	public bool TryGetStandardHandle(
		TerminalStandardStream stream,
		out nint handle
	) {
		if (
			!Enum.IsDefined(
				typeof( TerminalStandardStream ),
				stream
			)
		) {
			throw new ArgumentOutOfRangeException( nameof( stream ) );
		}

		if ( stream == TerminalStandardStream.Input ) {
			throw new ArgumentException(
				"Console output mode is available only for standard output or standard error.",
				nameof( stream )
			);
		}

		int standardHandle = stream switch {
			TerminalStandardStream.Output => StandardOutputHandle,
			TerminalStandardStream.Error => StandardErrorHandle,
			_ => throw new ArgumentOutOfRangeException( nameof( stream ) ),
		};

		handle = NativeMethods.GetStdHandle( standardHandle );

		return ( handle != IntPtr.Zero )
			&& ( handle != new IntPtr( -1 ) );
	}

	public bool TryGetConsoleMode(
		nint handle,
		out uint mode
	) {
		return NativeMethods.GetConsoleMode(
			handle,
			out mode
		);
	}

	public bool TrySetConsoleMode(
		nint handle,
		uint mode
	) {
		return NativeMethods.SetConsoleMode(
			handle,
			mode
		);
	}

#pragma warning disable SYSLIB1054 // Keep this small blittable interop surface free of generated unsafe code.
	private static class NativeMethods {
		[DllImport( "kernel32.dll", SetLastError = true )]
		internal static extern nint GetStdHandle( int standardHandle );

		[DllImport( "kernel32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		internal static extern bool GetConsoleMode(
			nint consoleHandle,
			out uint mode
		);

		[DllImport( "kernel32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		internal static extern bool SetConsoleMode(
			nint consoleHandle,
			uint mode
		);
	}
#pragma warning restore SYSLIB1054
}
