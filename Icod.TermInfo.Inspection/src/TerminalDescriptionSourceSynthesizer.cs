namespace Icod.TermInfo.Inspection;

/// <summary>
/// Synthesizes deterministic terminfo source for an effective terminal
/// description relative to an explicitly ordered parent set.
/// </summary>
/// <remarks>
/// RS01 validates and freezes synthesis inputs. RS02 adds deterministic standard
/// Boolean, numeric, and string capability deltas and cancellations for ordered
/// parent sets. RS03 extends the same engine to ordinal, case-sensitive extended
/// capabilities, including kind changes and inherited cancellation. RS04 freezes
/// exact multi-parent order and source-reference fidelity: supplied
/// <see cref="TerminalDescriptionSourceSynthesisParent.UseName"/> values are
/// emitted without canonicalization or pruning, including repeated effective
/// parents under distinct references. The zero-parent form remains equivalent
/// to effective-source rendering.
/// </remarks>
public static class TerminalDescriptionSourceSynthesizer {
	/// <summary>
	/// Synthesizes relative terminfo source for one target description.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="parents">
	/// Ordered parents whose <c>use=</c> precedence is part of the synthesis
	/// contract.
	/// </param>
	/// <param name="options">
	/// Optional deterministic synthesis and resource-limit policy.
	/// </param>
	/// <returns>The deterministic terminfo source representation.</returns>
	/// <exception cref="ArgumentException">
	/// The parent sequence contains a null item, duplicate <c>use=</c> reference
	/// name, or more parents than the configured maximum.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> or <paramref name="parents"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Extended-capability output is disabled while reproducing the target requires
	/// one or more local extended declarations or cancellations.
	/// </exception>
	public static string Synthesize(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
		TerminalDescriptionSourceSynthesisOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );

		TerminalDescriptionSourceSynthesisPlan plan =
			CreatePlan(
				target,
				parents,
				options
			);

		return RenderPlan( plan );
	}

	/// <summary>
	/// Writes relative terminfo source for one target description.
	/// </summary>
	/// <param name="writer">The destination writer.</param>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="parents">The explicitly ordered synthesis parents.</param>
	/// <param name="options">
	/// Optional deterministic synthesis and resource-limit policy.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="writer"/>, <paramref name="target"/>, or
	/// <paramref name="parents"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The parent sequence violates the synthesis parent contract.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Extended-capability output is disabled while reproducing the target requires
	/// one or more local extended declarations or cancellations.
	/// </exception>
	public static void Write(
		TextWriter writer,
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
		TerminalDescriptionSourceSynthesisOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( writer );
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );

		writer.Write(
			Synthesize(
				target,
				parents,
				options
			)
		);
	}

	internal static TerminalDescriptionSourceSynthesisPlan CreatePlan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents,
		TerminalDescriptionSourceSynthesisOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );

		TerminalDescriptionSourceSynthesisOptions effectiveOptions =
			options
			?? new TerminalDescriptionSourceSynthesisOptions();
		List<TerminalDescriptionSourceSynthesisParent> orderedParents = [];
		HashSet<string> useNames =
			new(
				StringComparer.Ordinal
			);

		foreach ( TerminalDescriptionSourceSynthesisParent? parent in parents ) {
			if ( parent is null ) {
				throw new ArgumentException(
					"The synthesis parent sequence cannot contain null.",
					nameof( parents )
				);
			}
			if ( orderedParents.Count >= effectiveOptions.MaximumParentCount ) {
				throw new ArgumentException(
					$"The synthesis request exceeds the configured maximum of {effectiveOptions.MaximumParentCount} parents.",
					nameof( parents )
				);
			}
			if ( !useNames.Add( parent.UseName ) ) {
				throw new ArgumentException(
					$"The synthesis parent reference '{parent.UseName}' is duplicated. "
						+ "Reference names must be unique within one ordered parent list.",
					nameof( parents )
				);
			}

			orderedParents.Add( parent );
		}

		return new TerminalDescriptionSourceSynthesisPlan(
			target,
			orderedParents,
			effectiveOptions
		);
	}

	private static string RenderPlan(
		TerminalDescriptionSourceSynthesisPlan plan
	) {
		ArgumentNullException.ThrowIfNull( plan );

		if ( plan.Parents.Count == 0 ) {
			if ( !plan.Options.IncludeExtendedCapabilities
				&& plan.Target.ExtendedCapabilities.Count != 0 ) {
				throw new InvalidOperationException(
					"Extended-capability output is disabled, but reproducing the target "
						+ "requires local extended declarations."
				);
			}

			return TerminalDescriptionSourceRenderer.Render(
				plan.Target,
				plan.Options.CreateRendererOptions()
			);
		}

		return TerminalDescriptionSourceRenderer.RenderRelative(
			plan
		);
	}
}
