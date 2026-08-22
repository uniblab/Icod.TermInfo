namespace Icod.TermInfo;

/// <summary>
/// Represents terminal dimensions in character cells.
/// </summary>
public readonly record struct TerminalSize
{
    /// <summary>
    /// Initializes terminal dimensions.
    /// </summary>
    public TerminalSize(
        int columns,
        int rows)
    {
        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(columns),
                columns,
                "The terminal column count must be positive.");
        }

        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rows),
                rows,
                "The terminal row count must be positive.");
        }

        Columns = columns;
        Rows = rows;
    }

    /// <summary>
    /// Gets the number of character columns.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the number of character rows.
    /// </summary>
    public int Rows { get; }
}
