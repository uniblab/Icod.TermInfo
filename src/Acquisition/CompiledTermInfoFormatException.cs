namespace Icod.TermInfo;

/// <summary>
/// A compiled terminfo entry is malformed or uses an unsupported compiled
/// layout.
/// </summary>
public sealed class CompiledTermInfoFormatException : FormatException
{
    /// <summary>
    /// Initializes an exception with the default message.
    /// </summary>
    public CompiledTermInfoFormatException()
    {
        Offset = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message.
    /// </summary>
    public CompiledTermInfoFormatException(string? message)
        : base(message)
    {
        Offset = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message and inner
    /// exception.
    /// </summary>
    public CompiledTermInfoFormatException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
        Offset = -1;
    }

    internal CompiledTermInfoFormatException(
        string message,
        int offset,
        string? section,
        Exception? innerException = null)
        : base(
            ValidateDiagnostic(
                message,
                offset,
                section),
            innerException)
    {
        Offset = offset;
        Section = section;
    }

    /// <summary>
    /// Gets the zero-based compiled-entry byte offset associated with the
    /// error, or <c>-1</c> when no byte offset was supplied.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the stable compiled-entry section associated with the error, or
    /// <see langword="null"/> when no section was supplied.
    /// </summary>
    public string? Section { get; }

    private static string ValidateDiagnostic(
        string message,
        int offset,
        string? section)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (offset < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (section is not null
            && string.IsNullOrWhiteSpace(section))
        {
            throw new ArgumentException(
                "The section name cannot be empty or whitespace.",
                nameof(section));
        }

        return message;
    }
}
