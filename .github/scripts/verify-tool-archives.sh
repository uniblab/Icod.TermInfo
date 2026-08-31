#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 1 ]]; then
	echo "usage: verify-tool-archives.sh <archive-directory>" >&2
	exit 2
fi

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "$script_directory/../.." && pwd)"
archive_directory="$1"

if [[ ! -d "$archive_directory" ]]; then
	echo "tool archive directory '$archive_directory' does not exist" >&2
	exit 1
fi

archive_directory="$(cd -- "$archive_directory" && pwd)"

for tool in find grep sed tar unzip; do
	if ! command -v "$tool" >/dev/null 2>&1; then
		echo "required tool '$tool' was not found" >&2
		exit 1
	fi
done

read_suite_version() {
	sed -n 's/^[[:space:]]*<IcodTermInfoSuiteVersion>\([^<]*\)<\/IcodTermInfoSuiteVersion>[[:space:]]*$/\1/p' \
		"$repository_root/Directory.Build.props" |
		head -n 1
}

version="$(read_suite_version)"
if [[ -z "$version" ]]; then
	echo "Directory.Build.props does not declare IcodTermInfoSuiteVersion" >&2
	exit 1
fi

rids=(
	win-x64
	win-arm64
	linux-x64
	linux-arm64
	osx-x64
	osx-arm64
)

archive_count="$(
	find "$archive_directory" -maxdepth 1 -type f \
		\( -name "Icod.TermInfo.Tools.${version}.*.zip" -o -name "Icod.TermInfo.Tools.${version}.*.tar.gz" \) |
		wc -l |
		tr -d '[:space:]'
)"
if [[ "$archive_count" != "6" ]]; then
	echo "expected six tool-suite archives for '$version'; found '$archive_count'" >&2
	exit 1
fi

work_root="$(mktemp -d)"
trap 'rm -rf "$work_root"' EXIT

validate_listing() {
	local archive="$1"
	local listing="$2"

	while IFS= read -r entry; do
		entry="${entry#./}"
		if [[ -z "$entry" ]]; then
			continue
		fi

		case "$entry" in
			/*|../*|*/../*|*/..)
				echo "archive '$archive' contains unsafe path '$entry'" >&2
				exit 1
				;;
		esac
	done <<< "$listing"
}

for rid in "${rids[@]}"; do
	case "$rid" in
		win-*)
			archive="$archive_directory/Icod.TermInfo.Tools.${version}.${rid}.zip"
			;;
		*)
			archive="$archive_directory/Icod.TermInfo.Tools.${version}.${rid}.tar.gz"
			;;
	esac

	if [[ ! -f "$archive" ]]; then
		echo "expected archive '$archive' was not found" >&2
		exit 1
	fi

	stage="$work_root/$rid"
	mkdir -p "$stage"

	case "$archive" in
		*.zip)
			listing="$(unzip -Z1 "$archive")"
			validate_listing "$archive" "$listing"
			unzip -q "$archive" -d "$stage"
			;;
		*.tar.gz)
			listing="$(tar -tzf "$archive")"
			validate_listing "$archive" "$listing"
			tar -xzf "$archive" -C "$stage"
			;;
	esac

	manifest="$stage/TOOL-SUITE.txt"
	if [[ ! -f "$manifest" ]]; then
		echo "archive '$archive' is missing TOOL-SUITE.txt" >&2
		exit 1
	fi

	for required_line in \
		"Icod.TermInfo tool suite" \
		"Version: $version" \
		"RID: $rid" \
		"Framework: net10.0" \
		"Deployment: framework-dependent" \
		"Commands: tic infocmp toe captoinfo infotocap"
	do
		if ! grep -Fxq "$required_line" "$manifest"; then
			echo "archive '$archive' manifest is missing '$required_line'" >&2
			exit 1
		fi
	done

	for document in README.md LICENSE.txt tic.md infocmp.md toe.md captoinfo.md infotocap.md; do
		if [[ ! -f "$stage/documentation/$document" ]]; then
			echo "archive '$archive' is missing documentation/$document" >&2
			exit 1
		fi
	done

	for dependency in \
		Icod.CommandFramework.dll \
		Icod.TermInfo.dll \
		Icod.TermInfo.Source.dll \
		Icod.TermInfo.Termcap.dll \
		Icod.TermInfo.Compiler.dll \
		Icod.TermInfo.Inspection.dll
	do
		if [[ ! -f "$stage/$dependency" ]]; then
			echo "archive '$archive' is missing managed dependency '$dependency'" >&2
			exit 1
		fi
	done

	for command in tic infocmp toe captoinfo infotocap; do
		if [[ "$rid" == win-* ]]; then
			launcher="$stage/$command.exe"
		else
			launcher="$stage/$command"
		fi

		if [[ ! -f "$launcher" ]]; then
			echo "archive '$archive' is missing command launcher '$command'" >&2
			exit 1
		fi
		if [[ "$rid" != win-* && ! -x "$launcher" ]]; then
			echo "archive '$archive' command launcher '$command' is not executable" >&2
			exit 1
		fi

		for metadata in "$command.dll" "$command.deps.json" "$command.runtimeconfig.json"; do
			if [[ ! -f "$stage/$metadata" ]]; then
				echo "archive '$archive' is missing '$metadata'" >&2
				exit 1
			fi
		done
	done

	forbidden="$(
		find "$stage" -type f \
			\( -name '*.pdb' -o -name '*.csproj' -o -name '*.sln' \) \
			-print \
			-quit
	)"
	if [[ -n "$forbidden" ]]; then
		echo "archive '$archive' contains development-only file '$forbidden'" >&2
		exit 1
	fi
done

echo "Verified six Icod.TermInfo tool-suite archives for $version."
