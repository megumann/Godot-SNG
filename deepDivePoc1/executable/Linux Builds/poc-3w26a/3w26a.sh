#!/bin/sh
echo -ne '\033c\033]0;DeepDivePOC1\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/3w26a.x86_64" "$@"
