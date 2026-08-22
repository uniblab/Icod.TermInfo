namespace Icod.TermInfo;

/// <summary>
/// Provides explicit, reversible Windows console virtual-terminal output
/// enablement.
/// </summary>
public static class WindowsVirtualTerminal
{
    internal const uint EnableProcessedOutput = 0x0001u;
    internal const uint EnableVirtualTerminalProcessing = 0x0004u;
    internal const uint RequiredOutputModeFlags =
        EnableProcessedOutput | EnableVirtualTerminalProcessing;

    /// <summary>
    /// Attempts to enable virtual-terminal processing for standard output.
    /// </summary>
    /// <returns>
    /// A lease which restores the previous console mode when disposed, or
    /// <see langword="null"/> when the process is not running on Windows, the
    /// stream is redirected or is not a console, or the mode cannot be changed.
    /// </returns>
    public static IDisposable? TryEnableOutput()
    {
        return TryEnableOutput(TerminalStandardStream.Output);
    }

    /// <summary>
    /// Attempts to enable virtual-terminal processing for standard output or
    /// standard error.
    /// </summary>
    /// <param name="stream">
    /// The output stream whose Windows console mode should be changed.
    /// </param>
    /// <returns>
    /// A lease which restores the previous console mode when disposed, or
    /// <see langword="null"/> when the process is not running on Windows, the
    /// stream is redirected or is not a console, or the mode cannot be changed.
    /// </returns>
    public static IDisposable? TryEnableOutput(
        TerminalStandardStream stream)
    {
        ValidateOutputStream(stream);

        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        if (TerminalEnvironment.IsRedirected(stream))
        {
            return null;
        }

        return TryEnableOutput(
            stream,
            WindowsConsoleModeApi.Instance);
    }

    internal static IDisposable? TryEnableOutput(
        TerminalStandardStream stream,
        IWindowsConsoleModeApi consoleModeApi)
    {
        ValidateOutputStream(stream);
        ArgumentNullException.ThrowIfNull(consoleModeApi);

        if (!consoleModeApi.TryGetStandardHandle(
                stream,
                out nint handle))
        {
            return null;
        }

        if (!consoleModeApi.TryGetConsoleMode(
                handle,
                out uint originalMode))
        {
            return null;
        }

        uint enabledMode =
            originalMode | RequiredOutputModeFlags;
        bool modeChanged = enabledMode != originalMode;

        if (modeChanged
            && !consoleModeApi.TrySetConsoleMode(
                handle,
                enabledMode))
        {
            return null;
        }

        return new WindowsVirtualTerminalLease(
            consoleModeApi,
            handle,
            originalMode,
            modeChanged);
    }

    private static void ValidateOutputStream(
        TerminalStandardStream stream)
    {
        if (!Enum.IsDefined(
                typeof(TerminalStandardStream),
                stream))
        {
            throw new ArgumentOutOfRangeException(nameof(stream));
        }

        if (stream == TerminalStandardStream.Input)
        {
            throw new ArgumentException(
                "Virtual-terminal output processing can only be enabled for standard output or standard error.",
                nameof(stream));
        }
    }
}
