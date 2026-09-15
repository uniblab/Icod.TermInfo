/*
	Icod.TermInfo.Termcap.PackageSmoke
	Exercises packaged Icod.TermInfo APIs in an isolated consumer.
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

using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;

static void Require(
	bool condition,
	string message
) {
	ArgumentNullException.ThrowIfNull( message );
	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}

Assembly termcapAssembly =
	typeof( TermcapSourceParser ).Assembly;
Require(
	termcapAssembly.GetName().Name == "Icod.TermInfo.Termcap",
	"The Termcap package assembly could not be loaded."
);
Require(
	termcapAssembly.GetName().Version == new Version( 1, 0, 0, 0 ),
	"The Termcap package must retain the stable 1.x assembly identity."
);

const string source =
	"package-base|Package base terminal:am:co#80:li#24:cl=\\E[H:\n"
	+ "package-smoke|Package smoke terminal:co#132:cm=\\E[%i%d;%dH:tc=package-base:\n";
TermcapSourceParseResult parsed =
	TermcapSourceParser.Parse(
		source,
		"package-smoke.termcap"
	);
Require( !parsed.HasErrors, "The Termcap package could not parse representative source." );

TermcapSourceResolveResult resolved =
	TermcapSourceResolver.Resolve(
		parsed.Document,
		"package-smoke"
	);
Require( !resolved.HasErrors && resolved.Entry is not null, "The Termcap package could not resolve tc= inheritance." );

TermcapConversionResult converted =
	TermcapConverter.Convert( resolved.Entry! );
Require( !converted.HasErrors && converted.Description is not null, "The Termcap package could not convert to Runtime." );
TerminalDescription description = converted.Description!;
Require(
	description.GetBoolean( BooleanCapability.AutoRightMargin )
		&& description.GetNumber( NumericCapability.Columns ) == 132
		&& description.GetNumber( NumericCapability.Lines ) == 24,
	"Resolved Termcap package semantics were not preserved."
);

TermcapRenderResult rendered =
	TermcapRenderer.Render(
		description,
		new TermcapRenderOptions( 72 )
	);
Require(
	rendered.IsRepresentable
		&& !rendered.HasErrors
		&& !string.IsNullOrEmpty( rendered.Text ),
	"The Termcap package could not reverse-render a representative Runtime description."
);

TermcapAcquisitionResult acquired =
	TermcapAcquirer.Acquire(
		"package-smoke",
		new TermcapAcquisitionOptions(
			inlineTermcap: source
		)
	);
Require(
	acquired.IsSuccess
		&& acquired.Description?.GetNumber( NumericCapability.Columns ) == 132,
	"The package could not perform explicit inline termcap acquisition."
);

Assembly runtimeAssembly =
	typeof( TerminalDescription ).Assembly;
Require(
	runtimeAssembly.GetName().Name == "Icod.TermInfo"
		&& runtimeAssembly.GetName().Version == new Version( 1, 0, 0, 0 ),
	"The Runtime-only transitive dependency is unavailable or has the wrong identity."
);

Console.WriteLine( "Icod.TermInfo.Termcap package smoke test passed." );
