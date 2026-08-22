using System.Globalization;

namespace Icod.TermInfo;

/// <summary>
/// Provides stateless semantic color inspection and safe color-capability
/// expansion.
/// </summary>
public static class TerminalColors
{
    private const string DirectRgbCapabilityName = "RGB";
    private const string DirectIndexedPrefixCapabilityName = "CO";
    private const int TrueColorCount = 1 << 24;

    /// <summary>
    /// Derives semantic color support from raw standard and extended terminfo
    /// capabilities.
    /// </summary>
    public static TerminalColorSupport GetColorSupport(
        TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        int? colors = GetNonNegativeNumber(
            terminal,
            NumericCapability.Colors,
            "colors");
        int? pairs = GetNonNegativeNumber(
            terminal,
            NumericCapability.ColorPairs,
            "pairs");
        int? noColorVideo = GetNonNegativeNumber(
            terminal,
            NumericCapability.NoColorVideo,
            "ncv");

        bool hasSetForeground =
            terminal.GetString(StringCapability.SetForegroundColor) is not null;
        bool hasSetBackground =
            terminal.GetString(StringCapability.SetBackgroundColor) is not null;
        bool hasLegacyForeground =
            terminal.GetString(StringCapability.SetLegacyForegroundColor) is not null;
        bool hasLegacyBackground =
            terminal.GetString(StringCapability.SetLegacyBackgroundColor) is not null;
        bool hasForegroundSelector =
            hasSetForeground || hasLegacyForeground;
        bool hasBackgroundSelector =
            hasSetBackground || hasLegacyBackground;

        TerminalColorModel model = TerminalColorModel.None;
        TerminalColorTier tier = TerminalColorTier.Monochrome;
        TerminalRgbLayout? rgbLayout = null;
        int indexedColorCount = 0;

        if (TryGetDirectRgbLayout(
                terminal,
                colors,
                out TerminalRgbLayout directLayout))
        {
            if (!hasSetForeground || !hasSetBackground)
            {
                throw CreateInvalidColorMetadataException(
                    terminal,
                    "extended 'RGB' requires both standard 'setaf' and 'setab' selectors");
            }

            rgbLayout = directLayout;
            model = TerminalColorModel.DirectRgb;
            indexedColorCount = GetDirectIndexedPrefixCount(
                terminal,
                colors!.Value);
            tier = ClassifyDirect(directLayout, colors.Value);
        }
        else if (colors is > 0
            && (hasForegroundSelector || hasBackgroundSelector))
        {
            model = TerminalColorModel.Indexed;
            indexedColorCount = colors.Value;
            tier = ClassifyIndexed(colors.Value);
        }

        return new TerminalColorSupport(
            model,
            tier,
            colors,
            indexedColorCount,
            pairs,
            noColorVideo,
            rgbLayout,
            hasForegroundSelector,
            hasBackgroundSelector,
            terminal.GetBoolean(BooleanCapability.BackColorErase),
            terminal.GetBoolean(BooleanCapability.CanChangeColor),
            terminal.GetBoolean(BooleanCapability.HueLightnessSaturation),
            terminal.GetString(StringCapability.InitializeColor) is not null,
            terminal.GetString(StringCapability.OriginalColorPair) is not null,
            terminal.GetString(StringCapability.OriginalColors) is not null);
    }

    /// <summary>
    /// Expands the terminal's foreground selector for an indexed color.
    /// </summary>
    public static string ExpandForeground(
        TerminalDescription terminal,
        int colorIndex)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        return ExpandIndexed(terminal, colorIndex, foreground: true);
    }

    /// <summary>
    /// Expands the terminal's background selector for an indexed color.
    /// </summary>
    public static string ExpandBackground(
        TerminalDescription terminal,
        int colorIndex)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        return ExpandIndexed(terminal, colorIndex, foreground: false);
    }

    /// <summary>
    /// Expands the terminal's foreground selector for a direct RGB color.
    /// </summary>
    public static string ExpandForeground(
        TerminalDescription terminal,
        TerminalRgbColor color)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        return ExpandDirect(terminal, color, foreground: true);
    }

    /// <summary>
    /// Expands the terminal's background selector for a direct RGB color.
    /// </summary>
    public static string ExpandBackground(
        TerminalDescription terminal,
        TerminalRgbColor color)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        return ExpandDirect(terminal, color, foreground: false);
    }

    private static string ExpandIndexed(
        TerminalDescription terminal,
        int colorIndex,
        bool foreground)
    {
        TerminalColorSupport support = GetColorSupport(terminal);

        if (support.IndexedColorCount <= 0)
        {
            throw new InvalidOperationException(
                $"Terminal '{terminal.Name}' does not advertise an indexed color range.");
        }

        if (colorIndex < 0 || colorIndex >= support.IndexedColorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorIndex),
                colorIndex,
                $"The color index must be between 0 and {support.IndexedColorCount - 1}.");
        }

        StringCapability? selector = foreground
            ? GetForegroundSelector(terminal)
            : GetBackgroundSelector(terminal);

        if (selector is null)
        {
            string direction = foreground ? "foreground" : "background";
            throw new InvalidOperationException(
                $"Terminal '{terminal.Name}' does not advertise a {direction} color selector.");
        }

        return terminal.Expand(selector.Value, colorIndex);
    }

    private static string ExpandDirect(
        TerminalDescription terminal,
        TerminalRgbColor color,
        bool foreground)
    {
        TerminalColorSupport support = GetColorSupport(terminal);

        if (support.Model != TerminalColorModel.DirectRgb
            || support.RgbLayout is not TerminalRgbLayout layout
            || support.ColorCount is not int colorCount)
        {
            throw new InvalidOperationException(
                $"Terminal '{terminal.Name}' does not advertise direct RGB color semantics.");
        }

        int packed = layout.Pack(color);
        if (packed >= colorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                $"The packed RGB value {packed} is outside terminal '{terminal.Name}'s advertised color range of {colorCount} values.");
        }

        if (packed > 0 && packed < support.IndexedColorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                $"The packed RGB value {packed} falls inside terminal '{terminal.Name}'s retained indexed-color prefix (0-{support.IndexedColorCount - 1}).");
        }

        StringCapability selector = foreground
            ? StringCapability.SetForegroundColor
            : StringCapability.SetBackgroundColor;

        return terminal.Expand(selector, packed);
    }

    private static StringCapability? GetForegroundSelector(
        TerminalDescription terminal)
    {
        if (terminal.GetString(StringCapability.SetForegroundColor) is not null)
        {
            return StringCapability.SetForegroundColor;
        }

        if (terminal.GetString(StringCapability.SetLegacyForegroundColor) is not null)
        {
            return StringCapability.SetLegacyForegroundColor;
        }

        return null;
    }

    private static StringCapability? GetBackgroundSelector(
        TerminalDescription terminal)
    {
        if (terminal.GetString(StringCapability.SetBackgroundColor) is not null)
        {
            return StringCapability.SetBackgroundColor;
        }

        if (terminal.GetString(StringCapability.SetLegacyBackgroundColor) is not null)
        {
            return StringCapability.SetLegacyBackgroundColor;
        }

        return null;
    }

    private static bool TryGetDirectRgbLayout(
        TerminalDescription terminal,
        int? colors,
        out TerminalRgbLayout layout)
    {
        if (!terminal.TryGetExtendedCapability(
                DirectRgbCapabilityName,
                out TermInfoCapabilityValue rgb))
        {
            layout = default;
            return false;
        }

        if (rgb.IsBoolean && !rgb.BooleanValue)
        {
            layout = default;
            return false;
        }

        if (colors is null || colors.Value <= 0)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                "extended 'RGB' requires a positive standard 'colors' value");
        }

        try
        {
            if (rgb.IsBoolean)
            {
                layout = DeriveBooleanRgbLayout(colors.Value);
                return true;
            }

            if (rgb.IsNumber)
            {
                int channelBits = rgb.NumberValue;
                if (channelBits <= 0)
                {
                    throw CreateInvalidColorMetadataException(
                        terminal,
                        "numeric extended 'RGB' must be positive");
                }

                layout = new TerminalRgbLayout(
                    channelBits,
                    channelBits,
                    channelBits);
                return true;
            }

            layout = ParseRgbLayout(terminal, rgb.StringValue);
            return true;
        }
        catch (ArgumentException exception)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                "extended 'RGB' describes an unsupported channel layout",
                exception);
        }
    }

    private static TerminalRgbLayout DeriveBooleanRgbLayout(int colors)
    {
        if (colors < 8)
        {
            throw new ArgumentException(
                "Boolean RGB requires at least eight advertised colors.");
        }

        int width = 0;
        int maximum = colors - 1;
        while (maximum > 0)
        {
            width++;
            maximum >>= 1;
        }

        int channelBits = (width + 2) / 3;

        return new TerminalRgbLayout(
            channelBits,
            channelBits,
            width - (2 * channelBits));
    }

    private static TerminalRgbLayout ParseRgbLayout(
        TerminalDescription terminal,
        string value)
    {
        string[] components = value.Split('/');
        if (components.Length != 3)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                "string extended 'RGB' must contain exactly three slash-separated decimal channel widths");
        }

        int[] widths = new int[3];
        for (int i = 0; i < components.Length; i++)
        {
            if (!int.TryParse(
                    components[i],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int width)
                || width < 0)
            {
                throw CreateInvalidColorMetadataException(
                    terminal,
                    "string extended 'RGB' must contain three non-negative decimal channel widths");
            }

            widths[i] = width;
        }

        return new TerminalRgbLayout(
            widths[0],
            widths[1],
            widths[2]);
    }

    private static int GetDirectIndexedPrefixCount(
        TerminalDescription terminal,
        int colors)
    {
        if (!terminal.TryGetExtendedCapability(
                DirectIndexedPrefixCapabilityName,
                out TermInfoCapabilityValue prefix))
        {
            return 0;
        }

        if (!prefix.IsNumber)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                "extended 'CO' must be numeric when used with direct RGB color");
        }

        int value = prefix.NumberValue;
        if (value < 0 || value >= colors)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                $"extended 'CO' must be non-negative and less than the advertised 'colors' value ({colors})");
        }

        return value;
    }

    private static int? GetNonNegativeNumber(
        TerminalDescription terminal,
        NumericCapability capability,
        string shortName)
    {
        int? value = terminal.GetNumber(capability);
        if (value is int number && number < 0)
        {
            throw CreateInvalidColorMetadataException(
                terminal,
                $"standard '{shortName}' cannot be negative");
        }

        return value;
    }

    private static TerminalColorTier ClassifyIndexed(int colors)
    {
        return colors switch
        {
            4 => TerminalColorTier.Color4,
            8 => TerminalColorTier.Color8,
            16 => TerminalColorTier.Color16,
            256 => TerminalColorTier.Color256,
            _ => TerminalColorTier.OtherIndexed,
        };
    }

    private static TerminalColorTier ClassifyDirect(
        TerminalRgbLayout layout,
        int colors)
    {
        if (layout.RedBits == 8
            && layout.GreenBits == 8
            && layout.BlueBits == 8
            && colors >= TrueColorCount)
        {
            return TerminalColorTier.TrueColor;
        }

        return TerminalColorTier.OtherDirectRgb;
    }

    private static InvalidOperationException CreateInvalidColorMetadataException(
        TerminalDescription terminal,
        string message,
        Exception? innerException = null)
    {
        return new InvalidOperationException(
            $"Terminal '{terminal.Name}' has invalid color metadata: {message}.",
            innerException);
    }
}
