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
/// Provides the terminal profiles built into <c>Icod.TermInfo</c>.
/// </summary>
public static class TerminalProfiles {
	/// <summary>
	/// Gets the selected modern <c>xterm</c> core profile.
	/// </summary>
	public static TerminalDescription Xterm { get; } =
		XtermTerminalProfile.Create();

	/// <summary>
	/// Gets the modern <c>xterm-16color</c> indexed-color profile.
	/// </summary>
	public static TerminalDescription Xterm16Color { get; } =
		XtermIndexedTerminalProfile.Create16Color();

	/// <summary>
	/// Gets the modern <c>xterm-88color</c> indexed-color profile.
	/// </summary>
	public static TerminalDescription Xterm88Color { get; } =
		XtermIndexedTerminalProfile.Create88Color();

	/// <summary>
	/// Gets the modern <c>xterm-256color</c> indexed-color profile.
	/// </summary>
	public static TerminalDescription Xterm256Color { get; } =
		XtermIndexedTerminalProfile.Create256Color();

	/// <summary>
	/// Gets the modern <c>xterm-direct</c> true-color profile.
	/// </summary>
	public static TerminalDescription XtermDirect { get; } =
		XtermDirectTerminalProfile.Create();

	/// <summary>
	/// Gets the modern <c>xterm-direct16</c> true-color profile retaining 16
	/// indexed colors.
	/// </summary>
	public static TerminalDescription XtermDirect16 { get; } =
		XtermDirectTerminalProfile.Create16Color();

	/// <summary>
	/// Gets the modern <c>xterm-direct256</c> true-color profile retaining 256
	/// indexed colors.
	/// </summary>
	public static TerminalDescription XtermDirect256 { get; } =
		XtermDirectTerminalProfile.Create256Color();

	/// <summary>
	/// Gets the color-capable ANSI/PC-terminal profile.
	/// </summary>
	public static TerminalDescription Ansi { get; } =
		AnsiTerminalProfile.Create();

	/// <summary>
	/// Gets the authoritative ncurses <c>ms-terminal</c> Windows Terminal
	/// profile with 256 indexed colors.
	/// </summary>
	public static TerminalDescription MsTerminal { get; } =
		WindowsTerminalProfile.Create();

	/// <summary>
	/// Gets the authoritative ncurses <c>ms-terminal-direct</c> Windows
	/// Terminal profile with direct RGB color.
	/// </summary>
	public static TerminalDescription MsTerminalDirect { get; } =
		WindowsTerminalProfile.CreateDirect();

	/// <summary>
	/// Gets the authoritative ncurses <c>winconsole</c> profile for the
	/// Windows 10-and-later virtual-terminal console.
	/// </summary>
	public static TerminalDescription WinConsole { get; } =
		WindowsConsoleTerminalProfile.Create();

	/// <summary>
	/// Gets the DEC VT100 profile with the advanced-video option.
	/// </summary>
	public static TerminalDescription Vt100 { get; } =
		Vt100TerminalProfile.Create();

	/// <summary>
	/// Gets the canonical DEC VT102 profile.
	/// </summary>
	public static TerminalDescription Vt102 { get; } =
		Vt102TerminalProfile.Create();

	/// <summary>
	/// Gets the canonical seven-bit DEC VT220 profile.
	/// </summary>
	public static TerminalDescription Vt220 { get; } =
		Vt220TerminalProfile.Create();

	/// <summary>
	/// Gets the lowest-common-denominator <c>dumb</c> terminal profile.
	/// </summary>
	public static TerminalDescription Dumb { get; } =
		DumbTerminalProfile.Create();
}
