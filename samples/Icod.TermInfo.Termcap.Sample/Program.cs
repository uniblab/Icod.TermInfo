/*
	Icod.TermInfo.Termcap.Sample
	Demonstrates reusable Icod.TermInfo.Termcap parsing, resolution, conversion, rendering, and acquisition APIs.
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
using Icod.TermInfo.Termcap;

static void Require(
	bool condition,
	string message
) {
	ArgumentException.ThrowIfNullOrWhiteSpace( message );
	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}

const string source =
	"termcap-sample-base|Termcap sample base:am:co#80:li#24:cl=\\E[H\\E[2J:\n"
	+ "termcap-sample|Termcap sample child:co#132:cm=\\E[%i%d;%dH:tc=termcap-sample-base:\n";

TermcapSourceParseResult parsed = TermcapSourceParser.Parse(
	source,
	"termcap-sample.termcap"
);
Require(
	!parsed.HasErrors && parsed.Document.Entries.Count == 2,
	"Representative termcap source must parse without errors."
);

TermcapSourceField columnsField = parsed.Document.Entries[ 1 ].Fields.Single(
	field => field.CapabilityName == "co"
);
TermcapCapabilityClassificationResult classification =
	TermcapCapabilityClassifier.Classify( columnsField );
Require(
	classification.Mapping?.NumericCapability == NumericCapability.Columns,
	"The termcap co capability must classify as Runtime columns."
);

TermcapSourceResolveResult resolved = TermcapSourceResolver.Resolve(
	parsed.Document,
	"termcap-sample"
);
Require(
	!resolved.HasErrors && resolved.Entry is not null,
	"tc= inheritance must resolve without errors."
);

TermcapConversionResult converted = TermcapConverter.Convert( resolved.Entry! );
Require(
	!converted.HasErrors && converted.Description is not null,
	"Resolved termcap state must convert to Runtime."
);
TerminalDescription description = converted.Description!;
Require(
	description.GetBoolean( BooleanCapability.AutoRightMargin )
		&& description.GetNumber( NumericCapability.Columns ) == 132
		&& description.GetNumber( NumericCapability.Lines ) == 24
		&& description.GetString( StringCapability.ClearScreen ) is not null
		&& description.GetString( StringCapability.CursorAddress ) is not null,
	"Inherited and overridden termcap semantics must survive conversion."
);

TermcapRenderResult rendered = TermcapRenderer.Render(
	description,
	new TermcapRenderOptions( 72 )
);
Require(
	rendered.IsRepresentable
		&& !rendered.HasErrors
		&& !string.IsNullOrWhiteSpace( rendered.Text ),
	"The converted Runtime description must be representable as termcap."
);

TermcapAcquisitionResult acquired = TermcapAcquirer.Acquire(
	"termcap-sample",
	new TermcapAcquisitionOptions(
		inlineTermcap: source
	)
);
Require(
	acquired.IsSuccess
		&& acquired.Description?.GetBoolean( BooleanCapability.AutoRightMargin ) == true
		&& acquired.Description?.GetNumber( NumericCapability.Columns ) == 132
		&& acquired.Description?.GetNumber( NumericCapability.Lines ) == 24,
	"Explicit inline acquisition must reproduce the resolved Runtime semantics."
);

Console.WriteLine( $"Resolved terminal: {description.Name}" );
Console.WriteLine( $"Classified co as: {classification.Mapping!.TermInfoLongName}" );
Console.WriteLine( $"Columns: {description.GetNumber( NumericCapability.Columns )}" );
Console.WriteLine( $"Lines: {description.GetNumber( NumericCapability.Lines )}" );
Console.WriteLine( "Rendered termcap:" );
Console.WriteLine( rendered.Text );
Console.WriteLine( "Explicit inline acquisition: OK" );

return 0;
