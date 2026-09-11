using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL06PersistentRasterLifecycleJsonTests {
	[Fact]
	public void LifecycleAutomationIdentityIsVersionThree() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:3",
			TermInfoJsonRenderer.PersistentRasterLifecycleSchemaIdentifier
		);
		Assert.Equal(
			3,
			TermInfoJsonRenderer.PersistentRasterLifecycleSchemaVersion
		);
	}

	[Fact]
	public void ProfileRenderingMatchesFrozenCompactVector() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
						isPositive: false,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"caller-negative",
						sourceOrdinal: 0
					),
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"caller-positive",
						sourceOrdinal: 0
					),
				}
			);

		string json = TermInfoJsonRenderer.Render( profile );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:3\",\"schemaVersion\":3,\"documentKind\":\"persistentRasterLifecycleProfile\",\"data\":{\"states\":{\"rasterDisplay\":\"unknown\",\"persistentUpload\":\"supported\",\"acknowledgedUpload\":\"unknown\",\"placementCreation\":\"unsupported\",\"multiplePlacements\":\"unknown\",\"placementUpdate\":\"unknown\",\"placementDeletion\":\"unknown\",\"resourceDeletion\":\"unknown\"},\"evidenceCount\":2,\"evidence\":[{\"subject\":\"persistentUpload\",\"isPositive\":true,\"kind\":\"declared\",\"sourceLabel\":\"caller-positive\",\"sourceOrdinal\":0},{\"subject\":\"placementCreation\",\"isPositive\":false,\"kind\":\"declared\",\"sourceLabel\":\"caller-negative\",\"sourceOrdinal\":0}]}}",
			json
		);
	}

	[Fact]
	public void PlanRenderingMatchesFrozenCompactVector() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"runtime",
						sourceOrdinal: 0
					),
				}
			);
		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				new PersistentRasterLifecycleRequest(
					uploadResource: true,
					placementCount: 1
				)
			);

		string json = TermInfoJsonRenderer.Render( plan );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:3\",\"schemaVersion\":3,\"documentKind\":\"persistentRasterLifecyclePlan\",\"data\":{\"status\":\"indeterminate\",\"requiresRuntimeVerification\":true,\"stepCount\":2,\"issueCount\":1,\"steps\":[{\"sequenceIndex\":0,\"operation\":\"uploadResource\",\"requiresRuntimeVerification\":false},{\"sequenceIndex\":1,\"operation\":\"createPlacement\",\"requiresRuntimeVerification\":true}],\"issues\":[{\"operation\":\"createPlacement\",\"subject\":\"placementCreation\",\"supportStatus\":\"unknown\",\"requiresRuntimeVerification\":true}]}}",
			json
		);
	}

	[Fact]
	public void VersionOneAndTwoSchemaBytesRemainFrozenAfterLfNormalization() {
		string root = FindRepositoryRoot();
		Assert.Equal(
			"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500",
			NormalizedLfSha256(
				Path.Combine(
					root,
					"docs",
					"Icod.TermInfo.Inspection.schema.json"
				)
			)
		);
		Assert.Equal(
			"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0",
			NormalizedLfSha256(
				Path.Combine(
					root,
					"docs",
					"Icod.TermInfo.Inspection.schema.v2.json"
				)
			)
		);
	}

	[Fact]
	public void VersionThreeSchemaAndPackageWiringAreFrozen() {
		string root = FindRepositoryRoot();
		string schemaPath = Path.Combine(
			root,
			"docs",
			"Icod.TermInfo.Inspection.schema.v3.json"
		);
		Assert.Equal(
			"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97",
			NormalizedLfSha256( schemaPath )
		);
		using JsonDocument schema =
			JsonDocument.Parse(
				File.ReadAllText( schemaPath )
			);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:3",
			schema.RootElement.GetProperty( "$id" ).GetString()
		);
		Assert.Contains(
			"persistentRasterLifecycleProfile",
			File.ReadAllText( schemaPath ),
			StringComparison.Ordinal
		);
		Assert.Contains(
			"persistentRasterLifecyclePlan",
			File.ReadAllText( schemaPath ),
			StringComparison.Ordinal
		);
		string projectText = File.ReadAllText(
			Path.Combine(
				root,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.schema.v3.json",
			projectText,
			StringComparison.Ordinal
		);
		string packageVerifier = File.ReadAllText(
			Path.Combine(
				root,
				"tools",
				"inspection-package-verifier",
				"Program.cs"
			)
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.schema.v3.json",
			packageVerifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97",
			packageVerifier,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void LifecycleRenderingIsBoundedCancelableAndCultureIndependent() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"İ-runtime",
						sourceOrdinal: 0
					),
				}
			);
		string invariant = TermInfoJsonRenderer.Render( profile );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( invariant, TermInfoJsonRenderer.Render( profile ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void AdditiveRendererMembersRemainExplicitlyAllowListed() {
		string root = FindRepositoryRoot();
		string allowListPath = Path.Combine(
			root,
			"docs",
			"1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
		);
		string allowList = File.ReadAllText( allowListPath );
		Assert.Contains(
			"PersistentRasterLifecycleSchemaVersion",
			allowList,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterLifecycleSchemaIdentifier",
			allowList,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Render(Icod.TermInfo.Inspection.PersistentRasterLifecycleProfile",
			allowList,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Render(Icod.TermInfo.Inspection.PersistentRasterLifecyclePlan",
			allowList,
			StringComparison.Ordinal
		);

		string verifier = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-inspection-compatibility.ps1"
			)
		);
		Assert.Contains(
			"1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt",
			verifier,
			StringComparison.Ordinal
		);
	}

	private static string NormalizedLfSha256(
		string path
	) {
		string text = File.ReadAllText( path )
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( "\r", "\n", StringComparison.Ordinal );
		byte[] bytes = Encoding.UTF8.GetBytes( text );
		return Convert.ToHexString(
			SHA256.HashData( bytes )
		).ToLowerInvariant();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate repository root."
		);
	}
}
