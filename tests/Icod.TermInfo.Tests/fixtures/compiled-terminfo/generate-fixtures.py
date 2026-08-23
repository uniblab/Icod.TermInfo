#!/usr/bin/env python3
"""Regenerate the T29 compiled-terminfo fixture corpus.

This is a maintainer tool. Normal builds/tests consume checked-in fixtures and do
not require Python or tic.
"""

from __future__ import annotations

import hashlib
import shutil
import struct
import subprocess
import tempfile
from pathlib import Path

EXPECTED_TIC_VERSION = "ncurses 6.5.20250216"
LEGACY_MAGIC = 0x011A
EXTENDED_NUMBER_MAGIC = 0x021E


def read_u16(data: bytes | bytearray, offset: int) -> int:
    return struct.unpack_from("<H", data, offset)[0]


def conventional_end(data: bytes | bytearray) -> int:
    magic, names, booleans, numbers, strings, table = struct.unpack_from(
        "<6H", data, 0
    )
    if magic not in (LEGACY_MAGIC, EXTENDED_NUMBER_MAGIC):
        raise ValueError(f"unsupported generated magic: {magic:#x}")

    offset = 12 + names + booleans
    if offset & 1:
        offset += 1

    numeric_width = 4 if magic == EXTENDED_NUMBER_MAGIC else 2
    return offset + (numbers * numeric_width) + (strings * 2) + table


def compile_sources(root: Path, tic: str) -> None:
    compiled = root / "compiled"
    compiled.mkdir(exist_ok=True)

    with tempfile.TemporaryDirectory(prefix="icod-terminfo-t29-") as temp_name:
        temp = Path(temp_name)
        for source in sorted((root / "source").glob("*.ti")):
            shutil.rmtree(temp)
            temp.mkdir()
            subprocess.run(
                [tic, "-x", "-o", str(temp), str(source)],
                check=True,
            )
            terminal_name = source.stem
            matches = list(temp.rglob(terminal_name))
            if len(matches) != 1:
                raise RuntimeError(
                    f"expected one compiled entry for {terminal_name}, got {len(matches)}"
                )
            shutil.copyfile(matches[0], compiled / f"{terminal_name}.bin")

    # term(5) reserves 0xfe (0376) for a canceled Boolean. In this standalone
    # source entry tic normalizes bw@ to absent, so preserve the binary boundary
    # explicitly for the future parser fixture.
    edge_path = compiled / "t29-legacy-edge.bin"
    edge = bytearray(edge_path.read_bytes())
    names_size = read_u16(edge, 2)
    boolean_count = read_u16(edge, 4)
    if boolean_count < 1:
        raise RuntimeError("edge fixture has no Boolean table")
    edge[12 + names_size] = 0xFE  # bw has standard Boolean binary index 0.
    edge_path.write_bytes(edge)


def create_adversarial_seeds(root: Path) -> None:
    compiled = root / "compiled"
    malformed = root / "malformed"
    malformed.mkdir(exist_ok=True)

    minimal = (compiled / "t29-legacy-minimal.bin").read_bytes()
    extended = (compiled / "t29-extended.bin").read_bytes()

    (malformed / "truncated-header.bin").write_bytes(minimal[:8])

    data = bytearray(minimal)
    struct.pack_into("<H", data, 4, 0xFFFF)
    (malformed / "impossible-count.bin").write_bytes(data)

    data = bytearray(minimal)
    names_size = read_u16(data, 2)
    data[12 + names_size - 1] = ord("X")
    (malformed / "bad-names-terminator.bin").write_bytes(data)

    data = bytearray(minimal)
    _, names, booleans, numbers, strings, table = struct.unpack_from(
        "<6H", data, 0
    )
    offset = 12 + names + booleans
    if offset & 1:
        offset += 1
    offset += numbers * 2
    if strings < 1:
        raise RuntimeError("minimal fixture has no string offsets")
    struct.pack_into("<h", data, offset, table + 10)
    (malformed / "illegal-string-offset.bin").write_bytes(data)

    data = bytearray(minimal)
    struct.pack_into("<H", data, 0, 0x1234)
    (malformed / "unsupported-magic.bin").write_bytes(data)

    end = conventional_end(extended)
    if end & 1:
        end += 1
    if end >= len(extended):
        raise RuntimeError("extended fixture has no ncurses extension")
    (malformed / "malformed-extended-header.bin").write_bytes(
        extended[: end + 6]
    )

    data = bytearray(extended)
    struct.pack_into("<H", data, end, 0xFFFF)
    (malformed / "impossible-extended-count.bin").write_bytes(data)

    data = bytearray(extended)
    ext_booleans, ext_numbers, ext_strings, _, ext_table_size = (
        struct.unpack_from("<5H", data, end)
    )
    ext_offset = end + 10 + ext_booleans
    if ext_offset & 1:
        ext_offset += 1
    numeric_width = 4 if read_u16(data, 0) == EXTENDED_NUMBER_MAGIC else 2
    ext_offset += ext_numbers * numeric_width
    if ext_strings < 1:
        raise RuntimeError("extended fixture has no extended string offset")
    struct.pack_into("<h", data, ext_offset, ext_table_size + 5)
    (malformed / "illegal-extended-string-offset.bin").write_bytes(data)

    collision = bytearray(extended)
    collision_offset = collision.rfind(b"xyz\0")
    if collision_offset < 0:
        raise RuntimeError("extended fixture has no xyz capability name")
    collision[collision_offset : collision_offset + 4] = b"cup\0"
    (malformed / "extended-standard-name-collision.bin").write_bytes(
        collision
    )


def print_hashes(root: Path) -> None:
    for folder in ("compiled", "malformed"):
        for path in sorted((root / folder).glob("*.bin")):
            digest = hashlib.sha256(path.read_bytes()).hexdigest()
            print(f"{digest}  {path.relative_to(root)}")


def main() -> None:
    root = Path(__file__).resolve().parent
    tic = shutil.which("tic")
    if tic is None:
        raise SystemExit("tic was not found on PATH")

    version = subprocess.check_output([tic, "-V"], text=True).strip()
    if version != EXPECTED_TIC_VERSION:
        raise SystemExit(
            "fixture provenance mismatch: expected "
            f"'{EXPECTED_TIC_VERSION}', found '{version}'"
        )

    compile_sources(root, tic)
    create_adversarial_seeds(root)
    print_hashes(root)


if __name__ == "__main__":
    main()
