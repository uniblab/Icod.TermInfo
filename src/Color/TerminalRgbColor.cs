namespace Icod.TermInfo;

/// <summary>
/// Represents an RGB color with eight-bit red, green, and blue components.
/// </summary>
public readonly record struct TerminalRgbColor
{
    /// <summary>
    /// Initializes an RGB color.
    /// </summary>
    public TerminalRgbColor(
        byte red,
        byte green,
        byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    /// <summary>
    /// Gets the red component.
    /// </summary>
    public byte Red { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public byte Green { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public byte Blue { get; }
}
