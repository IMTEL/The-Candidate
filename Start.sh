#!/usr/bin/env bash

set -a
if [[ -f "$(dirname "$0")/.env" ]]; then
	source "$(dirname "$0")/.env"
fi
set +a

nohup unityhub > /dev/null