using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Inspection;

public static partial class TerminalDescriptionSourceRenderer {
	internal static string RenderRelative(
		TerminalDescriptionSourceSynthesisPlan plan
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ValidateIdentity( plan.Target );

		TerminalDescriptionSourceRendererOptions options =
			plan.Options.CreateRendererOptions();
		ParentAggregate inherited =
			CreateParentAggregate(
				plan.Parents
			);
		StringBuilder builder =
			new();
		AppendConfiguredHeader(
			builder,
			plan.Target,
			options.Layout
		);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.BooleanCapabilities,
				options.CapabilityOrder
			)
		) {
			bool targetValue =
				plan.Target.GetBoolean( metadata.Capability );
			bool inheritedValue =
				inherited.BooleanCapabilities.Contains(
					metadata.Capability
				);

			if ( targetValue == inheritedValue ) {
				continue;
			}

			if ( targetValue ) {
				AppendConfiguredBooleanField(
					builder,
					metadata.ShortName,
					options
				);
			} else {
				AppendConfiguredCancellationField(
					builder,
					metadata.ShortName,
					options
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.NumericCapabilities,
				options.CapabilityOrder
			)
		) {
			int? targetValue =
				plan.Target.GetNumber( metadata.Capability );
			int? inheritedValue =
				inherited.NumericCapabilities.TryGetValue(
					metadata.Capability,
					out int inheritedNumber
				)
					? inheritedNumber
					: null;

			if ( targetValue == inheritedValue ) {
				continue;
			}

			if ( targetValue.HasValue ) {
				AppendConfiguredNumericField(
					builder,
					metadata.ShortName,
					targetValue.Value,
					options
				);
			} else {
				AppendConfiguredCancellationField(
					builder,
					metadata.ShortName,
					options
				);
			}
		}

		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in OrderStandardCapabilities(
				StandardCapabilityCatalog.StringCapabilities,
				options.CapabilityOrder
			)
		) {
			string? targetValue =
				plan.Target.GetString( metadata.Capability );
			string? inheritedValue =
				inherited.StringCapabilities.TryGetValue(
					metadata.Capability,
					out string? inheritedString
				)
					? inheritedString
					: null;

			if (
				string.Equals(
					targetValue,
					inheritedValue,
					StringComparison.Ordinal
				)
			) {
				continue;
			}

			if ( targetValue is not null ) {
				AppendConfiguredStringField(
					builder,
					metadata.ShortName,
					targetValue,
					options
				);
			} else {
				AppendConfiguredCancellationField(
					builder,
					metadata.ShortName,
					options
				);
			}
		}

		AppendRelativeExtendedCapabilities(
			builder,
			plan.Target,
			inherited.ExtendedCapabilities,
			options,
			plan.Options.IncludeExtendedCapabilities
		);

		// RS04: emit the materialized caller order exactly; do not canonicalize,
		// deduplicate, prune, or reorder parent references.
		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in plan.Parents
		) {
			AppendConfiguredUseReference(
				builder,
				parent.UseName,
				options
			);
		}

		if ( options.Layout == TerminalDescriptionSourceLayout.SingleLine ) {
			builder.Append( '\n' );
		}

		return builder.ToString();
	}

	private static ParentAggregate CreateParentAggregate(
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents
	) {
		ArgumentNullException.ThrowIfNull( parents );

		ParentAggregate aggregate =
			new();

		// RS04: overlay effective parents from right to left. Absence in a
		// higher-priority effective description is no contribution, not a
		// reconstructed source cancellation tombstone.
		for ( int index = parents.Count - 1; index >= 0; index-- ) {
			TerminalDescription description =
				parents[ index ].Description;

			foreach (
				BooleanCapability capability
				in description.BooleanCapabilities
			) {
				aggregate.BooleanCapabilities.Add( capability );
			}

			foreach (
				KeyValuePair<NumericCapability, int> pair
				in description.NumericCapabilities
			) {
				aggregate.NumericCapabilities[ pair.Key ] =
					pair.Value;
			}

			foreach (
				KeyValuePair<StringCapability, string> pair
				in description.StringCapabilities
			) {
				aggregate.StringCapabilities[ pair.Key ] =
					pair.Value;
			}

			foreach (
				KeyValuePair<string, TermInfoCapabilityValue> pair
				in description.ExtendedCapabilities
			) {
				aggregate.ExtendedCapabilities[ pair.Key ] =
					pair.Value;
			}
		}

		return aggregate;
	}

	private static void AppendRelativeExtendedCapabilities(
		StringBuilder builder,
		TerminalDescription target,
		IReadOnlyDictionary<string, TermInfoCapabilityValue> inherited,
		TerminalDescriptionSourceRendererOptions options,
		bool includeExtendedCapabilities
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( inherited );
		ArgumentNullException.ThrowIfNull( options );

		List<ExtendedRelativeDirective> directives =
			CreateExtendedRelativeDirectives(
				target.ExtendedCapabilities,
				inherited
			);
		if ( directives.Count == 0 ) {
			return;
		}
		if ( !includeExtendedCapabilities ) {
			throw new InvalidOperationException(
				"Extended-capability output is disabled, but reproducing the target "
					+ "requires one or more local extended declarations or cancellations."
			);
		}

		foreach (
			ExtendedRelativeDirective directive
			in directives
				.OrderBy( item => item.KindOrder )
				.ThenBy(
					item => item.Name,
					StringComparer.Ordinal
				)
		) {
			if ( directive.TargetValue.HasValue ) {
				AppendConfiguredExtendedValue(
					builder,
					directive.Name,
					directive.TargetValue.Value,
					options
				);
			} else {
				AppendConfiguredCancellationField(
					builder,
					directive.Name,
					options
				);
			}
		}
	}

	private static List<ExtendedRelativeDirective>
		CreateExtendedRelativeDirectives(
			IReadOnlyDictionary<string, TermInfoCapabilityValue> target,
			IReadOnlyDictionary<string, TermInfoCapabilityValue> inherited
		) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( inherited );

		SortedSet<string> names =
			new(
				StringComparer.Ordinal
			);
		names.UnionWith( target.Keys );
		names.UnionWith( inherited.Keys );

		List<ExtendedRelativeDirective> directives = [];
		foreach ( string name in names ) {
			bool targetPresent =
				target.TryGetValue(
					name,
					out TermInfoCapabilityValue targetValue
				);
			bool inheritedPresent =
				inherited.TryGetValue(
					name,
					out TermInfoCapabilityValue inheritedValue
				);

			if ( targetPresent
				&& inheritedPresent
				&& targetValue.Equals( inheritedValue ) ) {
				continue;
			}

			TermInfoCapabilityValue orderingValue =
				targetPresent
					? targetValue
					: inheritedValue;
			directives.Add(
				new ExtendedRelativeDirective(
					name,
					targetPresent
						? targetValue
						: null,
					GetExtendedKindOrder( orderingValue )
				)
			);
		}

		return directives;
	}

	private static void AppendConfiguredExtendedValue(
		StringBuilder builder,
		string name,
		TermInfoCapabilityValue value,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( options );
		ValidateExtendedCapabilityName( name );

		switch ( value.Kind ) {
			case TermInfoCapabilityValueKind.Boolean:
				if ( !value.BooleanValue ) {
					throw new InvalidOperationException(
						$"Extended Boolean capability '{name}' has a false stored value, which cannot be represented as present terminfo source state."
					);
				}
				AppendConfiguredBooleanField(
					builder,
					name,
					options
				);
				break;

			case TermInfoCapabilityValueKind.Number:
				AppendConfiguredNumericField(
					builder,
					name,
					value.NumberValue,
					options
				);
				break;

			case TermInfoCapabilityValueKind.String:
				AppendConfiguredStringField(
					builder,
					name,
					value.StringValue,
					options
				);
				break;

			default:
				throw new InvalidOperationException(
					$"Extended capability '{name}' has unsupported value kind '{value.Kind}'."
				);
		}
	}

	private static void AppendConfiguredCancellationField(
		StringBuilder builder,
		string name,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( options );

		AppendConfiguredFieldPrefix(
			builder,
			options.Layout
		);
		builder.Append( name );
		builder.Append( '@' );
		builder.Append( ',' );
		AppendConfiguredFieldSuffix(
			builder,
			options.Layout
		);
	}

	private static void AppendConfiguredUseReference(
		StringBuilder builder,
		string useName,
		TerminalDescriptionSourceRendererOptions options
	) {
		ArgumentNullException.ThrowIfNull( builder );
		ArgumentException.ThrowIfNullOrWhiteSpace( useName );
		ArgumentNullException.ThrowIfNull( options );

		AppendConfiguredFieldPrefix(
			builder,
			options.Layout
		);
		builder.Append( "use=" );
		builder.Append( useName );
		builder.Append( ',' );
		AppendConfiguredFieldSuffix(
			builder,
			options.Layout
		);
	}

	private sealed class ParentAggregate {
		public HashSet<BooleanCapability> BooleanCapabilities {
			get;
		} = [];

		public Dictionary<NumericCapability, int> NumericCapabilities {
			get;
		} = [];

		public Dictionary<StringCapability, string> StringCapabilities {
			get;
		} = [];

		public Dictionary<string, TermInfoCapabilityValue> ExtendedCapabilities {
			get;
		} = new( StringComparer.Ordinal );
	}

	private sealed class ExtendedRelativeDirective {
		public ExtendedRelativeDirective(
			string name,
			TermInfoCapabilityValue? targetValue,
			int kindOrder
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			Name = name;
			TargetValue = targetValue;
			KindOrder = kindOrder;
		}

		public string Name {
			get;
		}

		public TermInfoCapabilityValue? TargetValue {
			get;
		}

		public int KindOrder {
			get;
		}
	}
}
