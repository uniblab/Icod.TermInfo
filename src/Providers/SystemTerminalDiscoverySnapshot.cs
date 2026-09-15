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

internal enum TerminalHostPlatform {
	Windows,
	Linux,
	MacOS,
	Other,
}

internal sealed class SystemTerminalDiscoverySnapshot {
	internal SystemTerminalDiscoverySnapshot(
		string? termInfo,
		string? termInfoDirs,
		string? homeDirectory,
		string currentDirectory,
		TerminalHostPlatform platform
	) {
		ArgumentNullException.ThrowIfNull( currentDirectory );

		if ( string.IsNullOrWhiteSpace( currentDirectory ) ) {
			throw new ArgumentException(
				"The current directory snapshot cannot be empty or whitespace.",
				nameof( currentDirectory )
			);
		}

		if ( !Path.IsPathFullyQualified( currentDirectory ) ) {
			throw new ArgumentException(
				"The current directory snapshot must be fully qualified.",
				nameof( currentDirectory )
			);
		}

		TermInfo = termInfo;
		TermInfoDirs = termInfoDirs;
		HomeDirectory =
			( string.IsNullOrEmpty( homeDirectory ) )
				? null
				: homeDirectory
		;
		CurrentDirectory =
			Path.GetFullPath( currentDirectory );
		Platform = platform;
	}

	internal string? TermInfo { get; }

	internal string? TermInfoDirs { get; }

	internal string? HomeDirectory { get; }

	internal string CurrentDirectory { get; }

	internal TerminalHostPlatform Platform { get; }

	internal static SystemTerminalDiscoverySnapshot Capture(
		SystemTerminalDescriptionProviderOptions options
	) {
		ArgumentNullException.ThrowIfNull( options );

		return Capture(
			options,
			Environment.GetEnvironmentVariable,
			() => Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile
			),
			() => Environment.CurrentDirectory,
			DetectPlatform
		);
	}

	internal static SystemTerminalDiscoverySnapshot Capture(
		SystemTerminalDescriptionProviderOptions options,
		Func<string, string?> environmentReader,
		Func<string?> homeDirectoryReader,
		Func<string> currentDirectoryReader,
		Func<TerminalHostPlatform> platformReader
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( environmentReader );
		ArgumentNullException.ThrowIfNull( homeDirectoryReader );
		ArgumentNullException.ThrowIfNull( currentDirectoryReader );
		ArgumentNullException.ThrowIfNull( platformReader );

		string? termInfo = null;
		string? termInfoDirs = null;

		if ( options.UseEnvironment ) {
			termInfo =
				environmentReader( "TERMINFO" );
			termInfoDirs =
				environmentReader( "TERMINFO_DIRS" );
		}

		string? homeDirectory =
			( options.UseUserDatabase )
				? homeDirectoryReader()
				: null
		;

		return new SystemTerminalDiscoverySnapshot(
			termInfo,
			termInfoDirs,
			homeDirectory,
			currentDirectoryReader(),
			platformReader()
		);
	}

	private static TerminalHostPlatform DetectPlatform() {
		if ( OperatingSystem.IsWindows() ) {
			return TerminalHostPlatform.Windows;
		}

		if ( OperatingSystem.IsLinux() ) {
			return TerminalHostPlatform.Linux;
		}

		if ( OperatingSystem.IsMacOS() ) {
			return TerminalHostPlatform.MacOS;
		}

		return TerminalHostPlatform.Other;
	}
}
