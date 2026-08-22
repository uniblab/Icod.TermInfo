namespace Icod.TermInfo;

/// <summary>
/// Describes semantic color support derived from one immutable terminal
/// description.
/// </summary>
public readonly record struct TerminalColorSupport
{
    internal TerminalColorSupport(
        TerminalColorModel model,
        TerminalColorTier tier,
        int? colorCount,
        int indexedColorCount,
        int? colorPairCount,
        int? noColorVideoMask,
        TerminalRgbLayout? rgbLayout,
        bool hasForegroundSelector,
        bool hasBackgroundSelector,
        bool backColorErase,
        bool canChangeColor,
        bool usesHlsColorInitialization,
        bool hasInitializeColor,
        bool hasOriginalColorPair,
        bool hasOriginalColors)
    {
        Model = model;
        Tier = tier;
        ColorCount = colorCount;
        IndexedColorCount = indexedColorCount;
        ColorPairCount = colorPairCount;
        NoColorVideoMask = noColorVideoMask;
        RgbLayout = rgbLayout;
        HasForegroundSelector = hasForegroundSelector;
        HasBackgroundSelector = hasBackgroundSelector;
        BackColorErase = backColorErase;
        CanChangeColor = canChangeColor;
        UsesHlsColorInitialization = usesHlsColorInitialization;
        HasInitializeColor = hasInitializeColor;
        HasOriginalColorPair = hasOriginalColorPair;
        HasOriginalColors = hasOriginalColors;
    }

    /// <summary>
    /// Gets the semantic color model.
    /// </summary>
    public TerminalColorModel Model { get; }

    /// <summary>
    /// Gets the convenient color-depth classification.
    /// </summary>
    public TerminalColorTier Tier { get; }

    /// <summary>
    /// Gets the raw <c>colors</c> value, or <see langword="null"/> when absent.
    /// </summary>
    public int? ColorCount { get; }

    /// <summary>
    /// Gets the safely addressable indexed-color range. For an indexed terminal
    /// this is <see cref="ColorCount"/>; for a direct-color terminal this is the
    /// retained indexed prefix advertised by extended <c>CO</c> metadata.
    /// </summary>
    public int IndexedColorCount { get; }

    /// <summary>
    /// Gets the independently advertised <c>pairs</c> value, or
    /// <see langword="null"/> when absent.
    /// </summary>
    public int? ColorPairCount { get; }

    /// <summary>
    /// Gets the raw <c>ncv</c> attribute mask, or <see langword="null"/> when
    /// absent.
    /// </summary>
    public int? NoColorVideoMask { get; }

    /// <summary>
    /// Gets the direct RGB channel layout when <see cref="Model"/> is
    /// <see cref="TerminalColorModel.DirectRgb"/>.
    /// </summary>
    public TerminalRgbLayout? RgbLayout { get; }

    /// <summary>
    /// Gets whether a foreground color selector is present.
    /// </summary>
    public bool HasForegroundSelector { get; }

    /// <summary>
    /// Gets whether a background color selector is present.
    /// </summary>
    public bool HasBackgroundSelector { get; }

    /// <summary>
    /// Gets the standard <c>bce</c> value.
    /// </summary>
    public bool BackColorErase { get; }

    /// <summary>
    /// Gets the standard <c>ccc</c> value.
    /// </summary>
    public bool CanChangeColor { get; }

    /// <summary>
    /// Gets the standard <c>hls</c> value.
    /// </summary>
    public bool UsesHlsColorInitialization { get; }

    /// <summary>
    /// Gets whether the standard <c>initc</c> capability is present.
    /// </summary>
    public bool HasInitializeColor { get; }

    /// <summary>
    /// Gets whether the standard <c>op</c> capability is present.
    /// </summary>
    public bool HasOriginalColorPair { get; }

    /// <summary>
    /// Gets whether the standard <c>oc</c> capability is present.
    /// </summary>
    public bool HasOriginalColors { get; }
}
