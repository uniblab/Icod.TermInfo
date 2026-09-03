using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	private const string ComparisonDocumentKind =
		"comparison";
	private const string SourcePlanDocumentKind =
		"sourcePlan";

	private static string RenderComparison(
		TermInfoComparisonResult comparison,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output =
			new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer =
			new(
				output,
				options.WriteIndented
			);

		try {
			writer.WriteStartObject();
			WriteEnvelopePrefix(
				writer,
				ComparisonDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteBoolean(
				"areEqual",
				comparison.AreEqual
			);
			writer.WriteStartArray( "differences" );
			foreach ( TermInfoDifference difference in comparison.Differences ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDifference(
					writer,
					difference,
					cancellationToken
				);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException(
				options,
				exception
			);
		}

		return output.GetString();
	}

	private static string RenderSourcePlan(
		TerminalDescriptionSourcePlan plan,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output =
			new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer =
			new(
				output,
				options.WriteIndented
			);

		try {
			writer.WriteStartObject();
			WriteEnvelopePrefix(
				writer,
				SourcePlanDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteNumber(
				"selectedParentCount",
				plan.SelectedParents.Count
			);
			writer.WriteStartArray( "selectedParentUseNames" );
			foreach (
				TerminalDescriptionSourceSynthesisParent parent
				in plan.SelectedParents
			) {
				cancellationToken.ThrowIfCancellationRequested();
				writer.WriteStringValue( parent.UseName );
			}
			writer.WriteEndArray();
			writer.WriteString(
				"source",
				plan.Source
			);
			WritePlanningScore(
				writer,
				plan.Score,
				cancellationToken
			);
			writer.WriteNumber(
				"evaluatedPlanCount",
				plan.EvaluatedPlanCount
			);
			writer.WriteBoolean(
				"isExhaustive",
				plan.IsExhaustive
			);
			writer.WriteNumber(
				"candidateCount",
				plan.CandidateCount
			);
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException(
				options,
				exception
			);
		}

		return output.GetString();
	}

	private static void WriteEnvelopePrefix(
		DeterministicJsonWriter writer,
		string documentKind
	) {
		writer.WriteString(
			"schema",
			SchemaIdentifier
		);
		writer.WriteNumber(
			"schemaVersion",
			SchemaVersion
		);
		writer.WriteString(
			"documentKind",
			documentKind
		);
	}

	private static InvalidOperationException CreateOutputLimitException(
		TermInfoJsonRendererOptions options,
		Exception innerException
	) =>
		new(
			$"The rendered JSON exceeds the configured {options.MaximumOutputByteCount} UTF-8 byte limit.",
			innerException
		);

	private static void WriteDifference(
		DeterministicJsonWriter writer,
		TermInfoDifference difference,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"kind",
			GetDifferenceKindName( difference.Kind )
		);
		writer.WriteString(
			"capabilityName",
			difference.CapabilityName
		);
		WriteNullableBoolean(
			writer,
			"isExtendedCapability",
			difference.IsExtendedCapability
		);
		WriteDifferenceSide(
			writer,
			"left",
			difference.LeftText,
			difference.LeftAliases,
			difference.LeftCapabilityValue,
			difference.LeftSourceEntry,
			difference.LeftSourceEntryIndex,
			difference.LeftSourceField,
			difference.LeftSourceFieldIndex,
			difference.LeftSourceSpan,
			cancellationToken
		);
		WriteDifferenceSide(
			writer,
			"right",
			difference.RightText,
			difference.RightAliases,
			difference.RightCapabilityValue,
			difference.RightSourceEntry,
			difference.RightSourceEntryIndex,
			difference.RightSourceField,
			difference.RightSourceFieldIndex,
			difference.RightSourceSpan,
			cancellationToken
		);
		writer.WriteEndObject();
	}

	private static void WriteDifferenceSide(
		DeterministicJsonWriter writer,
		string propertyName,
		string? text,
		IReadOnlyList<string>? aliases,
		TermInfoCapabilityValue? capabilityValue,
		TermInfoSourceEntry? sourceEntry,
		int? sourceEntryIndex,
		TermInfoSourceField? sourceField,
		int? sourceFieldIndex,
		TermInfoSourceSpan? sourceSpan,
		CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		writer.WriteStartObject( propertyName );
		writer.WriteString(
			"text",
			text
		);
		WriteNullableStringArray(
			writer,
			"aliases",
			aliases,
			cancellationToken
		);
		WriteCapabilityValue(
			writer,
			"capabilityValue",
			capabilityValue
		);
		WriteSourceEntry(
			writer,
			"sourceEntry",
			sourceEntry,
			cancellationToken
		);
		WriteNullableNumber(
			writer,
			"sourceEntryIndex",
			sourceEntryIndex
		);
		WriteSourceField(
			writer,
			"sourceField",
			sourceField
		);
		WriteNullableNumber(
			writer,
			"sourceFieldIndex",
			sourceFieldIndex
		);
		WriteSourceSpan(
			writer,
			"sourceSpan",
			sourceSpan
		);
		writer.WriteEndObject();
	}

	private static void WriteCapabilityValue(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoCapabilityValue? value
	) {
		if ( !value.HasValue ) {
			writer.WriteNull( propertyName );
			return;
		}

		TermInfoCapabilityValue actual = value.Value;
		writer.WriteStartObject( propertyName );
		writer.WriteString(
			"kind",
			GetCapabilityValueKindName( actual.Kind )
		);
		switch ( actual.Kind ) {
			case TermInfoCapabilityValueKind.Boolean:
				writer.WriteBoolean(
					"value",
					actual.BooleanValue
				);
				break;

			case TermInfoCapabilityValueKind.Number:
				writer.WriteNumber(
					"value",
					actual.NumberValue
				);
				break;

			case TermInfoCapabilityValueKind.String:
				writer.WriteString(
					"value",
					actual.StringValue
				);
				break;

			default:
				throw new InvalidOperationException(
					$"Unsupported capability value kind '{actual.Kind}'."
				);
		}
		writer.WriteEndObject();
	}

	private static void WriteSourceEntry(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoSourceEntry? entry,
		CancellationToken cancellationToken
	) {
		if ( entry is null ) {
			writer.WriteNull( propertyName );
			return;
		}

		writer.WriteStartObject( propertyName );
		writer.WriteString(
			"canonicalName",
			entry.CanonicalName
		);
		WriteStringArray(
			writer,
			"aliases",
			entry.Aliases,
			cancellationToken
		);
		writer.WriteString(
			"description",
			entry.Description
		);
		writer.WriteNumber(
			"fieldCount",
			entry.Fields.Count
		);
		WriteSourceSpan(
			writer,
			"span",
			entry.Span
		);
		writer.WriteEndObject();
	}

	private static void WriteSourceField(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoSourceField? field
	) {
		if ( field is null ) {
			writer.WriteNull( propertyName );
			return;
		}

		writer.WriteStartObject( propertyName );
		writer.WriteString(
			"kind",
			GetSourceFieldKindName( field.Kind )
		);
		writer.WriteString(
			"capabilityName",
			field.CapabilityName
		);
		writer.WriteString(
			"capabilityClassification",
			field.CapabilityClassification.HasValue
				? GetSourceCapabilityClassificationName(
					field.CapabilityClassification.Value
				)
				: null
		);
		writer.WriteString(
			"canonicalCapabilityName",
			field.CanonicalCapabilityName
		);
		writer.WriteString(
			"standardValueKind",
			field.StandardValueKind.HasValue
				? GetCapabilityValueKindName(
					field.StandardValueKind.Value
				)
				: null
		);
		writer.WriteString(
			"referenceName",
			field.ReferenceName
		);
		WriteNullableNumber(
			writer,
			"numericValue",
			field.NumericValue
		);
		writer.WriteString(
			"stringValue",
			field.StringValue
		);
		writer.WriteString(
			"text",
			field.Text
		);
		WriteSourceSpan(
			writer,
			"span",
			field.Span
		);
		writer.WriteEndObject();
	}

	private static void WriteSourceSpan(
		DeterministicJsonWriter writer,
		string propertyName,
		TermInfoSourceSpan? span
	) {
		if ( span is null ) {
			writer.WriteNull( propertyName );
			return;
		}

		writer.WriteStartObject( propertyName );
		writer.WriteString(
			"sourceName",
			span.SourceName
		);
		writer.WriteNumber(
			"offset",
			span.Offset
		);
		writer.WriteNumber(
			"line",
			span.Line
		);
		writer.WriteNumber(
			"column",
			span.Column
		);
		writer.WriteNumber(
			"length",
			span.Length
		);
		writer.WriteNumber(
			"endOffset",
			span.EndOffset
		);
		writer.WriteEndObject();
	}

	private static void WritePlanningScore(
		DeterministicJsonWriter writer,
		TerminalDescriptionSourcePlanningScore score,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObject( "score" );
		writer.WriteNumber(
			"localDirectiveCount",
			score.LocalDirectiveCount
		);
		writer.WriteNumber(
			"cancellationCount",
			score.CancellationCount
		);
		writer.WriteNumber(
			"parentCount",
			score.ParentCount
		);
		writer.WriteNumber(
			"renderedUtf8ByteCount",
			score.RenderedUtf8ByteCount
		);
		writer.WriteStartArray( "selectedCandidateIndices" );
		foreach ( int candidateIndex in score.SelectedCandidateIndices ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteNumberValue( candidateIndex );
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	private static void WriteNullableBoolean(
		DeterministicJsonWriter writer,
		string propertyName,
		bool? value
	) {
		if ( value.HasValue ) {
			writer.WriteBoolean(
				propertyName,
				value.Value
			);
			return;
		}

		writer.WriteNull( propertyName );
	}

	private static void WriteNullableNumber(
		DeterministicJsonWriter writer,
		string propertyName,
		int? value
	) {
		if ( value.HasValue ) {
			writer.WriteNumber(
				propertyName,
				value.Value
			);
			return;
		}

		writer.WriteNull( propertyName );
	}

	private static void WriteNullableStringArray(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<string>? values,
		CancellationToken cancellationToken
	) {
		if ( values is null ) {
			writer.WriteNull( propertyName );
			return;
		}

		WriteStringArray(
			writer,
			propertyName,
			values,
			cancellationToken
		);
	}

	private static void WriteStringArray(
		DeterministicJsonWriter writer,
		string propertyName,
		IReadOnlyList<string> values,
		CancellationToken cancellationToken
	) {
		writer.WriteStartArray( propertyName );
		foreach ( string value in values ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStringValue( value );
		}
		writer.WriteEndArray();
	}

	private static string GetDifferenceKindName(
		TermInfoDifferenceKind kind
	) =>
		kind switch {
			TermInfoDifferenceKind.IdentityName => "identityName",
			TermInfoDifferenceKind.IdentityAliases => "identityAliases",
			TermInfoDifferenceKind.IdentityDescription => "identityDescription",
			TermInfoDifferenceKind.OnlyInLeft => "onlyInLeft",
			TermInfoDifferenceKind.OnlyInRight => "onlyInRight",
			TermInfoDifferenceKind.DifferentValue => "differentValue",
			TermInfoDifferenceKind.DifferentValueKind => "differentValueKind",
			TermInfoDifferenceKind.SourceEntryOnlyInLeft => "sourceEntryOnlyInLeft",
			TermInfoDifferenceKind.SourceEntryOnlyInRight => "sourceEntryOnlyInRight",
			TermInfoDifferenceKind.SourceFieldOnlyInLeft => "sourceFieldOnlyInLeft",
			TermInfoDifferenceKind.SourceFieldOnlyInRight => "sourceFieldOnlyInRight",
			TermInfoDifferenceKind.SourceFieldKind => "sourceFieldKind",
			TermInfoDifferenceKind.SourceFieldCapability => "sourceFieldCapability",
			TermInfoDifferenceKind.SourceFieldValue => "sourceFieldValue",
			TermInfoDifferenceKind.SourceUseReference => "sourceUseReference",
			_ => throw new InvalidOperationException(
				$"Unsupported comparison difference kind '{kind}'."
			),
		};

	private static string GetCapabilityValueKindName(
		TermInfoCapabilityValueKind kind
	) =>
		kind switch {
			TermInfoCapabilityValueKind.Boolean => "boolean",
			TermInfoCapabilityValueKind.Number => "number",
			TermInfoCapabilityValueKind.String => "string",
			_ => throw new InvalidOperationException(
				$"Unsupported capability value kind '{kind}'."
			),
		};

	private static string GetSourceFieldKindName(
		TermInfoSourceFieldKind kind
	) =>
		kind switch {
			TermInfoSourceFieldKind.BooleanCapability => "booleanCapability",
			TermInfoSourceFieldKind.NumericCapability => "numericCapability",
			TermInfoSourceFieldKind.StringCapability => "stringCapability",
			TermInfoSourceFieldKind.CancelledCapability => "cancelledCapability",
			TermInfoSourceFieldKind.UseReference => "useReference",
			TermInfoSourceFieldKind.DisabledCapability => "disabledCapability",
			_ => throw new InvalidOperationException(
				$"Unsupported source field kind '{kind}'."
			),
		};

	private static string GetSourceCapabilityClassificationName(
		TermInfoSourceCapabilityClassification classification
	) =>
		classification switch {
			TermInfoSourceCapabilityClassification.Standard => "standard",
			TermInfoSourceCapabilityClassification.KnownExtended => "knownExtended",
			TermInfoSourceCapabilityClassification.UnknownExtended => "unknownExtended",
			TermInfoSourceCapabilityClassification.Invalid => "invalid",
			_ => throw new InvalidOperationException(
				$"Unsupported source capability classification '{classification}'."
			),
		};
}
