using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Serializes immutable terminal descriptions into conventional compiled
/// terminfo entries.
/// </summary>
/// <remarks>
/// C01 implements deterministic minimal legacy <c>0432</c> entries containing
/// terminal identity metadata only. Standard capability tables, extended
/// sections, and wide-numeric format policy are added by later 1.2 tranches.
/// Writing is pure with respect to filesystem, environment, and native ncurses
/// state.
/// </remarks>
public static class CompiledTermInfoWriter {
	private const ushort LegacyMagic = 0x011A;
	private const int HeaderSize = 12;

	/// <summary>
	/// Writes one representable terminal description as deterministic legacy
	/// <c>0432</c> compiled bytes.
	/// </summary>
	/// <param name="description">
	/// The immutable terminal description to serialize.
	/// </param>
	/// <returns>A newly allocated compiled terminfo entry.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="description"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The terminal identity cannot be represented exactly by the C01 legacy
	/// names section.
	/// </exception>
	/// <exception cref="NotSupportedException">
	/// The description contains capabilities whose compiled tables are not yet
	/// implemented by C01.
	/// </exception>
	public static byte[] Write(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		EnsureC01CapabilityScope( description );

		string identity =
			CreateIdentity( description );
		byte[] identityBytes =
			Encoding.Latin1.GetBytes( identity );
		int namesSize =
			checked( identityBytes.Length + 1 );

		if ( namesSize > ushort.MaxValue ) {
			throw new InvalidOperationException(
				$"The compiled names section requires {namesSize} bytes, exceeding the legacy 16-bit section-size field."
			);
		}

		int alignmentSize =
			( ( HeaderSize + namesSize ) & 1 ) == 0
				? 0
				: 1;
		byte[] entry =
			new byte[HeaderSize + namesSize + alignmentSize];

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 0, 2 ),
			LegacyMagic
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 2, 2 ),
			(ushort)namesSize
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 4, 2 ),
			0
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 6, 2 ),
			0
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 8, 2 ),
			0
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 10, 2 ),
			0
		);

		identityBytes.CopyTo(
			entry.AsSpan( HeaderSize )
		);
		entry[HeaderSize + identityBytes.Length] = 0;

		return entry;
	}

	private static void EnsureC01CapabilityScope(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		if ( description.BooleanCapabilities.Count != 0
			|| description.NumericCapabilities.Count != 0
			|| description.StringCapabilities.Count != 0
			|| description.ExtendedCapabilities.Count != 0 ) {
			throw new NotSupportedException(
				"C01 writes identity-only legacy entries. Standard capability tables are introduced by C02 and extended capability tables by C03."
			);
		}
	}

	private static string CreateIdentity(
		TerminalDescription description
	) {
		ArgumentNullException.ThrowIfNull( description );

		string descriptionText =
			description.Description
			?? throw new InvalidOperationException(
				"The conventional compiled names section requires a verbose terminal description."
			);

		ValidateIdentityComponent(
			description.Name,
			"canonical terminal name"
		);
		foreach ( string alias in description.Aliases ) {
			ValidateIdentityComponent(
				alias,
				"terminal alias"
			);
		}
		ValidateIdentityComponent(
			descriptionText,
			"verbose terminal description"
		);

		string[] fields =
			new string[description.Aliases.Count + 2];
		fields[0] = description.Name;
		for ( int index = 0; index < description.Aliases.Count; index++ ) {
			fields[index + 1] = description.Aliases[index];
		}
		fields[^1] = descriptionText;

		return string.Join(
			'|',
			fields
		);
	}

	private static void ValidateIdentityComponent(
		string value,
		string role
	) {
		ArgumentNullException.ThrowIfNull( value );
		ArgumentException.ThrowIfNullOrWhiteSpace( role );

		if ( value.IndexOf( '\0' ) >= 0 ) {
			throw new InvalidOperationException(
				$"The {role} contains an embedded NUL and cannot be represented by the compiled names section."
			);
		}
		if ( value.IndexOf( '|' ) >= 0 ) {
			throw new InvalidOperationException(
				$"The {role} contains the compiled names separator '|'."
			);
		}

		foreach ( char character in value ) {
			if ( character > '\u00FF' ) {
				throw new InvalidOperationException(
					$"The {role} contains U+{(int)character:X4}, which cannot be represented exactly in Latin-1."
				);
			}
		}
	}
}