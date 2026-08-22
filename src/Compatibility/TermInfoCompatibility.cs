namespace Icod.TermInfo;

/// <summary>
/// Provides managed equivalents of the traditional terminfo compatibility
/// operations without introducing process-global terminal state.
/// </summary>
/// <remarks>
/// The compatibility names intentionally resemble <c>tigetflag</c>,
/// <c>tigetnum</c>, <c>tigetstr</c>, <c>tparm</c>/<c>tiparm</c>,
/// <c>tputs</c>, and <c>putp</c>. Managed nullability and exceptions replace
/// the sentinel pointer and integer error values used by native terminfo APIs.
/// </remarks>
public static class TermInfoCompatibility
{
    /// <summary>
    /// Gets a boolean capability by its traditional short name.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the capability is present; otherwise
    /// <see langword="false"/>. Unknown capability names are rejected.
    /// </returns>
    public static bool TiGetFlag(
        TerminalDescription terminal,
        string name)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        ArgumentNullException.ThrowIfNull(name);

        terminal.TryGetBoolean(name, out bool value);
        return value;
    }

    /// <summary>
    /// Gets a numeric capability by its traditional short name.
    /// </summary>
    /// <returns>
    /// The capability value, or <see langword="null"/> when the known
    /// capability is absent. Unknown capability names are rejected.
    /// </returns>
    public static int? TiGetNum(
        TerminalDescription terminal,
        string name)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        ArgumentNullException.ThrowIfNull(name);

        return (terminal.TryGetNumber(name, out int value))
            ? value
            : null
        ;
    }

    /// <summary>
    /// Gets a string capability by its traditional short name.
    /// </summary>
    /// <returns>
    /// The capability string, or <see langword="null"/> when the known
    /// capability is absent. Unknown capability names are rejected.
    /// </returns>
    public static string? TiGetStr(
        TerminalDescription terminal,
        string name)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        ArgumentNullException.ThrowIfNull(name);

        terminal.TryGetString(name, out string? value);
        return value;
    }

    /// <summary>
    /// Expands a terminfo parameter program with isolated variable storage.
    /// </summary>
    public static string TParm(
        string source,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterExpander.Expand(source, parameters);
    }

    /// <summary>
    /// Expands a terminfo parameter program with an explicit persistent-variable
    /// context.
    /// </summary>
    public static string TParm(
        string source,
        TermInfoExpansionContext context,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterExpander.Expand(
            source,
            context,
            parameters);
    }

    /// <summary>
    /// Expands a terminfo parameter program using the managed typed-parameter
    /// representation.
    /// </summary>
    public static string TiParm(
        string source,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parameters);

        return TParm(source, parameters);
    }

    /// <summary>
    /// Expands a terminfo parameter program using the managed typed-parameter
    /// representation and an explicit persistent-variable context.
    /// </summary>
    public static string TiParm(
        string source,
        TermInfoExpansionContext context,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameters);

        return TParm(source, context, parameters);
    }

    /// <summary>
    /// Emits a terminfo string through a character callback using
    /// <c>tputs</c>-style affected-line semantics.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        Action<char> output,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(output);

        TermInfoOutput.TPuts(
            value,
            affectedLines,
            output,
            paddingMode,
            delayProvider);
    }

    /// <summary>
    /// Emits a terminfo string through a character callback using
    /// <c>putp</c>-style one-line semantics.
    /// </summary>
    public static void PutP(
        string value,
        Action<char> output,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(output);

        TPuts(
            value,
            1,
            output,
            paddingMode,
            delayProvider);
    }
}
