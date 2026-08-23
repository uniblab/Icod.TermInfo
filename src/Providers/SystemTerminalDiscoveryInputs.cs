using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

internal static class SystemTerminalDiscoveryInputs
{
    private const string HexPrefix = "hex:";
    private const string Base64Prefix = "b64:";

    internal static bool TryLoadEncodedTermInfo(
        string? termInfo,
        string requestedName,
        CompiledTermInfoParserOptions parserOptions,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ArgumentNullException.ThrowIfNull(requestedName);
        ArgumentNullException.ThrowIfNull(parserOptions);

        if (string.IsNullOrWhiteSpace(requestedName))
        {
            throw new ArgumentException(
                "The requested terminal name cannot be empty or whitespace.",
                nameof(requestedName));
        }

        byte[] entry;

        if (termInfo is not null
            && termInfo.StartsWith(
                HexPrefix,
                StringComparison.Ordinal))
        {
            entry =
                DecodeHex(
                    termInfo[HexPrefix.Length..],
                    parserOptions.MaximumEntrySize);
        }
        else if (termInfo is not null
            && termInfo.StartsWith(
                Base64Prefix,
                StringComparison.Ordinal))
        {
            entry =
                DecodeBase64(
                    termInfo[Base64Prefix.Length..],
                    parserOptions.MaximumEntrySize);
        }
        else
        {
            terminal = null;
            return false;
        }

        TerminalDescription parsed =
            CompiledTermInfoParser.Parse(
                entry,
                parserOptions);

        if (!MatchesIdentity(
                requestedName,
                parsed))
        {
            terminal = null;
            return false;
        }

        terminal = parsed;
        return true;
    }

    internal static string[] SplitTermInfoDirs(
        string value,
        TerminalHostPlatform platform)
    {
        ArgumentNullException.ThrowIfNull(value);

        char separator =
            (platform == TerminalHostPlatform.Windows)
                ? ';'
                : ':'
        ;

        return value.Split(
            separator,
            StringSplitOptions.None);
    }

    internal static IReadOnlyList<string> ResolveTermInfoDirs(
        SystemTerminalDiscoverySnapshot snapshot,
        IReadOnlyList<string> defaultRoots)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(defaultRoots);

        if (snapshot.TermInfoDirs is null)
        {
            return Array.Empty<string>();
        }

        string[] components =
            SplitTermInfoDirs(
                snapshot.TermInfoDirs,
                snapshot.Platform);
        StringComparer comparer =
            (snapshot.Platform == TerminalHostPlatform.Windows)
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal
        ;
        HashSet<string> seen =
            new(comparer);
        List<string> roots = [];

        foreach (string component in components)
        {
            if (component.Length == 0)
            {
                foreach (string defaultRoot in defaultRoots)
                {
                    AddRoot(
                        defaultRoot,
                        snapshot.CurrentDirectory,
                        seen,
                        roots);
                }

                continue;
            }

            AddRoot(
                component,
                snapshot.CurrentDirectory,
                seen,
                roots);
        }

        return roots;
    }

    private static byte[] DecodeHex(
        string payload,
        int maximumEntrySize)
    {
        if ((payload.Length & 1) != 0)
        {
            throw new FormatException(
                "The hex-encoded TERMINFO payload must contain an even number of hexadecimal digits.");
        }

        int decodedLength =
            payload.Length / 2;
        EnsureDecodedSize(
            decodedLength,
            maximumEntrySize,
            "hex");

        byte[] result =
            new byte[decodedLength];

        for (int index = 0;
            index < decodedLength;
            index++)
        {
            int high =
                GetHexValue(
                    payload[index * 2]);
            int low =
                GetHexValue(
                    payload[(index * 2) + 1]);

            if (high < 0
                || low < 0)
            {
                throw new FormatException(
                    "The hex-encoded TERMINFO payload contains a non-hexadecimal character.");
            }

            result[index] =
                (byte)((high << 4) | low);
        }

        return result;
    }

    private static byte[] DecodeBase64(
        string payload,
        int maximumEntrySize)
    {
        int firstPadding =
            payload.IndexOf('=');
        int explicitPadding = 0;
        int dataLength =
            payload.Length;

        if (firstPadding >= 0)
        {
            dataLength = firstPadding;
            explicitPadding =
                payload.Length
                - firstPadding;

            if (explicitPadding > 2
                || (payload.Length & 3) != 0)
            {
                throw new FormatException(
                    "The base64-encoded TERMINFO payload has invalid padding.");
            }

            for (int index = firstPadding;
                index < payload.Length;
                index++)
            {
                if (payload[index] != '=')
                {
                    throw new FormatException(
                        "The base64-encoded TERMINFO payload has invalid padding.");
                }
            }
        }

        for (int index = 0;
            index < dataLength;
            index++)
        {
            if (!IsBase64DataCharacter(
                    payload[index]))
            {
                throw new FormatException(
                    "The base64-encoded TERMINFO payload contains an invalid character.");
            }
        }

        int implicitPadding = 0;

        if (explicitPadding == 0)
        {
            int remainder =
                payload.Length & 3;

            if (remainder == 1)
            {
                throw new FormatException(
                    "The base64-encoded TERMINFO payload has an invalid length.");
            }

            if (remainder != 0)
            {
                implicitPadding =
                    4 - remainder;
            }
        }

        long normalizedLength =
            (long)payload.Length
            + implicitPadding;
        long decodedLength =
            ((normalizedLength / 4) * 3)
            - explicitPadding
            - implicitPadding;

        if (decodedLength < 0)
        {
            throw new FormatException(
                "The base64-encoded TERMINFO payload has invalid padding.");
        }

        EnsureDecodedSize(
            decodedLength,
            maximumEntrySize,
            "base64");

        if (normalizedLength > int.MaxValue)
        {
            throw new FormatException(
                "The base64-encoded TERMINFO payload is too large to decode.");
        }

        char[] normalized =
            new char[(int)normalizedLength];

        for (int index = 0;
            index < payload.Length;
            index++)
        {
            normalized[index] =
                payload[index] switch
                {
                    '-' => '+',
                    '_' => '/',
                    _ => payload[index],
                };
        }

        for (int index = payload.Length;
            index < normalized.Length;
            index++)
        {
            normalized[index] = '=';
        }

        try
        {
            byte[] result =
                Convert.FromBase64String(
                    new string(normalized));

            if (result.LongLength != decodedLength)
            {
                throw new FormatException(
                    "The base64-encoded TERMINFO payload decoded to an unexpected length.");
            }

            return result;
        }
        catch (FormatException exception)
        {
            throw new FormatException(
                "The base64-encoded TERMINFO payload is malformed.",
                exception);
        }
    }

    private static void EnsureDecodedSize(
        long decodedLength,
        int maximumEntrySize,
        string encoding)
    {
        if (decodedLength > maximumEntrySize)
        {
            throw new FormatException(
                $"The {encoding}-encoded TERMINFO payload decodes to {decodedLength} bytes, exceeding the configured maximum of {maximumEntrySize} bytes.");
        }
    }

    private static int GetHexValue(char value)
    {
        if (value >= '0'
            && value <= '9')
        {
            return value - '0';
        }

        if (value >= 'a'
            && value <= 'f')
        {
            return value - 'a' + 10;
        }

        if (value >= 'A'
            && value <= 'F')
        {
            return value - 'A' + 10;
        }

        return -1;
    }

    private static bool IsBase64DataCharacter(
        char value)
    {
        return (value >= 'A'
                && value <= 'Z')
            || (value >= 'a'
                && value <= 'z')
            || (value >= '0'
                && value <= '9')
            || value == '+'
            || value == '/'
            || value == '-'
            || value == '_';
    }

    private static bool MatchesIdentity(
        string requestedName,
        TerminalDescription terminal)
    {
        if (string.Equals(
                requestedName,
                terminal.Name,
                StringComparison.Ordinal))
        {
            return true;
        }

        foreach (string alias in terminal.Aliases)
        {
            if (string.Equals(
                    requestedName,
                    alias,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddRoot(
        string root,
        string currentDirectory,
        ISet<string> seen,
        ICollection<string> roots)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(currentDirectory);
        ArgumentNullException.ThrowIfNull(seen);
        ArgumentNullException.ThrowIfNull(roots);

        if (root.Length == 0)
        {
            throw new ArgumentException(
                "A configured default terminfo root cannot be empty.",
                nameof(root));
        }

        string fullPath =
            Path.GetFullPath(
                root,
                currentDirectory);

        if (seen.Add(fullPath))
        {
            roots.Add(fullPath);
        }
    }
}
