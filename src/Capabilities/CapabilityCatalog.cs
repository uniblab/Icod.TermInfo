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

internal static class CapabilityCatalog {
	internal static bool IsStandardName( string name ) {
		ArgumentNullException.ThrowIfNull( name );
		return StandardCapabilityCatalog.IsStandardShortName( name );
	}

	internal static bool TryGetBoolean(
		string name,
		out BooleanCapability capability
	) {
		ArgumentNullException.ThrowIfNull( name );

		if (
			StandardCapabilityCatalog.TryGetBoolean(
				name,
				out StandardCapabilityMetadata<BooleanCapability>? metadata
			)
		) {
			capability = metadata.Capability;
			return true;
		}

		capability = default;
		return false;
	}

	internal static bool TryGetNumeric(
		string name,
		out NumericCapability capability
	) {
		ArgumentNullException.ThrowIfNull( name );

		if (
			StandardCapabilityCatalog.TryGetNumeric(
				name,
				out StandardCapabilityMetadata<NumericCapability>? metadata
			)
		) {
			capability = metadata.Capability;
			return true;
		}

		capability = default;
		return false;
	}

	internal static bool TryGetString(
		string name,
		out StringCapability capability
	) {
		ArgumentNullException.ThrowIfNull( name );

		if (
			StandardCapabilityCatalog.TryGetString(
				name,
				out StandardCapabilityMetadata<StringCapability>? metadata
			)
		) {
			capability = metadata.Capability;
			return true;
		}

		capability = default;
		return false;
	}
}
