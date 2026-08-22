namespace Icod.TermInfo;

/// <summary>
/// The terminfo parameter program is malformed.
/// </summary>
public sealed class TermInfoFormatException : FormatException
{
    internal TermInfoFormatException(
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
