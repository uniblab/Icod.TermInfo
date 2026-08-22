using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class WindowsVirtualTerminalTests
{
    [Fact]
    public void InputStreamIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Input));
    }

    [Fact]
    public void UndefinedStreamIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => WindowsVirtualTerminal.TryEnableOutput(
                (TerminalStandardStream)999));
    }

    [Fact]
    public void CoreRejectsNullConsoleModeApi()
    {
        Assert.Throws<ArgumentNullException>(
            () => WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                null!));
    }

    [Fact]
    public void UnavailableStandardHandleReturnsNull()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            StandardHandleAvailable = false,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.Null(lease);
        Assert.Equal<TerminalStandardStream?>(
            TerminalStandardStream.Output,
            api.LastRequestedStream);
        Assert.Equal(0, api.GetModeCallCount);
        Assert.Empty(api.SetModeCalls);
    }

    [Fact]
    public void NonConsoleHandleReturnsNull()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            ConsoleModeAvailable = false,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.Null(lease);
        Assert.Equal(1, api.GetModeCallCount);
        Assert.Empty(api.SetModeCalls);
    }

    [Fact]
    public void FailedModeChangeReturnsNull()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            OriginalMode = 0x0013u,
            SetModeSucceeds = false,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.Null(lease);
        Assert.Single(api.SetModeCalls);
        Assert.Equal(
            0x0017u,
            api.SetModeCalls[0].Mode);
    }

    [Fact]
    public void EnablementPreservesUnrelatedFlagsAndDisposeRestoresOriginalMode()
    {
        const uint originalMode = 0x0092u;
        FakeWindowsConsoleModeApi api = new()
        {
            OriginalMode = originalMode,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.NotNull(lease);
        Assert.Single(api.SetModeCalls);
        Assert.Equal(
            originalMode
                | WindowsVirtualTerminal.RequiredOutputModeFlags,
            api.SetModeCalls[0].Mode);

        lease!.Dispose();

        Assert.Equal(2, api.SetModeCalls.Count);
        Assert.Equal(
            originalMode,
            api.SetModeCalls[1].Mode);
    }

    [Fact]
    public void AlreadyEnabledModeIsNotRewrittenOrRestored()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            OriginalMode =
                0x0012u
                | WindowsVirtualTerminal.RequiredOutputModeFlags,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.NotNull(lease);
        Assert.Empty(api.SetModeCalls);

        lease!.Dispose();

        Assert.Empty(api.SetModeCalls);
    }

    [Fact]
    public void DisposeRestoresAtMostOnce()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            OriginalMode = 0x0001u,
        };

        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Output,
                api);

        Assert.NotNull(lease);
        Assert.Single(api.SetModeCalls);

        lease!.Dispose();
        lease.Dispose();

        Assert.Equal(2, api.SetModeCalls.Count);
        Assert.Equal(
            0x0001u,
            api.SetModeCalls[1].Mode);
    }

    [Fact]
    public void StandardErrorUsesErrorHandle()
    {
        FakeWindowsConsoleModeApi api = new()
        {
            OriginalMode = 0x0001u,
        };

        using IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput(
                TerminalStandardStream.Error,
                api);

        Assert.NotNull(lease);
        Assert.Equal<TerminalStandardStream?>(
            TerminalStandardStream.Error,
            api.LastRequestedStream);
    }

    [Fact]
    public void PublicHelperFailsGracefullyWithoutUsableWindowsConsole()
    {
        IDisposable? lease =
            WindowsVirtualTerminal.TryEnableOutput();

        try
        {
            if (!OperatingSystem.IsWindows()
                || TerminalEnvironment.IsOutputRedirected)
            {
                Assert.Null(lease);
            }
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private sealed class FakeWindowsConsoleModeApi
        : IWindowsConsoleModeApi
    {
        internal bool StandardHandleAvailable { get; init; } = true;

        internal bool ConsoleModeAvailable { get; init; } = true;

        internal bool SetModeSucceeds { get; init; } = true;

        internal nint Handle { get; init; } = new IntPtr(1234);

        internal uint OriginalMode { get; init; }

        internal TerminalStandardStream? LastRequestedStream { get; private set; }

        internal int GetModeCallCount { get; private set; }

        internal List<(nint Handle, uint Mode)> SetModeCalls { get; } = [];

        public bool TryGetStandardHandle(
            TerminalStandardStream stream,
            out nint handle)
        {
            LastRequestedStream = stream;

            if (!StandardHandleAvailable)
            {
                handle = default;
                return false;
            }

            handle = Handle;
            return true;
        }

        public bool TryGetConsoleMode(
            nint handle,
            out uint mode)
        {
            GetModeCallCount++;

            if (!ConsoleModeAvailable)
            {
                mode = default;
                return false;
            }

            mode = OriginalMode;
            return true;
        }

        public bool TrySetConsoleMode(
            nint handle,
            uint mode)
        {
            SetModeCalls.Add((handle, mode));
            return SetModeSucceeds;
        }
    }
}
