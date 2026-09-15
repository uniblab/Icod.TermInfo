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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Exposes canonical metadata for the standard terminfo capability universe.
/// </summary>
public static class StandardCapabilityCatalog {
	private static readonly IReadOnlyList<StandardCapabilityMetadata<BooleanCapability>> BooleanCapabilityList =
		Array.AsReadOnly( StandardCapabilityDefinitions.Boolean );
	private static readonly IReadOnlyList<StandardCapabilityMetadata<NumericCapability>> NumericCapabilityList =
		Array.AsReadOnly( StandardCapabilityDefinitions.Numeric );
	private static readonly IReadOnlyList<StandardCapabilityMetadata<StringCapability>> StringCapabilityList =
		Array.AsReadOnly( StandardCapabilityDefinitions.String );

	private static readonly IReadOnlyDictionary<BooleanCapability, StandardCapabilityMetadata<BooleanCapability>> BooleanByCapability =
		CreateByCapability( BooleanCapabilityList );
	private static readonly IReadOnlyDictionary<NumericCapability, StandardCapabilityMetadata<NumericCapability>> NumericByCapability =
		CreateByCapability( NumericCapabilityList );
	private static readonly IReadOnlyDictionary<StringCapability, StandardCapabilityMetadata<StringCapability>> StringByCapability =
		CreateByCapability( StringCapabilityList );

	private static readonly IReadOnlyDictionary<string, StandardCapabilityMetadata<BooleanCapability>> BooleanByShortName =
		CreateByShortName( BooleanCapabilityList );
	private static readonly IReadOnlyDictionary<string, StandardCapabilityMetadata<NumericCapability>> NumericByShortName =
		CreateByShortName( NumericCapabilityList );
	private static readonly IReadOnlyDictionary<string, StandardCapabilityMetadata<StringCapability>> StringByShortName =
		CreateByShortName( StringCapabilityList );

	/// <summary>
	/// Gets the standard Boolean capabilities in compiled-table order.
	/// </summary>
	public static IReadOnlyList<StandardCapabilityMetadata<BooleanCapability>> BooleanCapabilities =>
		BooleanCapabilityList;

	/// <summary>
	/// Gets the standard numeric capabilities in compiled-table order.
	/// </summary>
	public static IReadOnlyList<StandardCapabilityMetadata<NumericCapability>> NumericCapabilities =>
		NumericCapabilityList;

	/// <summary>
	/// Gets the standard string capabilities in compiled-table order.
	/// </summary>
	public static IReadOnlyList<StandardCapabilityMetadata<StringCapability>> StringCapabilities =>
		StringCapabilityList;

	/// <summary>
	/// Gets metadata for a standard Boolean capability.
	/// </summary>
	public static StandardCapabilityMetadata<BooleanCapability> GetMetadata(
		BooleanCapability capability
	) {
		if (
			!BooleanByCapability.TryGetValue(
				capability,
				out StandardCapabilityMetadata<BooleanCapability>? metadata
			)
		) {
			throw new ArgumentOutOfRangeException( nameof( capability ) );
		}

		return metadata;
	}

	/// <summary>
	/// Gets metadata for a standard numeric capability.
	/// </summary>
	public static StandardCapabilityMetadata<NumericCapability> GetMetadata(
		NumericCapability capability
	) {
		if (
			!NumericByCapability.TryGetValue(
				capability,
				out StandardCapabilityMetadata<NumericCapability>? metadata
			)
		) {
			throw new ArgumentOutOfRangeException( nameof( capability ) );
		}

		return metadata;
	}

	/// <summary>
	/// Gets metadata for a standard string capability.
	/// </summary>
	public static StandardCapabilityMetadata<StringCapability> GetMetadata(
		StringCapability capability
	) {
		if (
			!StringByCapability.TryGetValue(
				capability,
				out StandardCapabilityMetadata<StringCapability>? metadata
			)
		) {
			throw new ArgumentOutOfRangeException( nameof( capability ) );
		}

		return metadata;
	}

	/// <summary>
	/// Looks up Boolean capability metadata by traditional terminfo short name.
	/// </summary>
	public static bool TryGetBoolean(
		string shortName,
		[NotNullWhen( true )] out StandardCapabilityMetadata<BooleanCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( shortName );
		return BooleanByShortName.TryGetValue( shortName, out metadata );
	}

	/// <summary>
	/// Looks up numeric capability metadata by traditional terminfo short name.
	/// </summary>
	public static bool TryGetNumeric(
		string shortName,
		[NotNullWhen( true )] out StandardCapabilityMetadata<NumericCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( shortName );
		return NumericByShortName.TryGetValue( shortName, out metadata );
	}

	/// <summary>
	/// Looks up string capability metadata by traditional terminfo short name.
	/// </summary>
	public static bool TryGetString(
		string shortName,
		[NotNullWhen( true )] out StandardCapabilityMetadata<StringCapability>? metadata
	) {
		ArgumentNullException.ThrowIfNull( shortName );
		return StringByShortName.TryGetValue( shortName, out metadata );
	}

	internal static bool IsStandardShortName( string shortName ) {
		ArgumentNullException.ThrowIfNull( shortName );

		return BooleanByShortName.ContainsKey( shortName )
			|| NumericByShortName.ContainsKey( shortName )
			|| StringByShortName.ContainsKey( shortName );
	}

	private static IReadOnlyDictionary<TCapability, StandardCapabilityMetadata<TCapability>> CreateByCapability<TCapability>(
		IEnumerable<StandardCapabilityMetadata<TCapability>> metadata
	) where TCapability : struct, Enum {
		return new ReadOnlyDictionary<TCapability, StandardCapabilityMetadata<TCapability>>(
			metadata.ToDictionary( item => item.Capability )
		);
	}

	private static IReadOnlyDictionary<string, StandardCapabilityMetadata<TCapability>> CreateByShortName<TCapability>(
		IEnumerable<StandardCapabilityMetadata<TCapability>> metadata
	) where TCapability : struct, Enum {
		return new ReadOnlyDictionary<string, StandardCapabilityMetadata<TCapability>>(
			metadata.ToDictionary(
				item => item.ShortName,
				StringComparer.Ordinal
			)
		);
	}
}
