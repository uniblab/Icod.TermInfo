namespace Icod.TermInfo;

/// <summary>
/// Expands terminfo parameter strings.
/// </summary>
public static class TermInfoParameterExpander
{
    /// <summary>
    /// Parses and expands a terminfo parameter string with isolated variable storage.
    /// </summary>
    public static string Expand(
        string source,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterProgram.Parse(source).Expand(parameters);
    }

    /// <summary>
    /// Parses and expands a terminfo parameter string using the supplied context.
    /// </summary>
    public static string Expand(
        string source,
        TermInfoExpansionContext context,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterProgram.Parse(source).Expand(context, parameters);
    }
}
