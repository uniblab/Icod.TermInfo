using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Inspection;

public static partial class TerminalDescriptionSourceRenderer {
	internal static string RenderRelativeStandard(
		TerminalDescriptionSourceSynthesisPlan plan
	) {
		ArgumentNullException.ThrowIfNull( plan );
		ValidateIdentity( plan.Target );
		ValidateStandardRelativeScope( plan );

		TerminalDescriptionSourceRendererOptions options =
			plan.Options.CreateRendererOptions();
		StandardParentAggregate inherited =
			CreateStandardParentAggregate(
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

	private static void ValidateStandardRelativeScope(
		TerminalDescriptionSourceSynthesisPlan plan
	) {
		ArgumentNullException.ThrowIfNull( plan );

		if ( plan.Target.ExtendedCapabilities.Count != 0 ) {
			throw new NotSupportedException(
				"Relative synthesis involving extended capabilities is introduced by RS03."
			);
		}

		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in plan.Parents
		) {
			if ( parent.Description.ExtendedCapabilities.Count != 0 ) {
				throw new NotSupportedException(
					"Relative synthesis involving extended capabilities is introduced by RS03."
				);
			}
		}
	}

	private static StandardParentAggregate CreateStandardParentAggregate(
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents
	) {
		ArgumentNullException.ThrowIfNull( parents );

		StandardParentAggregate aggregate =
			new();

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
		}

		return aggregate;
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

	private sealed class StandardParentAggregate {
		public HashSet<BooleanCapability> BooleanCapabilities {
			get;
		} = [];

		public Dictionary<NumericCapability, int> NumericCapabilities {
			get;
		} = [];

		public Dictionary<StringCapability, string> StringCapabilities {
			get;
		} = [];
	}
}
