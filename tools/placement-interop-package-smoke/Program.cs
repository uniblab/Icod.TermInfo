using Icod.Terminal;
using Icod.TermInfo.Inspection;

PersistentRasterPlacementProfile profile =
	PersistentRasterPlacementClassifier.Classify(
		new[] {
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"Icod.Terminal 1.12.0 source rectangle",
				0
			),
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"Icod.Terminal 1.12.0 z-order",
				1
			),
		}
	);
if (
	profile.GetStatus( PersistentRasterPlacementSubject.SourceRectangle )
		!= PersistentRasterLifecycleSupportStatus.Supported
	|| profile.GetStatus( PersistentRasterPlacementSubject.SignedZOrder )
		!= PersistentRasterLifecycleSupportStatus.Supported
) {
	throw new InvalidOperationException(
		"The PG07 package-only placement profile did not classify both Terminal 1.12 semantics as supported."
	);
}

TerminalRasterSourceRectangle sourceRectangle = new(
	0,
	0,
	16,
	8
);
TerminalRasterPlacementOptions options = new() {
	SourceRectangle = sourceRectangle,
	ZIndex = -7,
};
if (
	options.SourceRectangle is not TerminalRasterSourceRectangle mappedRectangle
	|| mappedRectangle.X != 0
	|| mappedRectangle.Y != 0
	|| mappedRectangle.Width != 16
	|| mappedRectangle.Height != 8
	|| options.ZIndex != -7
) {
	throw new InvalidOperationException(
		"The PG07 package-only Terminal 1.12 placement mapping did not preserve consumer-owned execution values."
	);
}

Console.WriteLine(
	"PG07 package-only Icod.Terminal 1.12 placement interoperability passed."
);
