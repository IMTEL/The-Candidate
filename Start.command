#!/usr/bin/env bash

set -euo pipefail

script_directory="$(cd -- "$(dirname -- "$0")" && pwd)"
env_file="$script_directory/.env"

if [[ ! -f "$env_file" ]]; then
	echo "Missing environment file: $env_file" >&2
	exit 1
fi

set -o allexport
source "$env_file"
set +o allexport


command -v unityhub >/dev/null 2>&1 || {
	echo "Could not find unityhub on PATH." >&2
	exit 1
}

cd "$script_directory"
nohup unityhub >/dev/null 2>&1 &