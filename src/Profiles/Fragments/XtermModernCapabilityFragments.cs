namespace Icod.TermInfo;

internal static class XtermModernCapabilityFragments
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), matching xterm-p370 and its selected feature fragments.
    internal static TerminalDescriptionBuilder ApplyXtermModernMetadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetExtendedBoolean("OTbs")
            .SetExtendedBoolean("AX")
            .SetExtendedBoolean("XT")
            .SetExtendedString("E3", "\u001b[3J")
            .SetExtendedString("smxx", "\u001b[9m")
            .SetExtendedString("rmxx", "\u001b[29m")
            .ApplyXtermBracketedPasteMetadata()
            .ApplyXtermTmux2Metadata()
            .ApplyXtermSgrMouseMetadata()
            .ApplyXtermFocusMetadata()
            .ApplyXtermReportMetadata();
    }

    internal static TerminalDescriptionBuilder ApplyXtermBracketedPasteMetadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetExtendedString("BD", "\u001b[?2004l")
            .SetExtendedString("BE", "\u001b[?2004h")
            .SetExtendedString("PE", "\u001b[201~")
            .SetExtendedString("PS", "\u001b[200~");
    }

    internal static TerminalDescriptionBuilder ApplyXtermTmux2Metadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetExtendedString("Cr", "\u001b]112\u001b\\")
            .SetExtendedString("Cs", "\u001b]12;%p1%s\u001b\\")
            .SetExtendedString("Ms", "\u001b]52;%p1%s;%p2%s\u001b\\")
            .SetExtendedString("Se", "\u001b[ q")
            .SetExtendedString("Ss", "\u001b[%p1%d q");
    }

    internal static TerminalDescriptionBuilder ApplyXtermSgrMouseMetadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetString(StringCapability.KeyMouse, "\u001b[<")
            .SetExtendedString(
                "XM",
                "\u001b[?1006;1000%?%p1%{1}%=%th%el%;")
            .SetExtendedString(
                "xm",
                "\u001b[<%i%p3%d;%p1%d;%p2%d;%?%p4%tM%em%;");
    }

    internal static TerminalDescriptionBuilder ApplyXtermFocusMetadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetExtendedBoolean("XF")
            .SetExtendedString("fd", "\u001b[?1004l")
            .SetExtendedString("fe", "\u001b[?1004h")
            .SetExtendedString("kxIN", "\u001b[I")
            .SetExtendedString("kxOUT", "\u001b[O");
    }

    internal static TerminalDescriptionBuilder ApplyXtermReportMetadata(
        this TerminalDescriptionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .SetExtendedString("RV", "\u001b[>c")
            .SetExtendedString("rv", "\u001b\\[>41;[1-6][0-9][0-9];0c")
            .SetExtendedString("XR", "\u001b[>0q")
            .SetExtendedString(
                "xr",
                "\u001bP>\\|XTerm\\(([1-9][0-9]+)\\)\u001b\\\\");
    }
}
