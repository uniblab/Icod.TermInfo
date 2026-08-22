namespace Icod.TermInfo;

/// <summary>
/// Represents a parsed, reusable terminfo parameter program.
/// </summary>
public sealed class TermInfoParameterProgram
{
    private readonly IReadOnlyList<TermInfoInstruction> _instructions;

    private TermInfoParameterProgram(
        string source,
        IReadOnlyList<TermInfoInstruction> instructions)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(instructions);

        Source = source;
        _instructions = instructions;
    }

    /// <summary>
    /// Gets the original terminfo parameter string.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Parses a terminfo parameter string into a reusable program.
    /// </summary>
    public static TermInfoParameterProgram Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        TermInfoParameterParser parser = new(source);
        return new TermInfoParameterProgram(source, parser.Parse());
    }

    /// <summary>
    /// Expands the program with isolated uppercase-variable storage.
    /// </summary>
    public string Expand(params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return Expand(new TermInfoExpansionContext(), parameters);
    }

    /// <summary>
    /// Expands the program using the supplied context for persistent uppercase variables.
    /// </summary>
    public string Expand(
        TermInfoExpansionContext context,
        params TermInfoParameter[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterEvaluator.Evaluate(
            _instructions,
            parameters,
            context);
    }
}
