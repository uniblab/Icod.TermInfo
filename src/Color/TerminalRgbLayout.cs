namespace Icod.TermInfo;

/// <summary>
/// Describes the number of bits assigned to each component of a packed direct
/// RGB color parameter.
/// </summary>
public readonly record struct TerminalRgbLayout
{
    private const int MaximumPackedBits = 31;

    /// <summary>
    /// Initializes a direct RGB channel layout.
    /// </summary>
    public TerminalRgbLayout(
        int redBits,
        int greenBits,
        int blueBits)
    {
        ValidateChannelBits(redBits, nameof(redBits));
        ValidateChannelBits(greenBits, nameof(greenBits));
        ValidateChannelBits(blueBits, nameof(blueBits));

        int totalBits = redBits + greenBits + blueBits;
        if (totalBits <= 0 || totalBits > MaximumPackedBits)
        {
            throw new ArgumentException(
                $"A packed RGB layout must use between 1 and {MaximumPackedBits} bits in total.");
        }

        RedBits = redBits;
        GreenBits = greenBits;
        BlueBits = blueBits;
    }

    /// <summary>
    /// Gets the number of bits assigned to red.
    /// </summary>
    public int RedBits { get; }

    /// <summary>
    /// Gets the number of bits assigned to green.
    /// </summary>
    public int GreenBits { get; }

    /// <summary>
    /// Gets the number of bits assigned to blue.
    /// </summary>
    public int BlueBits { get; }

    /// <summary>
    /// Gets the total packed parameter width.
    /// </summary>
    public int TotalBits => RedBits + GreenBits + BlueBits;

    /// <summary>
    /// Packs an eight-bit-per-channel RGB color according to this layout.
    /// </summary>
    /// <remarks>
    /// Red occupies the most-significant channel field, green the middle field,
    /// and blue the least-significant field. Components are scaled to the width
    /// advertised by the layout.
    /// </remarks>
    public int Pack(TerminalRgbColor color)
    {
        long red = Scale(color.Red, RedBits);
        long green = Scale(color.Green, GreenBits);
        long blue = Scale(color.Blue, BlueBits);

        long packed =
            (red << (GreenBits + BlueBits))
            | (green << BlueBits)
            | blue;

        return checked((int)packed);
    }

    private static long Scale(
        byte value,
        int bits)
    {
        if (bits == 0)
        {
            return 0;
        }

        long maximum = (1L << bits) - 1L;
        return ((long)value * maximum + 127L) / 255L;
    }

    private static void ValidateChannelBits(
        int bits,
        string parameterName)
    {
        if (bits < 0 || bits > MaximumPackedBits)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                bits,
                $"An RGB channel width must be between 0 and {MaximumPackedBits} bits.");
        }
    }
}
