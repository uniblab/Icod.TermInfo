#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 2 ]]; then
	echo "usage: build-tool-archives.sh <Debug|Staging|Release> <output-directory>" >&2
	exit 2
fi

configuration="$1"
case "$configuration" in
	Debug|Staging|Release)
		;;
	*)
		echo "unsupported configuration '$configuration'" >&2
		exit 2
		;;
esac

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "$script_directory/../.." && pwd)"
output_directory="$2"
mkdir -p "$output_directory"
output_directory="$(cd -- "$output_directory" && pwd)"

for tool in dotnet zip tar gzip sed touch; do
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

projects=(
	"$repository_root/Icod.TermInfo.csproj"
	"$repository_root/Icod.TermInfo.Source/Icod.TermInfo.Source.csproj"
	"$repository_root/Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj"
	"$repository_root/Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
	"$repository_root/tic/Icod.TermInfo.Tic.csproj"
	"$repository_root/infocmp/Icod.TermInfo.InfoCmp.csproj"
	"$repository_root/toe/Icod.TermInfo.Toe.csproj"
	"$repository_root/icod-terminfo/Icod.TermInfo.Router.csproj"
)

for project in "${projects[@]}"; do
	project_version="$(
		dotnet msbuild "$project" \
			-nologo \
			-getProperty:Version
	)"
	if [[ "$project_version" != "$version" ]]; then
		echo "project '$project' effective Version '$project_version' does not match '$version'" >&2
		exit 1
	fi
done

work_root="$(mktemp -d)"
trap 'rm -rf "$work_root"' EXIT

publish_projects=(
	"$repository_root/tic/Icod.TermInfo.Tic.csproj"
	"$repository_root/infocmp/Icod.TermInfo.InfoCmp.csproj"
	"$repository_root/toe/Icod.TermInfo.Toe.csproj"
)

rids=(
	win-x64
	win-arm64
	linux-x64
	linux-arm64
	osx-x64
	osx-arm64
)

for rid in "${rids[@]}"; do
	stage="$work_root/$rid"
	mkdir -p "$stage"

	for project in "${publish_projects[@]}"; do
		dotnet publish "$project" \
			--configuration "$configuration" \
			--framework net10.0 \
			--runtime "$rid" \
			--self-contained false \
			--nologo \
			-p:UseAppHost=true \
			-p:ContinuousIntegrationBuild=true \
			-p:Deterministic=true \
			-p:DebugSymbols=false \
			-p:DebugType=None \
			--output "$stage"
	done

	case "$rid" in
		win-*)
			for command in tic infocmp toe; do
				if [[ ! -f "$stage/$command.exe" ]]; then
					echo "published $rid payload is missing '$command.exe'" >&2
					exit 1
				fi
			done
			;;
		*)
			for command in tic infocmp toe; do
				if [[ ! -f "$stage/$command" ]]; then
					echo "published $rid payload is missing '$command'" >&2
					exit 1
				fi
				chmod 755 "$stage/$command"
			done
			;;
	esac

	mkdir -p "$stage/documentation"
	cp "$repository_root/README.md" "$stage/documentation/README.md"
	cp "$repository_root/LICENSE" "$stage/documentation/LICENSE.txt"
	cp "$repository_root/tic/README.md" "$stage/documentation/tic.md"
	cp "$repository_root/infocmp/README.md" "$stage/documentation/infocmp.md"
	cp "$repository_root/toe/README.md" "$stage/documentation/toe.md"

	cat > "$stage/TOOL-SUITE.txt" <<EOF
Icod.TermInfo tool suite
Version: $version
RID: $rid
Framework: net10.0
Deployment: framework-dependent
Commands: tic infocmp toe
EOF

	TZ=UTC find "$stage" -exec touch -h -t 198001010000 {} +

	case "$rid" in
		win-x64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.win-x64.zip"
			rm -f "$archive"
			(
				cd "$stage"
				find . -type f -print |
					LC_ALL=C sort |
					zip -X -q "$archive" -@
			)
			;;
		win-arm64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.win-arm64.zip"
			rm -f "$archive"
			(
				cd "$stage"
				find . -type f -print |
					LC_ALL=C sort |
					zip -X -q "$archive" -@
			)
			;;
		linux-x64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.linux-x64.tar.gz"
			(
				cd "$stage"
				tar \
					--sort=name \
					--mtime='1980-01-01T00:00:00Z' \
					--owner=0 \
					--group=0 \
					--numeric-owner \
					-cf - . |
					gzip -n > "$archive"
			)
			;;
		linux-arm64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.linux-arm64.tar.gz"
			(
				cd "$stage"
				tar \
					--sort=name \
					--mtime='1980-01-01T00:00:00Z' \
					--owner=0 \
					--group=0 \
					--numeric-owner \
					-cf - . |
					gzip -n > "$archive"
			)
			;;
		osx-x64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.osx-x64.tar.gz"
			(
				cd "$stage"
				tar \
					--sort=name \
					--mtime='1980-01-01T00:00:00Z' \
					--owner=0 \
					--group=0 \
					--numeric-owner \
					-cf - . |
					gzip -n > "$archive"
			)
			;;
		osx-arm64)
			archive="$output_directory/Icod.TermInfo.Tools.${version}.osx-arm64.tar.gz"
			(
				cd "$stage"
				tar \
					--sort=name \
					--mtime='1980-01-01T00:00:00Z' \
					--owner=0 \
					--group=0 \
					--numeric-owner \
					-cf - . |
					gzip -n > "$archive"
			)
			;;
	esac
done

archive_count="$(
	find "$output_directory" -maxdepth 1 -type f \
		\( -name "Icod.TermInfo.Tools.${version}.*.zip" -o -name "Icod.TermInfo.Tools.${version}.*.tar.gz" \) |
		wc -l |
		tr -d '[:space:]'
)"
if [[ "$archive_count" != "6" ]]; then
	echo "expected six tool-suite archives; found '$archive_count'" >&2
	exit 1
fi
