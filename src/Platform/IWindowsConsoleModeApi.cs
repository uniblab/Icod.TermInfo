using System.Runtime.InteropServices;

namespace Icod.TermInfo;

internal interface IWindowsConsoleModeApi
{
    bool TryGetStandardHandle(
        TerminalStandardStream stream,
        out nint handle);

    bool TryGetConsoleMode(
        nint handle,
        out uint mode);

    bool TrySetConsoleMode(
        nint handle,
        uint mode);
}

internal sealed class WindowsConsoleModeApi : IWindowsConsoleModeApi
{
    private const int StandardOutputHandle = -11;
    private const int StandardErrorHandle = -12;

    internal static WindowsConsoleModeApi Instance { get; } = new();

    private WindowsConsoleModeApi()
    {
    }

    public bool TryGetStandardHandle(
        TerminalStandardStream stream,
        out nint handle)
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
                "Console output mode is available only for standard output or standard error.",
                nameof(stream));
        }

        int standardHandle = stream switch
        {
            TerminalStandardStream.Output => StandardOutputHandle,
            TerminalStandardStream.Error => StandardErrorHandle,
            _ => throw new ArgumentOutOfRangeException(nameof(stream)),
        };

        handle = NativeMethods.GetStdHandle(standardHandle);

        return (handle != IntPtr.Zero)
            && (handle != new IntPtr(-1));
    }

    public bool TryGetConsoleMode(
        nint handle,
        out uint mode)
    {
        return NativeMethods.GetConsoleMode(
            handle,
            out mode);
    }

    public bool TrySetConsoleMode(
        nint handle,
        uint mode)
    {
        return NativeMethods.SetConsoleMode(
            handle,
            mode);
    }

#pragma warning disable SYSLIB1054 // Keep this small blittable interop surface free of generated unsafe code.
    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern nint GetStdHandle(
            int standardHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetConsoleMode(
            nint consoleHandle,
            out uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetConsoleMode(
            nint consoleHandle,
            uint mode);
    }
#pragma warning restore SYSLIB1054
}
