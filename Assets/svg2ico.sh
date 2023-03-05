#!/bin/bash

set -e

TEMP_DIR=$(mktemp -d)

function finish {
   rm -rf "$TEMP_DIR"
}

trap finish EXIT


if [[ -z "$1" ]]; then
    echo "First argument must be path"
    exit 1
fi

INPUT_FILE="$1"

declare -a LAYERS=()

for res in 256 128 64 32 16; do
    echo "Creating ${res}px layer..."
    OUTPUT_FILE="$TEMP_DIR/$res.png"
    convert -density 1200 -background none -resize "${res}x${res}" "$INPUT_FILE" "$OUTPUT_FILE"

    LAYERS+=("$OUTPUT_FILE")
done

OUTPUT_FILE="$(dirname $INPUT_FILE)/$(basename $INPUT_FILE .svg).ico"

echo "Merging layers to $OUTPUT_FILE..."
convert $(IFS=' '; echo "${LAYERS[*]}") "$OUTPUT_FILE"
