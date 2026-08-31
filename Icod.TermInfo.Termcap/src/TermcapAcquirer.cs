using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Performs explicit opt-in termcap acquisition without participating in Runtime
/// terminfo discovery.
/// </summary>
public static class TermcapAcquirer
{
	/// <summary>
	/// Acquires one named terminal description through the explicitly configured
	/// termcap source sequence, resolves <c>tc=</c>, and converts the result into
	/// the canonical Runtime model.
	/// </summary>
	public static TermcapAcquisitionResult Acquire(
		string name,
		TermcapAcquisitionOptions options
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( options );

		AcquisitionEntryProvider provider =
			new( options );
		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				provider,
				name,
				options.ResolverOptions
			);
		TermcapSourceDiagnostic[] sourceDiagnostics =
			provider.Diagnostics
				.Concat( resolved.Diagnostics )
				.ToArray();
		TermcapAcquisitionSource? source =
			provider.GetSource( name );

		if (
			provider.HasErrors
			|| resolved.HasErrors
			|| resolved.Entry is null
		) {
			return new TermcapAcquisitionResult(
				null,
				source,
				sourceDiagnostics,
				Array.Empty<TermcapConversionDiagnostic>()
			);
		}

		TermcapConversionResult converted =
			TermcapConverter.Convert( resolved.Entry );
		return new TermcapAcquisitionResult(
			converted.Description,
			source,
			sourceDiagnostics,
			converted.Diagnostics
		);
	}

	private sealed class AcquisitionEntryProvider : ITermcapSourceEntryProvider
	{
		private readonly TermcapAcquisitionOptions _options;
		private readonly Candidate[] _candidates;
		private readonly List<TermcapSourceDiagnostic> _diagnostics = [];
		private readonly Dictionary<string, TermcapAcquisitionSource> _sourceByName =
			new( StringComparer.Ordinal );

		internal AcquisitionEntryProvider(
			TermcapAcquisitionOptions options
		) {
			ArgumentNullException.ThrowIfNull( options );

			_options = options;
			_candidates = CreateCandidates( options );
		}

		internal IReadOnlyList<TermcapSourceDiagnostic> Diagnostics =>
			_diagnostics;

		internal bool HasErrors =>
			_diagnostics.Any(
				diagnostic =>
					diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
			);

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermcapSourceEntry? entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			foreach ( Candidate candidate in _candidates ) {
				TermcapSourceDocument? document =
					GetDocument( candidate );
				if ( document is null ) {
					continue;
				}

				foreach ( TermcapSourceEntry candidateEntry in document.Entries ) {
					if (
						candidateEntry.Names.Any(
							candidateName =>
								string.Equals(
									candidateName,
									name,
									StringComparison.Ordinal
								)
						)
					) {
						_sourceByName.TryAdd(
							name,
							candidate.Source
						);
						entry = candidateEntry;
						return true;
					}
				}
			}

			entry = null;
			return false;
		}

		internal TermcapAcquisitionSource? GetSource(
			string name
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			return _sourceByName.TryGetValue(
				name,
				out TermcapAcquisitionSource? source
			)
				? source
				: null
			;
		}

		private TermcapSourceDocument? GetDocument(
			Candidate candidate
		) {
			ArgumentNullException.ThrowIfNull( candidate );

			if ( candidate.WasLoaded ) {
				return candidate.Document;
			}
			candidate.WasLoaded = true;

			TermcapSourceParseResult parsed;
			if ( candidate.InlineSource is not null ) {
				parsed =
					TermcapSourceParser.Parse(
						candidate.InlineSource,
						candidate.Source.Identifier,
						_options.ParserOptions
					);
			} else {
				ITermcapFileProvider fileProvider =
					_options.FileProvider
					?? throw new InvalidOperationException(
						"A file-backed acquisition candidate requires a file provider."
					);
				bool found =
					fileProvider.TryOpenText(
						candidate.Source.Identifier,
						out TextReader? reader
					);
				if ( !found ) {
					if ( reader is not null ) {
						reader.Dispose();
						throw new InvalidOperationException(
							$"The file provider reported a clean miss for '{candidate.Source.Identifier}' but returned a reader."
						);
					}
					return null;
				}
				using TextReader openedReader =
					reader
					?? throw new InvalidOperationException(
						$"The file provider reported success for '{candidate.Source.Identifier}' but returned no reader."
					);
				parsed =
					TermcapSourceParser.Parse(
						openedReader,
						candidate.Source.Identifier,
						_options.ParserOptions
					);
			}

			_diagnostics.AddRange( parsed.Diagnostics );
			if ( parsed.HasErrors ) {
				return null;
			}

			candidate.Document = parsed.Document;
			return candidate.Document;
		}

		private static Candidate[] CreateCandidates(
			TermcapAcquisitionOptions options
		) {
			ArgumentNullException.ThrowIfNull( options );

			List<Candidate> candidates = [];
			if ( options.InlineTermcap is not null ) {
				candidates.Add(
					new Candidate(
						new TermcapAcquisitionSource(
							TermcapAcquisitionSourceKind.InlineTermcap,
							options.InlineSourceName
						),
						options.InlineTermcap
					)
				);
			}

			HashSet<string> seenPaths =
				new( StringComparer.Ordinal );
			AddPathCandidate(
				candidates,
				seenPaths,
				options.TermcapDatabasePath,
				TermcapAcquisitionSourceKind.TermcapDatabasePath
			);
			foreach ( string path in options.TermPath ) {
				AddPathCandidate(
					candidates,
					seenPaths,
					path,
					TermcapAcquisitionSourceKind.TermPathDatabase
				);
			}
			if ( options.DefaultPathPolicy == TermcapDefaultPathPolicy.Ncurses ) {
				AddPathCandidate(
					candidates,
					seenPaths,
					"/etc/termcap",
					TermcapAcquisitionSourceKind.ConventionalDefaultDatabase
				);
				AddPathCandidate(
					candidates,
					seenPaths,
					"/usr/share/misc/termcap",
					TermcapAcquisitionSourceKind.ConventionalDefaultDatabase
				);
				if ( options.HomeDirectory is not null ) {
					AddPathCandidate(
						candidates,
						seenPaths,
						Path.Combine(
							options.HomeDirectory,
							".termcap"
						),
						TermcapAcquisitionSourceKind.ConventionalDefaultDatabase
					);
				}
			}

			return candidates.ToArray();
		}

		private static void AddPathCandidate(
			ICollection<Candidate> candidates,
			ISet<string> seenPaths,
			string? path,
			TermcapAcquisitionSourceKind kind
		) {
			ArgumentNullException.ThrowIfNull( candidates );
			ArgumentNullException.ThrowIfNull( seenPaths );
			if ( !Enum.IsDefined( typeof( TermcapAcquisitionSourceKind ), kind ) ) {
				throw new ArgumentOutOfRangeException( nameof( kind ) );
			}

			if ( path is null || !seenPaths.Add( path ) ) {
				return;
			}
			candidates.Add(
				new Candidate(
					new TermcapAcquisitionSource(
						kind,
						path
					),
					null
				)
			);
		}
	}

	private sealed class Candidate
	{
		internal Candidate(
			TermcapAcquisitionSource source,
			string? inlineSource
		) {
			ArgumentNullException.ThrowIfNull( source );

			Source = source;
			InlineSource = inlineSource;
		}

		internal TermcapAcquisitionSource Source { get; }

		internal string? InlineSource { get; }

		internal bool WasLoaded { get; set; }

		internal TermcapSourceDocument? Document { get; set; }
	}
}
