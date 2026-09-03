using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA01DatabaseSetFoundationTests {
	[Fact]
	public void EmptySetIsDeterministicCompleteAndBounded() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				Array.Empty<TermInfoDatabaseCatalog>()
			);

		Assert.Empty(set.Entries);
		Assert.Empty(set.Identities);
		Assert.Empty(set.Issues);
		Assert.Equal(0, set.TotalEntryCount);
		Assert.True(set.IsComplete);
	}

	[Fact]
	public void CatalogsAreSnapshottedInCallerOrderAndPreservedByReference() {
		TermInfoDatabaseCatalog first =
			CreateCatalog(
				"first",
				CreateTerminal("zeta", "shared")
			);
		TermInfoDatabaseCatalog second =
			CreateCatalog(
				"second",
				CreateTerminal("alpha", "shared")
			);
		List<TermInfoDatabaseCatalog> catalogs = [ first, second ];

		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(catalogs);
		catalogs.Clear();

		Assert.Equal(2, set.Entries.Count);
		Assert.Same(first, set.Entries[0].Catalog);
		Assert.Same(second, set.Entries[1].Catalog);
		Assert.Equal(0, set.Entries[0].Index);
		Assert.Equal(1, set.Entries[1].Index);
		Assert.Equal(
			new[] { "alpha", "zeta" },
			set.Identities.Select(identity => identity.Name).ToArray()
		);
		Assert.Equal(2, set.TotalEntryCount);
	}

	[Fact]
	public void CanonicalOccurrencesRetainDatabaseAndCatalogCoordinates() {
		TermInfoDatabaseCatalog first =
			CreateCatalog(
				"first-duplicate",
				CreateTerminal("same", "first-alias"),
				CreateTerminal("same", "second-alias")
			);
		TermInfoDatabaseCatalog second =
			CreateCatalog(
				"second-duplicate",
				CreateTerminal("same", "third-alias")
			);

		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ first, second ]);
		TermInfoDatabaseSetIdentity identity = Assert.Single(set.Identities);

		Assert.Equal("same", identity.Name);
		Assert.Equal(3, identity.Occurrences.Count);
		Assert.Equal(0, identity.Occurrences[0].DatabaseIndex);
		Assert.Equal(0, identity.Occurrences[0].CatalogEntryIndex);
		Assert.Equal(0, identity.Occurrences[1].DatabaseIndex);
		Assert.Equal(1, identity.Occurrences[1].CatalogEntryIndex);
		Assert.Equal(1, identity.Occurrences[2].DatabaseIndex);
		Assert.Equal(0, identity.Occurrences[2].CatalogEntryIndex);
	}

	[Fact]
	public void AliasesRemainOccurrenceEvidenceAndDoNotBecomeCanonicalIdentities() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"aliases-a",
						CreateTerminal("canonical-a", "shared-alias")
					),
					CreateCatalog(
						"aliases-b",
						CreateTerminal("canonical-b", "shared-alias")
					),
				]
			);

		Assert.Equal(
			new[] { "canonical-a", "canonical-b" },
			set.Identities.Select(identity => identity.Name).ToArray()
		);
		Assert.All(
			set.Identities,
			identity => Assert.Contains(
				"shared-alias",
				Assert.Single(identity.Occurrences).Aliases
			)
		);
	}

	[Fact]
	public void MissingAndIssueBearingCatalogsMakeAggregateIncompleteWithoutLosingEvidence() {
		string missingRoot = AbsolutePath("missing");
		TermInfoDatabaseCatalog missing =
			new(
				missingRoot,
				TermInfoDatabaseCatalogKind.Missing,
				Array.Empty<TermInfoDatabaseCatalogEntry>(),
				Array.Empty<TermInfoDatabaseCatalogIssue>(),
				Array.Empty<string>()
			);
		string issueRoot = AbsolutePath("issues");
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine(issueRoot, "a", "bad"),
				"DA01 malformed fixture."
			);
		TermInfoDatabaseCatalog incomplete =
			new(
				issueRoot,
				TermInfoDatabaseCatalogKind.ConventionalDirectory,
				Array.Empty<TermInfoDatabaseCatalogEntry>(),
				[ issue ],
				Array.Empty<string>()
			);

		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet([ missing, incomplete ]);

		Assert.False(set.IsComplete);
		Assert.False(set.Entries[0].IsComplete);
		Assert.False(set.Entries[1].IsComplete);
		TermInfoDatabaseSetIssue aggregateIssue = Assert.Single(set.Issues);
		Assert.Equal(1, aggregateIssue.DatabaseIndex);
		Assert.Equal(0, aggregateIssue.CatalogIssueIndex);
		Assert.Same(issue, aggregateIssue.Issue);
		Assert.Equal(TermInfoDatabaseCatalogKind.Missing, set.Entries[0].Catalog.Kind);
	}

	[Fact]
	public void ConstructionIsOrdinalCultureIndependentAndRepeatable() {
		TermInfoDatabaseCatalog catalog =
			CreateCatalog(
				"culture",
				CreateTerminal("zebra"),
				CreateTerminal("I-terminal"),
				CreateTerminal("ı-terminal")
			);
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
			CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
			string[] first =
				TermInfoDatabaseInspector.CreateSet([ catalog ])
					.Identities
					.Select(identity => identity.Name)
					.ToArray();
			string[] second =
				TermInfoDatabaseInspector.CreateSet([ catalog ])
					.Identities
					.Select(identity => identity.Name)
					.ToArray();

			Assert.Equal(first, second);
			Assert.Equal(
				new[] { "I-terminal", "zebra", "ı-terminal" },
				first
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void CancellationAndConfiguredBoundsAreObservedBeforeMisleadingResults() {
		TermInfoDatabaseCatalog first =
			CreateCatalog("bound-one", CreateTerminal("one"));
		TermInfoDatabaseCatalog second =
			CreateCatalog("bound-two", CreateTerminal("two"));
		Assert.Throws<ArgumentException>(
			() => TermInfoDatabaseInspector.CreateSet(
				[ first, second ],
				new TermInfoDatabaseSetOptions(
					maximumDatabaseCount: 1,
					maximumTotalEntryCount: 10
				)
			)
		);
		Assert.Throws<ArgumentException>(
			() => TermInfoDatabaseInspector.CreateSet(
				[ first, second ],
				new TermInfoDatabaseSetOptions(
					maximumDatabaseCount: 2,
					maximumTotalEntryCount: 1
				)
			)
		);

		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoDatabaseInspector.CreateSet(
				[ first ],
				cancellationToken: cancellation.Token
			)
		);
		Assert.Throws<OperationCanceledException>(
			() => TermInfoDatabaseInspector.InspectSet(
				[ AbsolutePath("never-inspected") ],
				cancellationToken: cancellation.Token
			)
		);
	}

	[Fact]
	public void Da01AddsOnlyTheReviewedDatabaseSetConceptFamily() {
		Type[] exportedTypes =
			typeof(TermInfoDatabaseSet).Assembly.GetExportedTypes();
		foreach (
			Type expected
			in new[] {
				typeof(TermInfoDatabaseSet),
				typeof(TermInfoDatabaseSetEntry),
				typeof(TermInfoDatabaseSetIdentity),
				typeof(TermInfoDatabaseSetOccurrence),
				typeof(TermInfoDatabaseSetIssue),
				typeof(TermInfoDatabaseSetOptions),
			}
		) {
			Assert.Contains(expected, exportedTypes);
		}
		Assert.InRange(exportedTypes.Length, 37, int.MaxValue);
	}

	private static TermInfoDatabaseCatalog CreateCatalog(
		string rootName,
		params TerminalDescription[] terminals
	) {
		string root = AbsolutePath(rootName);
		TermInfoDatabaseCatalogEntry[] entries =
			terminals
				.Select(
					(terminal, index) => new TermInfoDatabaseCatalogEntry(
						Path.Combine(root, "entries", index.ToString(CultureInfo.InvariantCulture)),
						terminal
					)
				)
				.ToArray();
		string[] duplicates =
			entries
				.GroupBy(entry => entry.Name, StringComparer.Ordinal)
				.Where(group => group.Count() > 1)
				.Select(group => group.Key)
				.OrderBy(name => name, StringComparer.Ordinal)
				.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			Array.Empty<TermInfoDatabaseCatalogIssue>(),
			duplicates
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder(name);
		foreach (string alias in aliases) {
			builder.AddAlias(alias);
		}
		return builder.Build();
	}

	private static string AbsolutePath(
		string suffix
	) =>
		Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-da01-{suffix}-{Guid.NewGuid():N}"
		);
}
