using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Resolves termcap <c>tc=</c> inheritance into deterministic effective source
/// fields without performing terminfo conversion.
/// </summary>
public static class TermcapSourceResolver
{
	/// <summary>
	/// Resolves one named entry from a parsed termcap source document.
	/// </summary>
	/// <remarks>
	/// Header components are matched case-sensitively in document order. TC03
	/// does not reinterpret the TC01 header list as canonical names, aliases, or
	/// prose descriptions. If more than one entry contains the requested header
	/// component, the first source entry is selected deterministically.
	/// </remarks>
	public static TermcapSourceResolveResult Resolve(
		TermcapSourceDocument document,
		string name,
		TermcapSourceResolverOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return Resolve(
			new DocumentEntryProvider( document ),
			name,
			options
		);
	}

	/// <summary>
	/// Resolves one named entry through a caller-supplied source-entry provider.
	/// </summary>
	/// <remarks>
	/// Provider exceptions propagate to the caller. A clean provider miss is
	/// represented by a source diagnostic rather than an exception.
	/// </remarks>
	public static TermcapSourceResolveResult Resolve(
		ITermcapSourceEntryProvider provider,
		string name,
		TermcapSourceResolverOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( provider );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		ResolutionContext context =
			new(
				provider,
				options ?? new TermcapSourceResolverOptions()
			);
		ResolvedNode? node =
			context.ResolveNamed(
				name,
				0,
				null
			);
		IReadOnlyList<TermcapSourceDiagnostic> diagnostics =
			context.GetOrderedDiagnostics();

		if (
			node is null
			|| diagnostics.Any(
				diagnostic =>
					diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
			)
		) {
			return new TermcapSourceResolveResult(
				null,
				diagnostics
			);
		}

		return new TermcapSourceResolveResult(
			new TermcapSourceResolvedEntry(
				node.Entry,
				node.Fields
			),
			diagnostics
		);
	}

	private sealed class ResolutionContext
	{
		private readonly ITermcapSourceEntryProvider _provider;
		private readonly TermcapSourceResolverOptions _options;
		private readonly HashSet<string> _activeNames =
			new( StringComparer.Ordinal );
		private readonly HashSet<TermcapSourceEntry> _activeEntries =
			new( ReferenceEqualityComparer.Instance );
		private readonly List<ActiveFrame> _activePath = [];
		private readonly List<TermcapSourceDiagnostic> _diagnostics = [];

		internal ResolutionContext(
			ITermcapSourceEntryProvider provider,
			TermcapSourceResolverOptions options
		) {
			ArgumentNullException.ThrowIfNull( provider );
			ArgumentNullException.ThrowIfNull( options );

			_provider = provider;
			_options = options;
		}

		internal ResolvedNode? ResolveNamed(
			string requestedName,
			int depth,
			TermcapSourceSpan? referenceSpan
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( requestedName );

			if ( depth > _options.MaximumInheritanceDepth ) {
				AddDiagnostic(
					TermcapSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
					$"Maximum inheritance depth {_options.MaximumInheritanceDepth} was exceeded while resolving '{requestedName}'.",
					referenceSpan
				);
				return null;
			}

			bool found =
				_provider.TryLoad(
					requestedName,
					out TermcapSourceEntry? entry
				);
			ValidateProviderResult(
				found,
				entry,
				requestedName
			);

			if ( !found ) {
				AddDiagnostic(
					TermcapSourceDiagnosticCodes.MissingSourceEntry,
					$"Termcap source entry '{requestedName}' could not be found.",
					referenceSpan
				);
				return null;
			}

			TermcapSourceEntry loadedEntry =
				entry
				?? throw new InvalidOperationException(
					$"The source-entry provider returned no entry for '{requestedName}'."
				);
			if (
				_activeNames.Contains( requestedName )
				|| _activeEntries.Contains( loadedEntry )
			) {
				AddDiagnostic(
					TermcapSourceDiagnosticCodes.InheritanceCycle,
					CreateCycleMessage(
						requestedName,
						loadedEntry
					),
					referenceSpan
				);
				return null;
			}

			_activeNames.Add( requestedName );
			_activeEntries.Add( loadedEntry );
			_activePath.Add(
				new ActiveFrame(
					requestedName,
					loadedEntry
				)
			);

			try {
				return ResolveLoadedEntry(
					loadedEntry,
					depth
				);
			} finally {
				_activePath.RemoveAt(
					_activePath.Count - 1
				);
				_activeEntries.Remove( loadedEntry );
				_activeNames.Remove( requestedName );
			}
		}

		internal IReadOnlyList<TermcapSourceDiagnostic> GetOrderedDiagnostics() {
			return _diagnostics
				.Select(
					( diagnostic, ordinal ) =>
						new {
							Diagnostic = diagnostic,
							Ordinal = ordinal,
						}
				)
				.OrderBy(
					item => item.Diagnostic.Span?.SourceName ?? string.Empty,
					StringComparer.Ordinal
				)
				.ThenBy(
					item => item.Diagnostic.Span?.Offset ?? int.MaxValue
				)
				.ThenBy(
					item => item.Diagnostic.Span?.Length ?? int.MaxValue
				)
				.ThenBy(
					item => item.Ordinal
				)
				.Select(
					item => item.Diagnostic
				)
				.ToArray();
		}

		private ResolvedNode? ResolveLoadedEntry(
			TermcapSourceEntry entry,
			int depth
		) {
			ArgumentNullException.ThrowIfNull( entry );
			ArgumentOutOfRangeException.ThrowIfNegative( depth );

			List<TermcapSourceResolvedField> fields = [];
			HashSet<string> claimedNames =
				new( StringComparer.Ordinal );
			TermcapSourceField? reference = null;

			foreach ( TermcapSourceField field in entry.Fields ) {
				if ( field.Kind == TermcapSourceFieldKind.Reference ) {
					reference ??= field;
					continue;
				}
				if ( field.Kind == TermcapSourceFieldKind.DisabledCapability ) {
					continue;
				}
				if ( !claimedNames.Add( field.CapabilityName ) ) {
					continue;
				}
				if ( field.Kind == TermcapSourceFieldKind.CancelledCapability ) {
					continue;
				}

				fields.Add(
					new TermcapSourceResolvedField(
						entry,
						field,
						0
					)
				);
			}

			if ( reference is not null ) {
				string? referenceName = reference.ReferenceName;
				if ( string.IsNullOrWhiteSpace( referenceName ) ) {
					AddDiagnostic(
						TermcapSourceDiagnosticCodes.MissingReferenceName,
						"A termcap tc= inheritance reference must name another terminal description.",
						reference.Span
					);
					return null;
				}

				ResolvedNode? parent =
					ResolveNamed(
						referenceName,
						checked( depth + 1 ),
						reference.Span
					);
				if ( parent is null ) {
					return null;
				}

				foreach ( TermcapSourceResolvedField parentField in parent.Fields ) {
					if ( !claimedNames.Add( parentField.CapabilityName ) ) {
						continue;
					}

					fields.Add(
						new TermcapSourceResolvedField(
							parentField.SourceEntry,
							parentField.SourceField,
							checked( parentField.InheritanceDepth + 1 )
						)
					);
				}
			}

			return new ResolvedNode(
				entry,
				fields
			);
		}

		private void AddDiagnostic(
			string code,
			string message,
			TermcapSourceSpan? span
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( code );
			ArgumentException.ThrowIfNullOrWhiteSpace( message );

			_diagnostics.Add(
				new TermcapSourceDiagnostic(
					code,
					TermcapSourceDiagnosticSeverity.Error,
					message,
					span
				)
			);
		}

		private string CreateCycleMessage(
			string requestedName,
			TermcapSourceEntry entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( requestedName );
			ArgumentNullException.ThrowIfNull( entry );

			int start =
				_activePath.FindIndex(
					frame =>
						ReferenceEquals(
							frame.Entry,
							entry
						)
						|| string.Equals(
							frame.RequestedName,
							requestedName,
							StringComparison.Ordinal
						)
				);
			IEnumerable<string> cycle =
				( start < 0 )
					? _activePath
						.Select(
							frame => frame.RequestedName
						)
						.Append( requestedName )
					: _activePath
						.Skip( start )
						.Select(
							frame => frame.RequestedName
						)
						.Append( requestedName )
			;

			return
				"Inheritance cycle detected: "
				+ string.Join(
					" -> ",
					cycle
				)
				+ ".";
		}

		private static void ValidateProviderResult(
			bool found,
			TermcapSourceEntry? entry,
			string requestedName
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( requestedName );

			if ( found && entry is null ) {
				throw new InvalidOperationException(
					$"The source-entry provider reported success for '{requestedName}' but returned a null entry."
				);
			}
			if ( !found && entry is not null ) {
				throw new InvalidOperationException(
					$"The source-entry provider reported a clean miss for '{requestedName}' but returned a non-null entry."
				);
			}
		}
	}

	private sealed class DocumentEntryProvider : ITermcapSourceEntryProvider
	{
		private readonly TermcapSourceDocument _document;

		internal DocumentEntryProvider(
			TermcapSourceDocument document
		) {
			ArgumentNullException.ThrowIfNull( document );
			_document = document;
		}

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermcapSourceEntry? entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			foreach ( TermcapSourceEntry candidate in _document.Entries ) {
				if (
					candidate.Names.Any(
						candidateName =>
							string.Equals(
								candidateName,
								name,
								StringComparison.Ordinal
							)
					)
				) {
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
			TermcapSourceEntry entry,
			IEnumerable<TermcapSourceResolvedField> fields
		) {
			ArgumentNullException.ThrowIfNull( entry );
			ArgumentNullException.ThrowIfNull( fields );

			Entry = entry;
			Fields = fields.ToArray();
		}

		internal TermcapSourceEntry Entry { get; }

		internal IReadOnlyList<TermcapSourceResolvedField> Fields { get; }
	}

	private readonly record struct ActiveFrame(
		string RequestedName,
		TermcapSourceEntry Entry
	);
}
