from pathlib import Path
import sys


if len(sys.argv) != 3 or sys.argv[2] not in {"little", "big"}:
    raise SystemExit("Usage: assert-byte-order.py DATABASE little|big")

data = Path(sys.argv[1]).read_bytes()
if len(data) < 512:
    raise SystemExit("Database is smaller than one metadata page.")

expected = bytes.fromhex("61150600" if sys.argv[2] == "little" else "00061561")
actual = data[12:16]
if actual != expected:
    raise SystemExit(
        f"Expected {sys.argv[2]}-endian Hash magic {expected.hex()}, got {actual.hex()}."
    )

print(f"HDB07C byte order: {sys.argv[2]}-endian")
