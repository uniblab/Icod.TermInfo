using System.Runtime.InteropServices;

namespace Icod.TermInfo;

internal sealed class WindowsTerminalSizeProvider : ITerminalSizeProvider
{
    private const int StandardOutputHandle = -11;
    private const int StandardErrorHandle = -12;

    public bool TryGetSize(
        TerminalStandardStream stream,
        out TerminalSize size)
    {
        if (!Enum.IsDefined(typeof(TerminalStandardStream), stream))
        {
            throw new ArgumentOutOfRangeException(nameof(stream));
        }

        size = default;

        int standardHandle = stream switch
        {
            TerminalStandardStream.Output => StandardOutputHandle,
            TerminalStandardStream.Error => StandardErrorHandle,
            TerminalStandardStream.Input => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(stream)),
        };

        if (standardHandle == 0)
        {
            return false;
        }

        nint handle = NativeMethods.GetStdHandle(standardHandle);
        if ((handle == IntPtr.Zero)
            || (handle == new IntPtr(-1)))
        {
            return false;
        }

        if (!NativeMethods.GetConsoleScreenBufferInfo(
                handle,
                out ConsoleScreenBufferInfo info))
        {
            return false;
        }

        int columns = info.Window.Right - info.Window.Left + 1;
        int rows = info.Window.Bottom - info.Window.Top + 1;
        if ((columns <= 0) || (rows <= 0))
        {
            return false;
        }

        size = new TerminalSize(columns, rows);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ConsoleScreenBufferInfo
    {
        public Coord Size;
        public Coord CursorPosition;
        public ushort Attributes;
        public SmallRect Window;
        public Coord MaximumWindowSize;
    }

#pragma warning disable SYSLIB1054 // Keep this small blittable interop surface free of generated unsafe code.
    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern nint GetStdHandle(int standardHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetConsoleScreenBufferInfo(
            nint consoleOutput,
            out ConsoleScreenBufferInfo consoleScreenBufferInfo);
    }
#pragma warning restore SYSLIB1054
}
