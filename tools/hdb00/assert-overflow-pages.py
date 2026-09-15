#!/usr/bin/env python3
"""Assert that a Berkeley DB Hash v9 fixture contains overflow pages."""

from __future__ import annotations

import pathlib
import struct
import sys

HASH_MAGIC = 0x00061561
HASH_VERSION = 9
OVERFLOW_PAGE = 7


def fail(message: str) -> "NoReturn":
    raise SystemExit(message)


def main() -> int:
    if len(sys.argv) != 2:
        fail("Usage: assert-overflow-pages.py DATABASE")

    path = pathlib.Path(sys.argv[1])
    data = path.read_bytes()
    if len(data) < 512:
        fail("Database is too small to contain a Berkeley DB metadata page.")

    little_magic = struct.unpack_from("<I", data, 12)[0]
    big_magic = struct.unpack_from(">I", data, 12)[0]
    if little_magic == HASH_MAGIC:
        prefix = "<"
    elif big_magic == HASH_MAGIC:
        prefix = ">"
    else:
        fail("Database does not contain the Berkeley DB Hash magic.")

    version = struct.unpack_from(f"{prefix}I", data, 16)[0]
    if version != HASH_VERSION:
        fail(f"Expected Berkeley DB Hash v9, found v{version}.")

    page_size = struct.unpack_from(f"{prefix}I", data, 20)[0]
    if page_size < 512 or page_size > 65536 or page_size & (page_size - 1):
        fail(f"Invalid Berkeley DB page size {page_size}.")
    if len(data) % page_size:
        fail("Database length is not a whole number of pages.")

    last_page = struct.unpack_from(f"{prefix}I", data, 32)[0]
    if last_page >= len(data) // page_size:
        fail("Database metadata references a page beyond the file.")

    overflow_pages = 0
    for page_number in range(1, last_page + 1):
        page_offset = page_number * page_size
        if data[page_offset + 25] == OVERFLOW_PAGE:
            overflow_pages += 1

    print(f"Overflow pages: {overflow_pages}")
    if overflow_pages == 0:
        fail("Expected at least one Berkeley DB overflow page.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
