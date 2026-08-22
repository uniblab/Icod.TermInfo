using System.Runtime.InteropServices;

namespace Icod.TermInfo;

internal sealed class UnixTerminalSizeProvider : ITerminalSizeProvider
{
    private readonly nuint _getWindowSizeRequest;

    internal UnixTerminalSizeProvider(nuint getWindowSizeRequest)
    {
        if (getWindowSizeRequest == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(getWindowSizeRequest));
        }

        _getWindowSizeRequest = getWindowSizeRequest;
    }

    public bool TryGetSize(
        TerminalStandardStream stream,
        out TerminalSize size)
    {
        if (!Enum.IsDefined(typeof(TerminalStandardStream), stream))
        {
            throw new ArgumentOutOfRangeException(nameof(stream));
        }

        size = default;

        int fileDescriptor = stream switch
        {
            TerminalStandardStream.Input => 0,
            TerminalStandardStream.Output => 1,
            TerminalStandardStream.Error => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(stream)),
        };

        if (NativeMethods.Ioctl(
                fileDescriptor,
                _getWindowSizeRequest,
                out WindowSize windowSize) != 0)
        {
            return false;
        }

        if ((windowSize.Columns == 0) || (windowSize.Rows == 0))
        {
            return false;
        }

        size = new TerminalSize(
            windowSize.Columns,
            windowSize.Rows);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowSize
    {
        public ushort Rows;
        public ushort Columns;
        public ushort XPixel;
        public ushort YPixel;
    }

#pragma warning disable SYSLIB1054 // Keep this small blittable interop surface free of generated unsafe code.
    private static class NativeMethods
    {
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        internal static extern int Ioctl(
            int fileDescriptor,
            nuint request,
            out WindowSize windowSize);
    }
#pragma warning restore SYSLIB1054
}
