#!/usr/bin/env python3
"""Build the first Hood streetwear RSIs from the original imagegen atlas.

The source atlas is intentionally kept outside the repository. Pass its path as
the only argument; this script removes the generated checkerboard, crops the
six-by-five grid, and emits ordinary 32x32 RSI states.
"""

from __future__ import annotations

import json
import sys
from collections import deque
from pathlib import Path

from PIL import Image


ITEMS = (
    ("Uniforms/baggy_cream.rsi", "equipped-INNERCLOTHING"),
    ("Uniforms/cargo_graphic.rsi", "equipped-INNERCLOTHING"),
    ("OuterClothing/black_zip_hoodie.rsi", "equipped-OUTERCLOTHING"),
    ("OuterClothing/navy_varsity.rsi", "equipped-OUTERCLOTHING"),
    ("Shoes/white_lowtops.rsi", "equipped-FEET"),
    ("Head/charcoal_fitted_cap.rsi", "equipped-HELMET"),
)

COPYRIGHT = (
    "Original artwork generated with OpenAI imagegen for The Hood on 2026-08-17; "
    "processed into SS14 RSI states by the Hood project."
)


def remove_connected_checkerboard(source: Image.Image) -> Image.Image:
    image = source.convert("RGBA")
    pixels = image.load()
    width, height = image.size
    visited: set[tuple[int, int]] = set()
    queue: deque[tuple[int, int]] = deque()

    for x in range(width):
        queue.append((x, 0))
        queue.append((x, height - 1))
    for y in range(height):
        queue.append((0, y))
        queue.append((width - 1, y))

    def is_background(x: int, y: int) -> bool:
        red, green, blue, _ = pixels[x, y]
        return min(red, green, blue) >= 232 and max(red, green, blue) - min(red, green, blue) <= 8

    while queue:
        point = queue.popleft()
        if point in visited:
            continue
        visited.add(point)
        x, y = point
        if not is_background(x, y):
            continue
        pixels[x, y] = (0, 0, 0, 0)
        if x > 0:
            queue.append((x - 1, y))
        if x + 1 < width:
            queue.append((x + 1, y))
        if y > 0:
            queue.append((x, y - 1))
        if y + 1 < height:
            queue.append((x, y + 1))

    return image


def cell(image: Image.Image, column: int, row: int) -> Image.Image:
    width, height = image.size
    left = round(column * width / 5)
    right = round((column + 1) * width / 5)
    top = round(row * height / 6)
    bottom = round((row + 1) * height / 6)
    crop = image.crop((left, top, right, bottom))

    alpha = crop.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        raise RuntimeError(f"empty atlas cell at row {row}, column {column}")

    art = crop.crop(bounds)
    art.thumbnail((28, 28), Image.Resampling.LANCZOS)
    output = Image.new("RGBA", (32, 32))
    output.alpha_composite(art, ((32 - art.width) // 2, (32 - art.height) // 2))
    return output


def directions(views: list[Image.Image]) -> Image.Image:
    # Robust directions are laid out South, North, East, West in a 2x2 sheet.
    sheet = Image.new("RGBA", (64, 64))
    for view, position in zip(views, ((0, 0), (32, 0), (0, 32), (32, 32)), strict=True):
        sheet.alpha_composite(view, position)
    return sheet


def write_rsi(root: Path, relative: str, equipped_state: str, row: int, image: Image.Image) -> None:
    output = root / relative
    output.mkdir(parents=True, exist_ok=True)

    icon = cell(image, 0, row)
    equipped = directions([cell(image, column, row) for column in range(1, 5)])
    inhand = directions([icon.copy() for _ in range(4)])

    icon.save(output / "icon.png")
    equipped.save(output / f"{equipped_state}.png")
    inhand.save(output / "inhand-left.png")
    inhand.save(output / "inhand-right.png")

    meta = {
        "version": 1,
        "license": "CC-BY-SA-3.0",
        "copyright": COPYRIGHT,
        "size": {"x": 32, "y": 32},
        "states": [
            {"name": "icon"},
            {"name": equipped_state, "directions": 4},
            {"name": "inhand-left", "directions": 4},
            {"name": "inhand-right", "directions": 4},
        ],
    }
    (output / "meta.json").write_text(json.dumps(meta, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("usage: Build-HoodStreetwearRsis.py SOURCE_ATLAS")

    repository = Path(__file__).resolve().parents[2]
    output_root = repository / "Resources/Textures/_Hood/Clothing"
    image = remove_connected_checkerboard(Image.open(sys.argv[1]))

    for row, (relative, equipped_state) in enumerate(ITEMS):
        write_rsi(output_root, relative, equipped_state, row, image)


if __name__ == "__main__":
    main()
