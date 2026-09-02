namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	private const string DatabaseCatalogDocumentKind =
		"databaseCatalog";

	private static string RenderDatabaseCatalog(
		TermInfoDatabaseCatalog catalog,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		BoundedJsonOutput output =
			new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer =
			new(
				output,
				options.WriteIndented
			);

		try {
			writer.WriteStartObject();
			WriteEnvelopePrefix(
				writer,
				DatabaseCatalogDocumentKind
			);
			writer.WriteStartObject( "data" );
			writer.WriteString(
				"root",
				catalog.Root
			);
			writer.WriteString(
				"kind",
				GetDatabaseCatalogKindName( catalog.Kind )
			);
			writer.WriteBoolean(
				"isComplete",
				catalog.Kind == TermInfoDatabaseCatalogKind.ConventionalDirectory
					&& !catalog.HasIssues
			);
			writer.WriteStartArray( "entries" );
			foreach ( TermInfoDatabaseCatalogEntry entry in catalog.Entries ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseCatalogEntry(
					writer,
					entry,
					cancellationToken
				);
			}
			writer.WriteEndArray();
			writer.WriteStartArray( "issues" );
			foreach ( TermInfoDatabaseCatalogIssue issue in catalog.Issues ) {
				cancellationToken.ThrowIfCancellationRequested();
				WriteDatabaseCatalogIssue(
					writer,
					issue
				);
			}
			writer.WriteEndArray();
			WriteStringArray(
				writer,
				"duplicateCanonicalNames",
				catalog.DuplicateCanonicalNames,
				cancellationToken
			);
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw CreateOutputLimitException(
				options,
				exception
			);
		}

		return output.GetString();
	}

	private static void WriteDatabaseCatalogEntry(
		DeterministicJsonWriter writer,
		TermInfoDatabaseCatalogEntry entry,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"path",
			entry.Path
		);
		writer.WriteString(
			"name",
			entry.Name
		);
		WriteStringArray(
			writer,
			"aliases",
			entry.Aliases,
			cancellationToken
		);
		writer.WriteString(
			"description",
			entry.Description
		);
		writer.WriteEndObject();
	}

	private static void WriteDatabaseCatalogIssue(
		DeterministicJsonWriter writer,
		TermInfoDatabaseCatalogIssue issue
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"kind",
			GetDatabaseCatalogIssueKindName( issue.Kind )
		);
		writer.WriteString(
			"path",
			issue.Path
		);
		writer.WriteString(
			"message",
			issue.Message
		);
		writer.WriteEndObject();
	}

	private static string GetDatabaseCatalogKindName(
		TermInfoDatabaseCatalogKind kind
	) =>
		kind switch {
			TermInfoDatabaseCatalogKind.Missing => "missing",
			TermInfoDatabaseCatalogKind.ConventionalDirectory => "conventionalDirectory",
			TermInfoDatabaseCatalogKind.UnsupportedStore => "unsupportedStore",
			TermInfoDatabaseCatalogKind.Unavailable => "unavailable",
			_ => throw new InvalidOperationException(
				$"Unsupported database catalog kind '{kind}'."
			),
		};

	private static string GetDatabaseCatalogIssueKindName(
		TermInfoDatabaseCatalogIssueKind kind
	) =>
		kind switch {
			TermInfoDatabaseCatalogIssueKind.MalformedEntry => "malformedEntry",
			TermInfoDatabaseCatalogIssueKind.InvalidPlacement => "invalidPlacement",
			TermInfoDatabaseCatalogIssueKind.PermissionFailure => "permissionFailure",
			TermInfoDatabaseCatalogIssueKind.IoFailure => "ioFailure",
			TermInfoDatabaseCatalogIssueKind.LinkSkipped => "linkSkipped",
			_ => throw new InvalidOperationException(
				$"Unsupported database catalog issue kind '{kind}'."
			),
		};
}
