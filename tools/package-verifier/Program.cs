using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Icod.TermInfo.PackageVerifier;

internal static class Program
{
    private const string PackageId = "Icod.TermInfo";
    private const string RepositoryUrl =
        "https://github.com/uniblab/Icod.TermInfo";
    private const string ExpectedAssemblyVersion = "1.0.0.0";

    private static readonly string[] TargetFrameworks =
    [
        "net8.0",
        "net9.0",
        "net10.0",
    ];

    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length > 1)
        {
            Console.Error.WriteLine(
                "Usage: dotnet run --project tools/package-verifier/"
                + "Icod.TermInfo.PackageVerifier.csproj -- [artifact-directory]");
            return 2;
        }

        try
        {
            string root = FindRepositoryRoot();
            string artifactDirectory =
                args.Length == 0
                    ? Path.Combine(root, "artifacts")
                    : Path.GetFullPath(args[0], root);

            Directory.CreateDirectory(artifactDirectory);

            string packageVersion = ReadAndValidatePackageVersion(root);
            string nupkg =
                Path.Combine(
                    artifactDirectory,
                    $"{PackageId}.{packageVersion}.nupkg");
            string snupkg =
                Path.Combine(
                    artifactDirectory,
                    $"{PackageId}.{packageVersion}.snupkg");

            Require(File.Exists(nupkg), $"Package not found: {nupkg}");
            Require(File.Exists(snupkg), $"Symbol package not found: {snupkg}");

            VerifyParameterizationArchitecture(root);
            string commit = VerifyPrimaryPackage(nupkg, packageVersion);
            VerifySymbolPackage(snupkg, commit);

            Console.WriteLine(
                "Verified multi-target package structure, assembly identity, "
                + "package metadata, dependency closure, symbols, and "
                + $"Source Link for {packageVersion}.");
            return 0;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException
            or XmlException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static string FindRepositoryRoot()
    {
        string[] starts =
        [
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory,
        ];

        foreach (string start in starts)
        {
            DirectoryInfo? current = new(start);
            while (current is not null)
            {
                if (File.Exists(
                        Path.Combine(
                            current.FullName,
                            "Icod.TermInfo.csproj"))
                    && Directory.Exists(
                        Path.Combine(
                            current.FullName,
                            "src")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }

    private static string ReadAndValidatePackageVersion(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        XDocument project =
            XDocument.Load(
                Path.Combine(root, "Icod.TermInfo.csproj"),
                LoadOptions.None);

        string? version =
            project
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "Version")
                ?.Value
                .Trim();
        string? packageVersion =
            project
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "PackageVersion")
                ?.Value
                .Trim();

        Require(
            !string.IsNullOrWhiteSpace(version)
                && !string.IsNullOrWhiteSpace(packageVersion)
                && string.Equals(
                    version,
                    packageVersion,
                    StringComparison.Ordinal),
            "Version and PackageVersion must both be present and identical.");

        return packageVersion!;
    }

    private static void VerifyParameterizationArchitecture(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        string parameterizationRoot =
            Path.Combine(root, "src", "Parameterization");
        Require(
            Directory.Exists(parameterizationRoot),
            "The parameterization source directory is missing.");

        foreach (
            string file
            in Directory.EnumerateFiles(
                parameterizationRoot,
                "*",
                SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            if (text.Contains("TerminalProfiles.", StringComparison.Ordinal)
                || text.Contains("TerminalProfile", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The generic parameterization layer contains a "
                    + $"terminal-profile-specific reference in {file}.");
            }
        }
    }

    private static string VerifyPrimaryPackage(
        string packagePath,
        string expectedVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);

        using ZipArchive package =
            ZipFile.OpenRead(packagePath);

        HashSet<string> names =
            package.Entries
                .Select(entry => entry.FullName)
                .ToHashSet(StringComparer.Ordinal);

        List<string> required =
        [
            "README.md",
            "icon.png",
        ];

        foreach (string targetFramework in TargetFrameworks)
        {
            required.Add(
                $"lib/{targetFramework}/Icod.TermInfo.dll");
            required.Add(
                $"lib/{targetFramework}/Icod.TermInfo.xml");
        }

        string[] missing =
            required
                .Where(name => !names.Contains(name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        Require(
            missing.Length == 0,
            "Primary package is missing required entries: "
                + string.Join(", ", missing));

        Require(
            !names.Any(
                name =>
                    name.StartsWith(
                        "runtimes/",
                        StringComparison.Ordinal)),
            "Primary package unexpectedly contains a runtimes/ payload.");
        Require(
            !names.Any(
                name =>
                    ($"/{name.ToLowerInvariant()}/")
                        .Contains("/native/", StringComparison.Ordinal)),
            "Primary package unexpectedly contains a native payload directory.");

        string[] dlls =
            names
                .Where(
                    name =>
                        name.EndsWith(
                            ".dll",
                            StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        string[] expectedDlls =
            TargetFrameworks
                .Select(
                    targetFramework =>
                        $"lib/{targetFramework}/Icod.TermInfo.dll")
                .OrderBy(
                    name => name,
                    StringComparer.Ordinal)
                .ToArray();
        Require(
            dlls.SequenceEqual(
                expectedDlls,
                StringComparer.Ordinal),
            "Primary package contains unexpected DLL payloads: "
                + string.Join(", ", dlls));

        foreach (string targetFramework in TargetFrameworks)
        {
            VerifyAssemblyIdentity(
                package,
                targetFramework);
            VerifyDocumentation(
                package,
                targetFramework);
        }

        Require(
            !names.Any(HasNativeLibraryExtension),
            "Primary package unexpectedly contains a native library payload.");
        Require(
            !names.Any(IsRepositoryOnlyEntry),
            "Primary package unexpectedly contains repository-only "
                + "fixture/tooling data.");

        ZipArchiveEntry[] nuspecs =
            package.Entries
                .Where(
                    entry =>
                        entry.FullName.EndsWith(
                            ".nuspec",
                            StringComparison.OrdinalIgnoreCase))
                .ToArray();
        Require(
            nuspecs.Length == 1,
            $"Expected one nuspec, found {nuspecs.Length}.");

        using Stream nuspecStream = nuspecs[0].Open();
        XDocument nuspec =
            XDocument.Load(nuspecStream, LoadOptions.None);
        XElement? metadata =
            nuspec
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "metadata");
        Require(metadata is not null, "Package nuspec has no metadata element.");

        Require(
            GetMetadataText(metadata!, "id") == PackageId,
            "Unexpected package id.");
        Require(
            GetMetadataText(metadata!, "version") == expectedVersion,
            "Unexpected package version.");
        Require(
            GetMetadataText(metadata!, "title") == PackageId,
            "Unexpected package title.");
        Require(
            GetMetadataText(metadata!, "authors") == "Timothy J. Bruce",
            "Unexpected package authors.");
        Require(
            GetMetadataText(metadata!, "projectUrl") == RepositoryUrl,
            "Unexpected package project URL.");
        Require(
            GetMetadataText(metadata!, "readme") == "README.md",
            "Package metadata does not identify README.md.");
        Require(
            GetMetadataText(metadata!, "icon") == "icon.png",
            "Package metadata does not identify icon.png.");
        Require(
            string.Equals(
                GetMetadataText(
                    metadata!,
                    "requireLicenseAcceptance"),
                "true",
                StringComparison.OrdinalIgnoreCase),
            "Package must require license acceptance.");
        Require(
            !string.IsNullOrWhiteSpace(
                GetMetadataText(
                    metadata!,
                    "description")),
            "Package description is missing.");
        Require(
            !string.IsNullOrWhiteSpace(
                GetMetadataText(
                    metadata!,
                    "tags")),
            "Package tags are missing.");

        XElement? license =
            metadata!
                .Elements()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "license");
        Require(
            license is not null,
            "Package metadata has no license element.");
        Require(
            license!.Attribute("type")?.Value == "expression",
            "Package license is not an expression.");
        Require(
            license.Value == "LGPL-3.0-or-later",
            "Unexpected package license expression.");

        Require(
            !metadata!
                .Descendants()
                .Any(
                    element =>
                        element.Name.LocalName == "dependency"),
            "Icod.TermInfo must not have runtime NuGet dependencies.");

        XElement? repository =
            metadata!
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "repository");
        Require(repository is not null, "Package metadata has no repository element.");
        Require(
            repository!.Attribute("type")?.Value == "git",
            "Repository metadata is not git.");
        Require(
            repository.Attribute("url")?.Value == RepositoryUrl,
            "Unexpected repository URL in package metadata.");

        string commit =
            repository.Attribute("commit")?.Value ?? string.Empty;
        Require(
            Regex.IsMatch(
                commit,
                "^[0-9a-fA-F]{40}$",
                RegexOptions.CultureInvariant),
            $"Repository metadata has an invalid commit id: '{commit}'.");

        return commit;
    }

    private static void VerifyDocumentation(
        ZipArchive package,
        string targetFramework)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFramework);

        string documentationPath =
            $"lib/{targetFramework}/Icod.TermInfo.xml";
        ZipArchiveEntry? entry =
            package.GetEntry(
                documentationPath);
        Require(
            entry is not null,
            $"Primary package is missing {documentationPath}.");
        Require(
            entry!.Length > 0,
            $"{documentationPath} is empty.");

        using Stream stream =
            entry.Open();
        XDocument documentation =
            XDocument.Load(
                stream,
                LoadOptions.None);

        string? assemblyName =
            documentation
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "assembly")
                ?.Elements()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "name")
                ?.Value;

        Require(
            assemblyName == PackageId,
            $"{documentationPath} identifies unexpected assembly "
                + $"'{assemblyName}'.");

        int memberCount =
            documentation
                .Descendants()
                .Count(
                    element =>
                        element.Name.LocalName == "member");

        Require(
            memberCount > 0,
            $"{documentationPath} contains no documented members.");
    }

    private static void VerifyAssemblyIdentity(
        ZipArchive package,
        string targetFramework)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFramework);

        string assemblyPath =
            $"lib/{targetFramework}/Icod.TermInfo.dll";
        ZipArchiveEntry? entry =
            package.GetEntry(assemblyPath);
        Require(
            entry is not null,
            $"Primary package is missing {assemblyPath}.");

        string temporaryPath =
            Path.Combine(
                Path.GetTempPath(),
                "Icod.TermInfo-package-verifier-"
                + Guid.NewGuid().ToString("N")
                + ".dll");

        try
        {
            using (Stream source = entry!.Open())
            using (FileStream destination = File.Create(temporaryPath))
            {
                source.CopyTo(destination);
            }

            AssemblyName assemblyName =
                AssemblyName.GetAssemblyName(
                    temporaryPath);

            Require(
                assemblyName.Name == PackageId,
                $"{assemblyPath} has unexpected assembly name "
                    + $"'{assemblyName.Name}'.");
            Require(
                assemblyName.Version?.ToString() == ExpectedAssemblyVersion,
                $"{assemblyPath} has assembly version "
                    + $"'{assemblyName.Version}', expected "
                    + $"{ExpectedAssemblyVersion}.");

            byte[]? publicKeyToken =
                assemblyName.GetPublicKeyToken();
            Require(
                publicKeyToken is null
                    || publicKeyToken.Length == 0,
                $"{assemblyPath} is unexpectedly strong-name signed.");
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static void VerifySymbolPackage(
        string packagePath,
        string commit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(commit);

        using ZipArchive symbols =
            ZipFile.OpenRead(packagePath);

        foreach (string targetFramework in TargetFrameworks)
        {
            string pdbPath =
                $"lib/{targetFramework}/Icod.TermInfo.pdb";
            ZipArchiveEntry? pdbEntry =
                symbols.GetEntry(pdbPath);
            Require(
                pdbEntry is not null,
                $"Symbol package is missing {pdbPath}.");

            using Stream stream = pdbEntry!.Open();
            using MemoryStream buffer = new();
            stream.CopyTo(buffer);
            byte[] pdb = buffer.ToArray();

            Require(
                pdb.AsSpan().StartsWith("BSJB"u8),
                $"{pdbPath} is not a portable PDB.");
            Require(
                ContainsAscii(
                    pdb,
                    "raw.githubusercontent.com/uniblab/Icod.TermInfo/"),
                $"{pdbPath} does not contain the expected GitHub "
                    + "Source Link mapping.");
            Require(
                ContainsAscii(pdb, commit),
                $"{pdbPath} Source Link data does not contain the package "
                    + "repository commit.");
        }
    }

    private static string? GetMetadataText(
        XElement metadata,
        string name)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return metadata
            .Elements()
            .FirstOrDefault(
                element =>
                    element.Name.LocalName == name)
            ?.Value;
    }

    private static bool HasNativeLibraryExtension(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.EndsWith(".so", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".a", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".lib", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRepositoryOnlyEntry(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        string lower = name.ToLowerInvariant();
        return name.StartsWith("tests/", StringComparison.Ordinal)
            || name.StartsWith("tools/", StringComparison.Ordinal)
            || name.StartsWith("fixtures/", StringComparison.Ordinal)
            || lower.Contains("compiled-terminfo", StringComparison.Ordinal)
            || lower.EndsWith(".ti", StringComparison.Ordinal)
            || lower.EndsWith(".bin", StringComparison.Ordinal);
    }

    private static bool ContainsAscii(
        byte[] data,
        string text)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(text);

        byte[] expected = Encoding.ASCII.GetBytes(text);
        return data.AsSpan().IndexOf(expected) >= 0;
    }

    private static void Require(
        bool condition,
        string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
