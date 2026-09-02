using System.Buffers;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace Icod.TermInfo.Inspection;

public static partial class TermInfoJsonRenderer {
	private const string TerminalDescriptionDocumentKind =
		"terminalDescription";

	private static string RenderTerminalDescription(
		TerminalDescription description,
		TermInfoJsonRendererOptions options,
		CancellationToken cancellationToken
	) {
		(
			List<KeyValuePair<string, bool>> extendedBooleans,
			List<KeyValuePair<string, int>> extendedNumbers,
			List<KeyValuePair<string, string>> extendedStrings
		) = SnapshotExtendedCapabilities(
			description,
			cancellationToken
		);
		BoundedJsonOutput output =
			new( options.MaximumOutputByteCount );
		DeterministicJsonWriter writer =
			new(
				output,
				options.WriteIndented
			);

		try {
			writer.WriteStartObject();
			writer.WriteString(
				"schema",
				SchemaIdentifier
			);
			writer.WriteNumber(
				"schemaVersion",
				SchemaVersion
			);
			writer.WriteString(
				"documentKind",
				TerminalDescriptionDocumentKind
			);
			writer.WriteStartObject( "data" );
			WriteTerminalIdentity(
				writer,
				description,
				cancellationToken
			);
			WriteTerminalCapabilities(
				writer,
				description,
				extendedBooleans,
				extendedNumbers,
				extendedStrings,
				cancellationToken
			);
			writer.WriteEndObject();
			writer.WriteEndObject();
			cancellationToken.ThrowIfCancellationRequested();
		} catch ( JsonOutputLimitExceededException exception ) {
			throw new InvalidOperationException(
				$"The rendered JSON exceeds the configured {options.MaximumOutputByteCount} UTF-8 byte limit.",
				exception
			);
		}

		return output.GetString();
	}

	private static (
		List<KeyValuePair<string, bool>> Booleans,
		List<KeyValuePair<string, int>> Numbers,
		List<KeyValuePair<string, string>> Strings
	) SnapshotExtendedCapabilities(
		TerminalDescription description,
		CancellationToken cancellationToken
	) {
		List<KeyValuePair<string, bool>> booleans = [];
		List<KeyValuePair<string, int>> numbers = [];
		List<KeyValuePair<string, string>> strings = [];

		foreach (
			KeyValuePair<string, TermInfoCapabilityValue> pair
			in description.ExtendedCapabilities
		) {
			cancellationToken.ThrowIfCancellationRequested();

			switch ( pair.Value.Kind ) {
				case TermInfoCapabilityValueKind.Boolean:
					booleans.Add(
						new KeyValuePair<string, bool>(
							pair.Key,
							pair.Value.BooleanValue
						)
					);
					break;

				case TermInfoCapabilityValueKind.Number:
					numbers.Add(
						new KeyValuePair<string, int>(
							pair.Key,
							pair.Value.NumberValue
						)
					);
					break;

				case TermInfoCapabilityValueKind.String:
					strings.Add(
						new KeyValuePair<string, string>(
							pair.Key,
							pair.Value.StringValue
						)
					);
					break;

				default:
					throw new InvalidOperationException(
						$"Extended capability '{pair.Key}' has unsupported value kind '{pair.Value.Kind}'."
					);
			}
		}

		booleans.Sort( CompareExtendedNames );
		numbers.Sort( CompareExtendedNames );
		strings.Sort( CompareExtendedNames );
		cancellationToken.ThrowIfCancellationRequested();

		return (
			booleans,
			numbers,
			strings
		);
	}

	private static int CompareExtendedNames<TValue>(
		KeyValuePair<string, TValue> left,
		KeyValuePair<string, TValue> right
	) =>
		StringComparer.Ordinal.Compare(
			left.Key,
			right.Key
		);

	private static void WriteTerminalIdentity(
		DeterministicJsonWriter writer,
		TerminalDescription description,
		CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		writer.WriteStartObject( "identity" );
		writer.WriteString(
			"name",
			description.Name
		);
		writer.WriteStartArray( "aliases" );
		foreach ( string alias in description.Aliases ) {
			cancellationToken.ThrowIfCancellationRequested();
			writer.WriteStringValue( alias );
		}
		writer.WriteEndArray();
		writer.WriteString(
			"description",
			description.Description
		);
		writer.WriteEndObject();
	}

	private static void WriteTerminalCapabilities(
		DeterministicJsonWriter writer,
		TerminalDescription description,
		IReadOnlyList<KeyValuePair<string, bool>> extendedBooleans,
		IReadOnlyList<KeyValuePair<string, int>> extendedNumbers,
		IReadOnlyList<KeyValuePair<string, string>> extendedStrings,
		CancellationToken cancellationToken
	) {
		writer.WriteStartObject( "capabilities" );

		writer.WriteStartArray( "booleans" );
		foreach (
			BooleanCapability capability
			in description.BooleanCapabilities
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteBooleanCapability(
				writer,
				StandardCapabilityCatalog
					.GetMetadata( capability )
					.ShortName,
				value: true
			);
		}
		writer.WriteEndArray();

		writer.WriteStartArray( "numbers" );
		foreach (
			KeyValuePair<NumericCapability, int> pair
			in description.NumericCapabilities
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteNumericCapability(
				writer,
				StandardCapabilityCatalog
					.GetMetadata( pair.Key )
					.ShortName,
				pair.Value
			);
		}
		writer.WriteEndArray();

		writer.WriteStartArray( "strings" );
		foreach (
			KeyValuePair<StringCapability, string> pair
			in description.StringCapabilities
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteStringCapability(
				writer,
				StandardCapabilityCatalog
					.GetMetadata( pair.Key )
					.ShortName,
				pair.Value
			);
		}
		writer.WriteEndArray();

		writer.WriteStartObject( "extended" );
		writer.WriteStartArray( "booleans" );
		foreach (
			KeyValuePair<string, bool> pair
			in extendedBooleans
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteBooleanCapability(
				writer,
				pair.Key,
				pair.Value
			);
		}
		writer.WriteEndArray();

		writer.WriteStartArray( "numbers" );
		foreach (
			KeyValuePair<string, int> pair
			in extendedNumbers
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteNumericCapability(
				writer,
				pair.Key,
				pair.Value
			);
		}
		writer.WriteEndArray();

		writer.WriteStartArray( "strings" );
		foreach (
			KeyValuePair<string, string> pair
			in extendedStrings
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteStringCapability(
				writer,
				pair.Key,
				pair.Value
			);
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
		writer.WriteEndObject();
	}

	private static void WriteBooleanCapability(
		DeterministicJsonWriter writer,
		string name,
		bool value
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"name",
			name
		);
		writer.WriteBoolean(
			"value",
			value
		);
		writer.WriteEndObject();
	}

	private static void WriteNumericCapability(
		DeterministicJsonWriter writer,
		string name,
		int value
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"name",
			name
		);
		writer.WriteNumber(
			"value",
			value
		);
		writer.WriteEndObject();
	}

	private static void WriteStringCapability(
		DeterministicJsonWriter writer,
		string name,
		string value
	) {
		writer.WriteStartObjectValue();
		writer.WriteString(
			"name",
			name
		);
		writer.WriteString(
			"value",
			value
		);
		writer.WriteEndObject();
	}

	private enum JsonContainerKind {
		Object,
		Array,
	}

	private sealed class JsonContainerState {
		internal JsonContainerState(
			JsonContainerKind kind
		) {
			Kind = kind;
		}

		internal JsonContainerKind Kind {
			get;
		}

		internal int ItemCount {
			get;
			set;
		}
	}

	private sealed class DeterministicJsonWriter {
		private readonly BoundedJsonOutput _output;
		private readonly bool _writeIndented;
		private readonly List<JsonContainerState> _containers = [];
		private bool _hasRootValue;

		internal DeterministicJsonWriter(
			BoundedJsonOutput output,
			bool writeIndented
		) {
			ArgumentNullException.ThrowIfNull( output );

			_output = output;
			_writeIndented = writeIndented;
		}

		internal void WriteStartObject() {
			if ( _hasRootValue || _containers.Count != 0 ) {
				throw new InvalidOperationException(
					"The deterministic JSON writer already has a root value."
				);
			}

			_hasRootValue = true;
			_output.WriteByte( (byte)'{' );
			_containers.Add(
				new JsonContainerState( JsonContainerKind.Object )
			);
		}

		internal void WriteStartObject(
			string propertyName
		) {
			WritePropertyPrefix( propertyName );
			_output.WriteByte( (byte)'{' );
			_containers.Add(
				new JsonContainerState( JsonContainerKind.Object )
			);
		}

		internal void WriteStartObjectValue() {
			WriteArrayValuePrefix();
			_output.WriteByte( (byte)'{' );
			_containers.Add(
				new JsonContainerState( JsonContainerKind.Object )
			);
		}

		internal void WriteEndObject() =>
			WriteEndContainer(
				JsonContainerKind.Object,
				(byte)'}'
			);

		internal void WriteStartArray(
			string propertyName
		) {
			WritePropertyPrefix( propertyName );
			_output.WriteByte( (byte)'[' );
			_containers.Add(
				new JsonContainerState( JsonContainerKind.Array )
			);
		}

		internal void WriteEndArray() =>
			WriteEndContainer(
				JsonContainerKind.Array,
				(byte)']'
			);

		internal void WriteString(
			string propertyName,
			string? value
		) {
			WritePropertyPrefix( propertyName );
			if ( value is null ) {
				_output.WriteAscii( "null" );
				return;
			}

			WriteQuotedString( value );
		}

		internal void WriteStringValue(
			string value
		) {
			ArgumentNullException.ThrowIfNull( value );
			WriteArrayValuePrefix();
			WriteQuotedString( value );
		}

		internal void WriteBoolean(
			string propertyName,
			bool value
		) {
			WritePropertyPrefix( propertyName );
			_output.WriteAscii(
				value
					? "true"
					: "false"
			);
		}

		internal void WriteNumber(
			string propertyName,
			int value
		) {
			WritePropertyPrefix( propertyName );
			Span<byte> buffer =
				stackalloc byte[ 11 ];
			if (
				!Utf8Formatter.TryFormat(
					value,
					buffer,
					out int written
				)
			) {
				throw new InvalidOperationException(
					$"Unable to format JSON integer value '{value}'."
				);
			}

			_output.Write(
				buffer[ ..written ]
			);
		}

		private void WritePropertyPrefix(
			string propertyName
		) {
			ArgumentNullException.ThrowIfNull( propertyName );
			JsonContainerState container =
				GetCurrentContainer( JsonContainerKind.Object );
			WriteItemPrefix( container );
			WriteQuotedString( propertyName );
			_output.WriteByte( (byte)':' );
			if ( _writeIndented ) {
				_output.WriteByte( (byte)' ' );
			}
		}

		private void WriteArrayValuePrefix() {
			JsonContainerState container =
				GetCurrentContainer( JsonContainerKind.Array );
			WriteItemPrefix( container );
		}

		private void WriteItemPrefix(
			JsonContainerState container
		) {
			if ( container.ItemCount != 0 ) {
				_output.WriteByte( (byte)',' );
			}

			if ( _writeIndented ) {
				WriteNewLineAndIndent( _containers.Count );
			}

			container.ItemCount++;
		}

		private void WriteEndContainer(
			JsonContainerKind expectedKind,
			byte closingToken
		) {
			JsonContainerState container =
				GetCurrentContainer( expectedKind );
			_containers.RemoveAt( _containers.Count - 1 );
			if ( _writeIndented && container.ItemCount != 0 ) {
				WriteNewLineAndIndent( _containers.Count );
			}

			_output.WriteByte( closingToken );
		}

		private JsonContainerState GetCurrentContainer(
			JsonContainerKind expectedKind
		) {
			if ( _containers.Count == 0 ) {
				throw new InvalidOperationException(
					"The deterministic JSON writer has no open container."
				);
			}

			JsonContainerState container =
				_containers[ ^1 ];
			if ( container.Kind != expectedKind ) {
				throw new InvalidOperationException(
					$"The deterministic JSON writer expected an open {expectedKind} container, but found {container.Kind}."
				);
			}

			return container;
		}

		private void WriteNewLineAndIndent(
			int depth
		) {
			_output.WriteByte( (byte)'\n' );
			for ( int index = 0; index < depth * 2; index++ ) {
				_output.WriteByte( (byte)' ' );
			}
		}

		private void WriteQuotedString(
			string value
		) {
			ArgumentNullException.ThrowIfNull( value );

			JsonEncodedText encoded;
			try {
				encoded =
					JsonEncodedText.Encode( value );
			} catch ( ArgumentException exception ) {
				throw new InvalidOperationException(
					"Terminal-description JSON text contains invalid UTF-16 and cannot be represented losslessly.",
					exception
				);
			}

			_output.WriteByte( (byte)'\"' );
			_output.Write( encoded.EncodedUtf8Bytes );
			_output.WriteByte( (byte)'\"' );
		}
	}

	private sealed class BoundedJsonOutput {
		private readonly int _maximumOutputByteCount;
		private readonly ArrayBufferWriter<byte> _buffer;

		internal BoundedJsonOutput(
			int maximumOutputByteCount
		) {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
				maximumOutputByteCount
			);

			_maximumOutputByteCount = maximumOutputByteCount;
			_buffer =
				new ArrayBufferWriter<byte>(
					Math.Min(
						maximumOutputByteCount,
						4_096
					)
				);
		}

		internal void WriteByte(
			byte value
		) {
			EnsureCapacity( 1 );
			Span<byte> destination =
				_buffer.GetSpan( 1 );
			destination[ 0 ] = value;
			_buffer.Advance( 1 );
		}

		internal void Write(
			ReadOnlySpan<byte> value
		) {
			EnsureCapacity( value.Length );
			value.CopyTo(
				_buffer.GetSpan( value.Length )
			);
			_buffer.Advance( value.Length );
		}

		internal void WriteAscii(
			string value
		) {
			ArgumentNullException.ThrowIfNull( value );
			EnsureCapacity( value.Length );
			Span<byte> destination =
				_buffer.GetSpan( value.Length );
			for ( int index = 0; index < value.Length; index++ ) {
				char character = value[ index ];
				if ( character > 0x7F ) {
					throw new InvalidOperationException(
						"The deterministic JSON writer received non-ASCII syntax text."
					);
				}

				destination[ index ] = (byte)character;
			}
			_buffer.Advance( value.Length );
		}

		internal string GetString() =>
			Encoding.UTF8.GetString( _buffer.WrittenSpan );

		private void EnsureCapacity(
			int additionalByteCount
		) {
			ArgumentOutOfRangeException.ThrowIfNegative(
				additionalByteCount
			);

			if (
				additionalByteCount
					> _maximumOutputByteCount - _buffer.WrittenCount
			) {
				throw new JsonOutputLimitExceededException();
			}
		}
	}

	private sealed class JsonOutputLimitExceededException : Exception {
	}
}
