#!/usr/bin/env python3
"""Build the second Hood streetwear set from its original imagegen atlas."""

from __future__ import annotations

import json
import sys
from pathlib import Path

from PIL import Image


ITEMS = (
    ("Uniforms/tank_workpants.rsi", "equipped-INNERCLOTHING"),
    ("Uniforms/forest_polo.rsi", "equipped-INNERCLOTHING"),
    ("OuterClothing/charcoal_puffer.rsi", "equipped-OUTERCLOTHING"),
    ("OuterClothing/brown_work_jacket.rsi", "equipped-OUTERCLOTHING"),
    ("Head/black_knit_beanie.rsi", "equipped-HELMET"),
    ("Eyes/smoke_rectangular_glasses.rsi", "equipped-EYES"),
)

COPYRIGHT = (
    "Original artwork generated with OpenAI imagegen for The Hood on 2026-08-17; "
    "processed into SS14 RSI states by the Hood project."
)


def frame(image: Image.Image, column: int, row: int) -> Image.Image:
    width, height = image.size
    crop = image.crop((round(column * width / 5), round(row * height / 6),
                       round((column + 1) * width / 5), round((row + 1) * height / 6)))
    alpha = crop.getchannel("A").point(lambda value: 0 if value < 40 else 255)
    crop.putalpha(alpha)
    bounds = alpha.getbbox()
    if bounds is None:
        raise RuntimeError(f"empty atlas cell at row {row}, column {column}")
    art = crop.crop(bounds)
    art.thumbnail((28, 28), Image.Resampling.NEAREST)
    output = Image.new("RGBA", (32, 32))
    output.alpha_composite(art, ((32 - art.width) // 2, (32 - art.height) // 2))
    return output


def directions(frames: list[Image.Image]) -> Image.Image:
    sheet = Image.new("RGBA", (64, 64))
    for view, position in zip(frames, ((0, 0), (32, 0), (0, 32), (32, 32)), strict=True):
        sheet.alpha_composite(view, position)
    return sheet


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("usage: Build-HoodStreetwearExpansionRsis.py SOURCE_ATLAS")
    source = Image.open(sys.argv[1]).convert("RGBA")
    root = Path(__file__).resolve().parents[2] / "Resources/Textures/_Hood/Clothing"

    for row, (relative, equipped_state) in enumerate(ITEMS):
        output = root / relative
        output.mkdir(parents=True, exist_ok=True)
        icon = frame(source, 0, row)
        icon.save(output / "icon.png")
        directions([frame(source, column, row) for column in range(1, 5)]).save(output / f"{equipped_state}.png")
        inhand = directions([icon.copy() for _ in range(4)])
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


if __name__ == "__main__":
    main()
