namespace Icod.TermInfo;

/// <summary>
/// A valid terminfo parameter program cannot be evaluated with the supplied values.
/// </summary>
public sealed class TermInfoEvaluationException : InvalidOperationException
{
    internal TermInfoEvaluationException(
        string message,
        int position)
        : base($"{message} (position {position}).")
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        Position = position;
    }

    /// <summary>
    /// Gets the zero-based source position associated with the error.
    /// </summary>
    public int Position { get; }
}
