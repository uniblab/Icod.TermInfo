using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Icod.TermInfo.Inspection.Tests;

internal static class JsonSchemaFixtureValidator {
	internal static bool IsValid(
		JsonNode schemaDocument,
		JsonNode? instance,
		out string error
	) {
		ArgumentNullException.ThrowIfNull( schemaDocument );

		return Validate(
			schemaDocument,
			instance,
			schemaDocument,
			"$",
			out error
		);
	}

	private static bool Validate(
		JsonNode schema,
		JsonNode? instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if ( schema is not JsonObject schemaObject ) {
			error = $"{instancePath}: the test schema node is not an object.";
			return false;
		}

		if (
			schemaObject.TryGetPropertyValue(
				"$ref",
				out JsonNode? referenceNode
			)
		) {
			string reference =
				referenceNode!.GetValue<string>();
			JsonNode resolved =
				ResolveLocalReference(
					schemaDocument,
					reference
				);

			return Validate(
				resolved,
				instance,
				schemaDocument,
				instancePath,
				out error
			);
		}

		if (
			!ValidateAllOf(
				schemaObject,
				instance,
				schemaDocument,
				instancePath,
				out error
			)
			|| !ValidateAnyOf(
				schemaObject,
				instance,
				schemaDocument,
				instancePath,
				out error
			)
			|| !ValidateOneOf(
				schemaObject,
				instance,
				schemaDocument,
				instancePath,
				out error
			)
		) {
			return false;
		}

		if (
			schemaObject.TryGetPropertyValue(
				"const",
				out JsonNode? constant
			)
			&& !JsonNode.DeepEquals(
				constant,
				instance
			)
		) {
			error = $"{instancePath}: value does not match const.";
			return false;
		}

		if (
			schemaObject.TryGetPropertyValue(
				"enum",
				out JsonNode? enumNode
			)
			&& !enumNode!
				.AsArray()
				.Any(
					value => JsonNode.DeepEquals(
						value,
						instance
					)
				)
		) {
			error = $"{instancePath}: value is not in the enum.";
			return false;
		}

		if (
			schemaObject.TryGetPropertyValue(
				"type",
				out JsonNode? typeNode
			)
			&& !MatchesType(
				instance,
				typeNode!.GetValue<string>()
			)
		) {
			error =
				$"{instancePath}: value does not have JSON type '{typeNode!.GetValue<string>()}'.";
			return false;
		}

		if (
			instance is JsonObject instanceObject
			&& !ValidateObject(
				schemaObject,
				instanceObject,
				schemaDocument,
				instancePath,
				out error
			)
		) {
			return false;
		}

		if (
			instance is JsonArray instanceArray
			&& !ValidateArray(
				schemaObject,
				instanceArray,
				schemaDocument,
				instancePath,
				out error
			)
		) {
			return false;
		}

		if (
			instance?.GetValueKind() == JsonValueKind.String
			&& schemaObject.TryGetPropertyValue(
				"minLength",
				out JsonNode? minimumLengthNode
			)
			&& instance.GetValue<string>().Length
				< minimumLengthNode!.GetValue<int>()
		) {
			error = $"{instancePath}: string is shorter than minLength.";
			return false;
		}

		if (
			instance?.GetValueKind() == JsonValueKind.Number
			&& schemaObject.TryGetPropertyValue(
				"minimum",
				out JsonNode? minimumNode
			)
			&& GetDecimal( instance ) < GetDecimal( minimumNode )
		) {
			error = $"{instancePath}: number is less than minimum.";
			return false;
		}

		error = string.Empty;
		return true;
	}

	private static bool ValidateAllOf(
		JsonObject schema,
		JsonNode? instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if (
			!schema.TryGetPropertyValue(
				"allOf",
				out JsonNode? allOfNode
			)
		) {
			error = string.Empty;
			return true;
		}

		foreach ( JsonNode? branch in allOfNode!.AsArray() ) {
			if ( !Validate(
				branch!,
				instance,
				schemaDocument,
				instancePath,
				out error
			) ) {
				return false;
			}
		}

		error = string.Empty;
		return true;
	}

	private static bool ValidateAnyOf(
		JsonObject schema,
		JsonNode? instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if (
			!schema.TryGetPropertyValue(
				"anyOf",
				out JsonNode? anyOfNode
			)
		) {
			error = string.Empty;
			return true;
		}

		foreach ( JsonNode? branch in anyOfNode!.AsArray() ) {
			if ( Validate(
				branch!,
				instance,
				schemaDocument,
				instancePath,
				out _
			) ) {
				error = string.Empty;
				return true;
			}
		}

		error = $"{instancePath}: no anyOf branch matched.";
		return false;
	}

	private static bool ValidateOneOf(
		JsonObject schema,
		JsonNode? instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if (
			!schema.TryGetPropertyValue(
				"oneOf",
				out JsonNode? oneOfNode
			)
		) {
			error = string.Empty;
			return true;
		}

		int matchCount = 0;
		foreach ( JsonNode? branch in oneOfNode!.AsArray() ) {
			if ( Validate(
				branch!,
				instance,
				schemaDocument,
				instancePath,
				out _
			) ) {
				matchCount++;
			}
		}

		if ( matchCount != 1 ) {
			error =
				$"{instancePath}: expected exactly one oneOf match, found {matchCount}.";
			return false;
		}

		error = string.Empty;
		return true;
	}

	private static bool ValidateObject(
		JsonObject schema,
		JsonObject instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if (
			schema.TryGetPropertyValue(
				"required",
				out JsonNode? requiredNode
		) ) {
			foreach ( JsonNode? nameNode in requiredNode!.AsArray() ) {
				string name = nameNode!.GetValue<string>();
				if ( !instance.ContainsKey( name ) ) {
					error = $"{instancePath}: required property '{name}' is absent.";
					return false;
				}
			}
		}

		JsonObject? properties =
			schema.TryGetPropertyValue(
				"properties",
				out JsonNode? propertiesNode
			)
				? propertiesNode!.AsObject()
				: null;
		if ( properties is not null ) {
			foreach (
				KeyValuePair<string, JsonNode?> propertySchema
				in properties
			) {
				if ( instance.TryGetPropertyValue(
					propertySchema.Key,
					out JsonNode? propertyValue
				) && !Validate(
					propertySchema.Value!,
					propertyValue,
					schemaDocument,
					$"{instancePath}.{propertySchema.Key}",
					out error
				) ) {
					return false;
				}
			}
		}

		if (
			schema.TryGetPropertyValue(
				"additionalProperties",
				out JsonNode? additionalPropertiesNode
			)
			&& additionalPropertiesNode!.GetValueKind()
				== JsonValueKind.False
		) {
			foreach (
				KeyValuePair<string, JsonNode?> property
				in instance
			) {
				if ( properties is null
					|| !properties.ContainsKey( property.Key ) ) {
					error =
						$"{instancePath}: property '{property.Key}' is not allowed.";
					return false;
				}
			}
		}

		error = string.Empty;
		return true;
	}

	private static bool ValidateArray(
		JsonObject schema,
		JsonArray instance,
		JsonNode schemaDocument,
		string instancePath,
		out string error
	) {
		if (
			schema.TryGetPropertyValue(
				"minItems",
				out JsonNode? minimumItemsNode
			)
			&& instance.Count < minimumItemsNode!.GetValue<int>()
		) {
			error = $"{instancePath}: array contains fewer than minItems.";
			return false;
		}

		if (
			schema.TryGetPropertyValue(
				"maxItems",
				out JsonNode? maximumItemsNode
			)
			&& instance.Count > maximumItemsNode!.GetValue<int>()
		) {
			error = $"{instancePath}: array contains more than maxItems.";
			return false;
		}

		if (
			schema.TryGetPropertyValue(
				"items",
				out JsonNode? itemSchema
		) ) {
			for ( int index = 0; index < instance.Count; index++ ) {
				if ( !Validate(
					itemSchema!,
					instance[ index ],
					schemaDocument,
					$"{instancePath}[{index}]",
					out error
				) ) {
					return false;
				}
			}
		}

		error = string.Empty;
		return true;
	}

	private static bool MatchesType(
		JsonNode? instance,
		string type
	) {
		JsonValueKind kind =
			instance?.GetValueKind()
			?? JsonValueKind.Null;

		return type switch {
			"null" => kind == JsonValueKind.Null,
			"object" => kind == JsonValueKind.Object,
			"array" => kind == JsonValueKind.Array,
			"string" => kind == JsonValueKind.String,
			"boolean" => kind is JsonValueKind.True or JsonValueKind.False,
			"number" => kind == JsonValueKind.Number,
			"integer" => kind == JsonValueKind.Number
				&& decimal.Truncate(
					GetDecimal( instance )
				) == GetDecimal( instance ),
			_ => throw new InvalidOperationException(
				$"The fixture validator does not support JSON Schema type '{type}'."
			),
		};
	}

	private static decimal GetDecimal(
		JsonNode? node
	) {
		ArgumentNullException.ThrowIfNull( node );

		return decimal.Parse(
			node.ToJsonString(),
			NumberStyles.Float,
			CultureInfo.InvariantCulture
		);
	}

	private static JsonNode ResolveLocalReference(
		JsonNode schemaDocument,
		string reference
	) {
		ArgumentNullException.ThrowIfNull( schemaDocument );
		ArgumentException.ThrowIfNullOrWhiteSpace( reference );

		if ( !reference.StartsWith( "#/", StringComparison.Ordinal ) ) {
			throw new InvalidOperationException(
				$"Only local JSON Schema references are supported; found '{reference}'."
			);
		}

		JsonNode? current = schemaDocument;
		foreach (
			string encodedSegment
			in reference[ 2.. ].Split( '/' )
		) {
			string segment =
				encodedSegment
					.Replace( "~1", "/", StringComparison.Ordinal )
					.Replace( "~0", "~", StringComparison.Ordinal );
			if ( current is not JsonObject currentObject
				|| !currentObject.TryGetPropertyValue(
					segment,
					out current
				)
				|| current is null ) {
				throw new InvalidOperationException(
					$"Unable to resolve local JSON Schema reference '{reference}'."
				);
			}
		}

		return current;
	}
}
