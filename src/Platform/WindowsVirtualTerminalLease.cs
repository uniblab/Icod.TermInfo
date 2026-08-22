namespace Icod.TermInfo;

internal sealed class WindowsVirtualTerminalLease : IDisposable
{
    private IWindowsConsoleModeApi? _consoleModeApi;
    private readonly nint _handle;
    private readonly uint _originalMode;
    private readonly bool _restoreMode;

    internal WindowsVirtualTerminalLease(
        IWindowsConsoleModeApi consoleModeApi,
        nint handle,
        uint originalMode,
        bool restoreMode)
    {
        ArgumentNullException.ThrowIfNull(consoleModeApi);

        _consoleModeApi = consoleModeApi;
        _handle = handle;
        _originalMode = originalMode;
        _restoreMode = restoreMode;
    }

    public void Dispose()
    {
        IWindowsConsoleModeApi? consoleModeApi =
            Interlocked.Exchange(
                ref _consoleModeApi,
                null);

        if ((consoleModeApi is null) || !_restoreMode)
        {
            return;
        }

        _ = consoleModeApi.TrySetConsoleMode(
            _handle,
            _originalMode);
    }
}
