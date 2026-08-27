using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Composes explicit provider acquisition, canonical effective rendering, and
/// effective semantic comparison for reusable <c>infocmp</c>-style workflows.
/// </summary>
/// <remarks>
/// The engine consumes the frozen Runtime provider contract. Clean provider
/// misses remain distinguishable through <see cref="TryInspect"/>; provider
/// exceptions propagate. The engine does not enumerate providers or infer hidden
/// system-database provenance.
/// </remarks>
public static class TermInfoInspectionEngine {
	/// <summary>
	/// Attempts to acquire one explicit inspection target.
	/// </summary>
	/// <param name="target">The provider/name target to acquire.</param>
	/// <param name="result">
	/// The acquired target/result pair on success; otherwise
	/// <see langword="null"/>.
	/// </param>
	/// <returns>
	/// <see langword="true"/> when the provider resolves the requested terminal;
	/// otherwise <see langword="false"/> for a clean provider miss.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The provider violates its contract by reporting success with a
	/// <see langword="null"/> terminal description.
	/// </exception>
	public static bool TryInspect(
		TermInfoInspectionTarget target,
		[NotNullWhen( true )] out TermInfoInspectionResult? result
	) {
		ArgumentNullException.ThrowIfNull( target );

		if ( !target.Provider.TryLoad(
			target.RequestedName,
			out TerminalDescription? terminal
		) ) {
			result = null;
			return false;
		}

		if ( terminal is null ) {
			throw new InvalidOperationException(
				$"Terminal provider '{target.Provider.GetType().FullName}' returned success without a terminal description."
			);
		}

		result =
			new TermInfoInspectionResult(
				target,
				terminal
			);
		return true;
	}

	/// <summary>
	/// Acquires one explicit inspection target.
	/// </summary>
	/// <param name="target">The provider/name target to acquire.</param>
	/// <returns>The acquired target and effective terminal description.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="KeyNotFoundException">
	/// The provider reports a clean miss for the requested terminal name.
	/// </exception>
	public static TermInfoInspectionResult Inspect(
		TermInfoInspectionTarget target
	) {
		ArgumentNullException.ThrowIfNull( target );

		if ( TryInspect(
			target,
			out TermInfoInspectionResult? result
		) ) {
			return result;
		}

		throw new KeyNotFoundException(
			$"Terminal profile '{target.RequestedName}' is not available from inspection target '{target.DisplayName}'."
		);
	}

	/// <summary>
	/// Acquires and canonically renders one explicit inspection target.
	/// </summary>
	/// <param name="target">The provider/name target to acquire and render.</param>
	/// <returns>The canonical effective terminfo source representation.</returns>
	public static string Render(
		TermInfoInspectionTarget target
	) {
		ArgumentNullException.ThrowIfNull( target );

		return Render(
			Inspect(
				target
			)
		);
	}

	/// <summary>
	/// Canonically renders an already acquired inspection result without
	/// reacquiring it from its provider.
	/// </summary>
	/// <param name="result">The acquired result to render.</param>
	/// <returns>The canonical effective terminfo source representation.</returns>
	public static string Render(
		TermInfoInspectionResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		return TerminalDescriptionSourceRenderer.Render(
			result.Terminal
		);
	}

	/// <summary>
	/// Acquires and compares two explicit inspection targets.
	/// </summary>
	/// <param name="left">The left provider/name target.</param>
	/// <param name="right">The right provider/name target.</param>
	/// <returns>
	/// The acquired left/right identities and their effective semantic comparison.
	/// </returns>
	public static TermInfoInspectionComparison Compare(
		TermInfoInspectionTarget left,
		TermInfoInspectionTarget right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		return Compare(
			Inspect(
				left
			),
			Inspect(
				right
			)
		);
	}

	/// <summary>
	/// Compares two already acquired inspection results without reacquiring either
	/// terminal from its provider.
	/// </summary>
	/// <param name="left">The acquired left result.</param>
	/// <param name="right">The acquired right result.</param>
	/// <returns>
	/// The original left/right results and their effective semantic comparison.
	/// </returns>
	public static TermInfoInspectionComparison Compare(
		TermInfoInspectionResult left,
		TermInfoInspectionResult right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		return new TermInfoInspectionComparison(
			left,
			right,
			TerminalDescriptionComparer.Compare(
				left.Terminal,
				right.Terminal
			)
		);
	}
}
