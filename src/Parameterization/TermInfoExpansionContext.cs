namespace Icod.TermInfo;

/// <summary>
/// Owns persistent uppercase terminfo variables for a sequence of expansions.
/// </summary>
/// <remarks>
/// Lowercase variables are dynamic and are reset for every expansion. Uppercase
/// variables persist only when callers explicitly reuse the same context.
/// </remarks>
public sealed class TermInfoExpansionContext
{
    private readonly TermInfoParameter[] _staticVariables =
        new TermInfoParameter[26];

    internal object SyncRoot { get; } = new();

    /// <summary>
    /// Resets all persistent uppercase variables to integer zero.
    /// </summary>
    public void Reset()
    {
        lock (SyncRoot)
        {
            Array.Clear(_staticVariables);
        }
    }

    internal TermInfoParameter GetStaticVariable(char name)
    {
        ValidateStaticVariableName(name);
        return _staticVariables[name - 'A'];
    }

    internal void SetStaticVariable(
        char name,
        TermInfoParameter value)
    {
        ValidateStaticVariableName(name);
        _staticVariables[name - 'A'] = value;
    }

    private static void ValidateStaticVariableName(char name)
    {
        if (name is < 'A' or > 'Z')
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }
    }
}
