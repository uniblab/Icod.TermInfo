using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Source;

/// <summary>
/// Resolves terminfo source <c>use=</c> inheritance into deterministic semantic
/// capability state.
/// </summary>
public static class TermInfoSourceResolver
{
    /// <summary>
    /// Resolves one named entry from a parsed source document.
    /// </summary>
    /// <remarks>
    /// Canonical names and aliases are matched case-sensitively in document
    /// order. If duplicate names are present, the first matching entry is used;
    /// duplicate-name diagnostics remain S09 work.
    /// </remarks>
    public static TermInfoSourceResolveResult Resolve(
        TermInfoSourceDocument document,
        string name,
        TermInfoSourceResolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Resolve(
            new DocumentEntryProvider(document),
            name,
            options);
    }

    /// <summary>
    /// Resolves one named entry through a caller-supplied source-entry provider.
    /// </summary>
    /// <remarks>
    /// Provider exceptions propagate to the caller. A clean provider miss is
    /// represented by a source diagnostic rather than an exception.
    /// </remarks>
    public static TermInfoSourceResolveResult Resolve(
        ITermInfoSourceEntryProvider provider,
        string name,
        TermInfoSourceResolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ResolutionContext context =
            new(
                provider,
                options ?? new TermInfoSourceResolverOptions());
        ResolvedNode? node =
            context.ResolveNamed(
                name,
                0,
                null);
        IReadOnlyList<TermInfoSourceDiagnostic> diagnostics =
            context.GetOrderedDiagnostics();

        if (node is null
            || diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity
                        == TermInfoSourceDiagnosticSeverity.Error))
        {
            return new TermInfoSourceResolveResult(
                null,
                diagnostics);
        }

        return new TermInfoSourceResolveResult(
            new TermInfoSourceResolvedEntry(
                node.Entry,
                node.State),
            diagnostics);
    }

    private sealed class ResolutionContext
    {
        private readonly ITermInfoSourceEntryProvider _provider;
        private readonly TermInfoSourceResolverOptions _options;
        private readonly Dictionary<ResolutionCacheKey, ResolvedNode> _cache = [];
        private readonly HashSet<string> _activeNames =
            new(StringComparer.Ordinal);
        private readonly List<string> _activePath = [];
        private readonly List<TermInfoSourceDiagnostic> _diagnostics = [];

        internal ResolutionContext(
            ITermInfoSourceEntryProvider provider,
            TermInfoSourceResolverOptions options)
        {
            ArgumentNullException.ThrowIfNull(provider);
            ArgumentNullException.ThrowIfNull(options);

            _provider = provider;
            _options = options;
        }

        internal ResolvedNode? ResolveNamed(
            string requestedName,
            int depth,
            TermInfoSourceSpan? referenceSpan)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestedName);

            if (depth > _options.MaximumInheritanceDepth)
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
                    $"Maximum inheritance depth {_options.MaximumInheritanceDepth} was exceeded while resolving '{requestedName}'.",
                    referenceSpan);
                return null;
            }

            bool found =
                _provider.TryLoad(
                    requestedName,
                    out TermInfoSourceEntry? entry);
            ValidateProviderResult(
                found,
                entry,
                requestedName);

            if (!found)
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.MissingSourceEntry,
                    $"Source entry '{requestedName}' could not be found.",
                    referenceSpan);
                return null;
            }

            TermInfoSourceEntry loadedEntry =
                entry
                ?? throw new InvalidOperationException(
                    $"The source-entry provider returned no entry for '{requestedName}'.");
            string canonicalName =
                loadedEntry.CanonicalName;
            if (_activeNames.Contains(canonicalName))
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.InheritanceCycle,
                    CreateCycleMessage(canonicalName),
                    referenceSpan);
                return null;
            }

            ResolutionCacheKey cacheKey =
                new(
                    canonicalName,
                    depth);
            if (_cache.TryGetValue(
                    cacheKey,
                    out ResolvedNode cached))
            {
                return cached.Clone();
            }

            _activeNames.Add(canonicalName);
            _activePath.Add(canonicalName);

            try
            {
                TermInfoSourceCapabilityState local =
                    TermInfoSourceCapabilityState.CreateLocal(loadedEntry);
                TermInfoSourceCapabilityState parents =
                    TermInfoSourceCapabilityState.CreateEmpty();
                bool parentFailed = false;

                for (int index = loadedEntry.Fields.Count - 1; index >= 0; index--)
                {
                    TermInfoSourceField field =
                        loadedEntry.Fields[index];
                    if (field.Kind != TermInfoSourceFieldKind.UseReference)
                    {
                        continue;
                    }

                    string? referenceName =
                        field.ReferenceName;
                    if (string.IsNullOrWhiteSpace(referenceName))
                    {
                        AddDiagnostic(
                            TermInfoSourceDiagnosticCodes.MissingUseReference,
                            "A use= field must identify a parent source entry.",
                            field.Span);
                        parentFailed = true;
                        continue;
                    }

                    ResolvedNode? parent =
                        ResolveNamed(
                            referenceName,
                            checked(depth + 1),
                            field.Span);
                    if (parent is null)
                    {
                        parentFailed = true;
                        continue;
                    }

                    parents.OverlayHigherPriority(parent.State);
                }

                if (parentFailed)
                {
                    return null;
                }

                local.Inherit(parents);
                ResolvedNode resolved =
                    new(
                        loadedEntry,
                        local);
                _cache[cacheKey] =
                    resolved.Clone();
                return resolved;
            }
            finally
            {
                _activePath.RemoveAt(
                    _activePath.Count - 1);
                _activeNames.Remove(canonicalName);
            }
        }

        internal IReadOnlyList<TermInfoSourceDiagnostic> GetOrderedDiagnostics()
        {
            return _diagnostics
                .Select(
                    (diagnostic, ordinal) =>
                        new
                        {
                            Diagnostic = diagnostic,
                            Ordinal = ordinal,
                        })
                .OrderBy(
                    item =>
                        item.Diagnostic.Span?.SourceName
                        ?? string.Empty,
                    StringComparer.Ordinal)
                .ThenBy(
                    item =>
                        item.Diagnostic.Span?.Offset
                        ?? int.MaxValue)
                .ThenBy(
                    item =>
                        item.Diagnostic.Span?.Length
                        ?? int.MaxValue)
                .ThenBy(item => item.Ordinal)
                .Select(item => item.Diagnostic)
                .ToArray();
        }

        private void AddDiagnostic(
            string code,
            string message,
            TermInfoSourceSpan? span)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            _diagnostics.Add(
                new TermInfoSourceDiagnostic(
                    code,
                    TermInfoSourceDiagnosticSeverity.Error,
                    message,
                    span));
        }

        private string CreateCycleMessage(
            string canonicalName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(canonicalName);

            int start =
                _activePath.FindIndex(
                    name =>
                        string.Equals(
                            name,
                            canonicalName,
                            StringComparison.Ordinal));
            IEnumerable<string> cycle;
            if (start < 0)
            {
                cycle =
                    _activePath.Append(canonicalName);
            }
            else
            {
                cycle =
                    _activePath
                        .Skip(start)
                        .Append(canonicalName);
            }

            return
                "Inheritance cycle detected: "
                + string.Join(
                    " -> ",
                    cycle)
                + ".";
        }

        private static void ValidateProviderResult(
            bool found,
            TermInfoSourceEntry? entry,
            string requestedName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestedName);

            if (found && entry is null)
            {
                throw new InvalidOperationException(
                    $"The source-entry provider reported success for '{requestedName}' but returned a null entry.");
            }

            if (!found && entry is not null)
            {
                throw new InvalidOperationException(
                    $"The source-entry provider reported a clean miss for '{requestedName}' but returned a non-null entry.");
            }
        }
    }

    private sealed class DocumentEntryProvider : ITermInfoSourceEntryProvider
    {
        private readonly TermInfoSourceDocument _document;

        internal DocumentEntryProvider(
            TermInfoSourceDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            _document = document;
        }

        public bool TryLoad(
            string name,
            [NotNullWhen(true)] out TermInfoSourceEntry? entry)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            foreach (TermInfoSourceEntry candidate in _document.Entries)
            {
                if (string.Equals(
                        candidate.CanonicalName,
                        name,
                        StringComparison.Ordinal)
                    || candidate.Aliases.Any(
                        alias =>
                            string.Equals(
                                alias,
                                name,
                                StringComparison.Ordinal)))
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }
    }

    private sealed class ResolvedNode
    {
        internal ResolvedNode(
            TermInfoSourceEntry entry,
            TermInfoSourceCapabilityState state)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(state);

            Entry = entry;
            State = state;
        }

        internal TermInfoSourceEntry Entry { get; }

        internal TermInfoSourceCapabilityState State { get; }

        internal ResolvedNode Clone()
        {
            return new ResolvedNode(
                Entry,
                State.Clone());
        }
    }

    private readonly record struct ResolutionCacheKey(
        string CanonicalName,
        int Depth);
}
