namespace Icod.TermInfo;

internal enum TerminalHostPlatform
{
    Windows,
    Linux,
    MacOS,
    Other,
}

internal sealed class SystemTerminalDiscoverySnapshot
{
    internal SystemTerminalDiscoverySnapshot(
        string? termInfo,
        string? termInfoDirs,
        string? homeDirectory,
        string currentDirectory,
        TerminalHostPlatform platform)
    {
        ArgumentNullException.ThrowIfNull(currentDirectory);

        if (string.IsNullOrWhiteSpace(currentDirectory))
        {
            throw new ArgumentException(
                "The current directory snapshot cannot be empty or whitespace.",
                nameof(currentDirectory));
        }

        if (!Path.IsPathFullyQualified(currentDirectory))
        {
            throw new ArgumentException(
                "The current directory snapshot must be fully qualified.",
                nameof(currentDirectory));
        }

        TermInfo = termInfo;
        TermInfoDirs = termInfoDirs;
        HomeDirectory =
            string.IsNullOrEmpty(homeDirectory)
                ? null
                : homeDirectory;
        CurrentDirectory =
            Path.GetFullPath(currentDirectory);
        Platform = platform;
    }

    internal string? TermInfo { get; }

    internal string? TermInfoDirs { get; }

    internal string? HomeDirectory { get; }

    internal string CurrentDirectory { get; }

    internal TerminalHostPlatform Platform { get; }

    internal static SystemTerminalDiscoverySnapshot Capture(
        SystemTerminalDescriptionProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return Capture(
            options,
            Environment.GetEnvironmentVariable,
            () => Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            () => Environment.CurrentDirectory,
            DetectPlatform);
    }

    internal static SystemTerminalDiscoverySnapshot Capture(
        SystemTerminalDescriptionProviderOptions options,
        Func<string, string?> environmentReader,
        Func<string?> homeDirectoryReader,
        Func<string> currentDirectoryReader,
        Func<TerminalHostPlatform> platformReader)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environmentReader);
        ArgumentNullException.ThrowIfNull(homeDirectoryReader);
        ArgumentNullException.ThrowIfNull(currentDirectoryReader);
        ArgumentNullException.ThrowIfNull(platformReader);

        string? termInfo = null;
        string? termInfoDirs = null;

        if (options.UseEnvironment)
        {
            termInfo =
                environmentReader(
                    "TERMINFO");
            termInfoDirs =
                environmentReader(
                    "TERMINFO_DIRS");
        }

        string? homeDirectory =
            options.UseUserDatabase
                ? homeDirectoryReader()
                : null;

        return new SystemTerminalDiscoverySnapshot(
            termInfo,
            termInfoDirs,
            homeDirectory,
            currentDirectoryReader(),
            platformReader());
    }

    private static TerminalHostPlatform DetectPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return TerminalHostPlatform.Windows;
        }

        if (OperatingSystem.IsLinux())
        {
            return TerminalHostPlatform.Linux;
        }

        if (OperatingSystem.IsMacOS())
        {
            return TerminalHostPlatform.MacOS;
        }

        return TerminalHostPlatform.Other;
    }
}
