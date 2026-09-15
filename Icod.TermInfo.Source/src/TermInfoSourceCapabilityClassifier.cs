/*
	Icod.TermInfo.Source
	Parses, resolves, renders, and plans terminfo source descriptions.
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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;

namespace Icod.TermInfo.Source;

internal static class TermInfoSourceCapabilityClassifier {
	private static readonly IReadOnlyDictionary<
		string,
		StandardCapabilityMetadata<BooleanCapability>
	> BooleanByLongName =
		StandardCapabilityCatalog.BooleanCapabilities.ToDictionary(
			metadata => metadata.LongName,
			StringComparer.Ordinal
		);

	private static readonly IReadOnlyDictionary<
		string,
		StandardCapabilityMetadata<NumericCapability>
	> NumericByLongName =
		StandardCapabilityCatalog.NumericCapabilities.ToDictionary(
			metadata => metadata.LongName,
			StringComparer.Ordinal
		);

	private static readonly IReadOnlyDictionary<
		string,
		StandardCapabilityMetadata<StringCapability>
	> StringByLongName =
		StandardCapabilityCatalog.StringCapabilities.ToDictionary(
			metadata => metadata.LongName,
			StringComparer.Ordinal
		);

	private static readonly Lazy<HashSet<string>> KnownExtendedNames =
		new(
			CreateKnownExtendedNames,
			LazyThreadSafetyMode.ExecutionAndPublication
		);

	internal static TermInfoSourceCapabilityIdentity Classify( string name ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		if (
			TryGetStandardBoolean(
				name,
				out StandardCapabilityMetadata<BooleanCapability>? boolean
			)
		) {
			return TermInfoSourceCapabilityIdentity.StandardBoolean( boolean );
		}

		if (
			TryGetStandardNumeric(
				name,
				out StandardCapabilityMetadata<NumericCapability>? numeric
			)
		) {
			return TermInfoSourceCapabilityIdentity.StandardNumeric( numeric );
		}

		if (
			TryGetStandardString(
				name,
				out StandardCapabilityMetadata<StringCapability>? text
			)
		) {
			return TermInfoSourceCapabilityIdentity.StandardString( text );
		}

		if ( !IsValidExtendedName( name ) ) {
			return TermInfoSourceCapabilityIdentity.Invalid;
		}

		return ( KnownExtendedNames.Value.Contains( name ) )
			? TermInfoSourceCapabilityIdentity.KnownExtended( name )
			: TermInfoSourceCapabilityIdentity.UnknownExtended( name )
		;
	}

	private static bool TryGetStandardBoolean(
		string name,
		[NotNullWhen( true )] out StandardCapabilityMetadata<BooleanCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( name );

		return StandardCapabilityCatalog.TryGetBoolean(
				name,
				out metadata
			)
			|| BooleanByLongName.TryGetValue(
				name,
				out metadata
			);
	}

	private static bool TryGetStandardNumeric(
		string name,
		[NotNullWhen( true )] out StandardCapabilityMetadata<NumericCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( name );

		return StandardCapabilityCatalog.TryGetNumeric(
				name,
				out metadata
			)
			|| NumericByLongName.TryGetValue(
				name,
				out metadata
			);
	}

	private static bool TryGetStandardString(
		string name,
		[NotNullWhen( true )] out StandardCapabilityMetadata<StringCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( name );

		return StandardCapabilityCatalog.TryGetString(
				name,
				out metadata
			)
			|| StringByLongName.TryGetValue(
				name,
				out metadata
			);
	}

	private static bool IsValidExtendedName( string name ) {
		ArgumentNullException.ThrowIfNull( name );

		if (
			string.Equals(
				name,
				"use",
				StringComparison.Ordinal
			)
			|| ( name[0] == '.' )
		) {
			return false;
		}

		foreach ( char character in name ) {
			if (
				( character < '!' )
				|| ( character > '~' )
				|| ( character == ',' )
				|| ( character == '|' )
				|| ( character == '#' )
				|| ( character == '=' )
				|| ( character == '@' )
				|| ( character == '\\' )
			) {
				return false;
			}
		}

		return true;
	}

	private static HashSet<string> CreateKnownExtendedNames() {
		HashSet<string> names =
			new( StringComparer.Ordinal ) {
				// ncurses user_caps(5) capabilities with defined library or
				// application semantics in the selected compatibility family.
				"AX",
				"E3",
				"NQ",
				"RGB",
				"U8",
				"XM",
			};

		foreach (
			PropertyInfo property
			in typeof( TerminalProfiles )
				.GetProperties(
					BindingFlags.Public
					| BindingFlags.Static
				)
				.Where(
					property =>
						property.PropertyType
							== typeof( TerminalDescription )
				)
		) {
			TerminalDescription? terminal =
				property.GetValue( null )
					as TerminalDescription;
			if ( terminal is null ) {
				continue;
			}

			foreach ( string extendedName in terminal.ExtendedCapabilities.Keys ) {
				names.Add( extendedName );
			}
		}

		return names;
	}
}

internal readonly struct TermInfoSourceCapabilityIdentity {
	private TermInfoSourceCapabilityIdentity(
		TermInfoSourceCapabilityClassification classification,
		string? canonicalName,
		TermInfoCapabilityValueKind? standardValueKind,
		BooleanCapability? standardBooleanCapability,
		NumericCapability? standardNumericCapability,
		StringCapability? standardStringCapability
	) {
		Classification = classification;
		CanonicalName = canonicalName;
		StandardValueKind = standardValueKind;
		StandardBooleanCapability = standardBooleanCapability;
		StandardNumericCapability = standardNumericCapability;
		StandardStringCapability = standardStringCapability;
	}

	internal TermInfoSourceCapabilityClassification Classification { get; }

	internal string? CanonicalName { get; }

	internal TermInfoCapabilityValueKind? StandardValueKind { get; }

	internal BooleanCapability? StandardBooleanCapability { get; }

	internal NumericCapability? StandardNumericCapability { get; }

	internal StringCapability? StandardStringCapability { get; }

	internal static TermInfoSourceCapabilityIdentity Invalid { get; } =
		new(
			TermInfoSourceCapabilityClassification.Invalid,
			null,
			null,
			null,
			null,
			null
		);

	internal static TermInfoSourceCapabilityIdentity StandardBoolean(
		StandardCapabilityMetadata<BooleanCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermInfoSourceCapabilityIdentity(
			TermInfoSourceCapabilityClassification.Standard,
			metadata.ShortName,
			TermInfoCapabilityValueKind.Boolean,
			metadata.Capability,
			null,
			null
		);
	}

	internal static TermInfoSourceCapabilityIdentity StandardNumeric(
		StandardCapabilityMetadata<NumericCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermInfoSourceCapabilityIdentity(
			TermInfoSourceCapabilityClassification.Standard,
			metadata.ShortName,
			TermInfoCapabilityValueKind.Number,
			null,
			metadata.Capability,
			null
		);
	}

	internal static TermInfoSourceCapabilityIdentity StandardString(
		StandardCapabilityMetadata<StringCapability> metadata
	) {
		ArgumentNullException.ThrowIfNull( metadata );

		return new TermInfoSourceCapabilityIdentity(
			TermInfoSourceCapabilityClassification.Standard,
			metadata.ShortName,
			TermInfoCapabilityValueKind.String,
			null,
			null,
			metadata.Capability
		);
	}

	internal static TermInfoSourceCapabilityIdentity KnownExtended( string name ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TermInfoSourceCapabilityIdentity(
			TermInfoSourceCapabilityClassification.KnownExtended,
			name,
			null,
			null,
			null,
			null
		);
	}

	internal static TermInfoSourceCapabilityIdentity UnknownExtended( string name ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TermInfoSourceCapabilityIdentity(
			TermInfoSourceCapabilityClassification.UnknownExtended,
			name,
			null,
			null,
			null,
			null
		);
	}
}
