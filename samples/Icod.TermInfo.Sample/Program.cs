/*
	Icod.TermInfo.Sample
	Demonstrates Icod.TermInfo APIs and integration patterns.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Icod.TermInfo;
using Icod.TermInfo.Sample;

bool describeOnly =
	args.Contains( "--describe-only", StringComparer.Ordinal );

TerminalDescription terminal =
	SampleTerminalResolver.Resolve( args );

Console.WriteLine( $"Profile: {terminal.Name}" );
Console.WriteLine( $"Description: {terminal.Description ?? "(none)"}" );

SampleDescription.DescribeSemanticCompletionApis( terminal );

if (
	SampleTerminalResolver.TryResolveSize(
		terminal,
		out TerminalSize size,
		out string source
	)
) {
	Console.WriteLine( $"Size ({source}): {size.Columns}x{size.Rows}" );
} else {
	Console.WriteLine( "Size: unknown" );
}

SampleDescription.DescribeProfile( terminal );

TerminalDatabase customDatabase =
	new(
		[
			new ExampleTerminalDescriptionProvider(),
		]
	);

Console.WriteLine(
	$"Custom provider example available: {customDatabase.TryLoad( "example-terminal", out _ )}"
);

if ( describeOnly ) {
	Console.WriteLine(
		"Describe-only mode: no terminal-control strings were emitted."
	);
	return;
}

if ( TerminalEnvironment.IsOutputRedirected ) {
	Console.WriteLine(
		"Output is redirected; terminal-control demonstration skipped."
	);
	return;
}

using IDisposable? windowsVt =
	WindowsVirtualTerminal.TryEnableOutput();

if ( OperatingSystem.IsWindows() && ( windowsVt is null ) ) {
	Console.WriteLine(
		"Windows virtual-terminal processing is unavailable; "
		+ "terminal-control demonstration skipped."
	);
	return;
}

SampleOutput.EmitDemonstration( terminal );
