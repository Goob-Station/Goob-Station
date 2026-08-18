#!/usr/bin/env python3
"""Build Hood long-gun RSIs from the three original imagegen concept sheets.

Usage: Build-HoodLongGunRsis.py ROOK_ATLAS ARROYO_ATLAS MESA_ATLAS

Only the isolated weapon concepts are retained. Presentation backgrounds and
incidental figures are discarded through the source alpha channel and crops.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

from PIL import Image


COPYRIGHT = (
    "Original artwork generated with OpenAI imagegen for The Hood on 2026-08-17; "
    "isolated, downsampled, and arranged as SS14 RSI states by the Hood project."
)


def sprite(source: Image.Image, bounds: tuple[int, int, int, int]) -> Image.Image:
    art = source.convert("RGBA").crop(bounds)
    alpha = art.getchannel("A")
    # The source has useful soft alpha but no opaque backdrop. Removing its very
    # faint presentation glow yields clean game sprites without color-keying.
    alpha = alpha.point(lambda value: 0 if value < 48 else 255)
    art.putalpha(alpha)
    visible = alpha.getbbox()
    if visible is None:
        raise RuntimeError(f"empty source crop: {bounds}")
    art = art.crop(visible)
    art.thumbnail((30, 22), Image.Resampling.NEAREST)
    output = Image.new("RGBA", (32, 32))
    output.alpha_composite(art, ((32 - art.width) // 2, (32 - art.height) // 2))
    return output


def directional(icon: Image.Image) -> Image.Image:
    south = icon.rotate(270, resample=Image.Resampling.NEAREST, expand=False)
    north = icon.rotate(90, resample=Image.Resampling.NEAREST, expand=False)
    east = icon.copy()
    west = icon.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    sheet = Image.new("RGBA", (64, 64))
    for frame, position in zip(
        (south, north, east, west), ((0, 0), (32, 0), (0, 32), (32, 32)), strict=True
    ):
        sheet.alpha_composite(frame, position)
    return sheet


def write_rsi(
    root: Path,
    name: str,
    source: Image.Image,
    normal: tuple[int, int, int, int],
    opened: tuple[int, int, int, int],
    empty: tuple[int, int, int, int] | None,
    wielded: bool,
) -> None:
    output = root / f"{name}.rsi"
    output.mkdir(parents=True, exist_ok=True)
    icon = sprite(source, normal)
    bolt = sprite(source, opened)
    icon.save(output / "icon.png")
    icon.save(output / "base.png")
    bolt.save(output / "bolt-open.png")

    states: list[dict[str, object]] = [
        {"name": "icon"},
        {"name": "base"},
        {"name": "bolt-open"},
    ]
    if empty is not None:
        sprite(source, empty).save(output / "mag-0.png")
        states.append({"name": "mag-0"})

    views = ("inhand-left", "inhand-right", "equipped-BACKPACK", "equipped-SUITSTORAGE")
    if wielded:
        views += ("wielded-inhand-left", "wielded-inhand-right")
    for state in views:
        directional(icon).save(output / f"{state}.png")
        states.append({"name": state, "directions": 4})

    meta = {
        "version": 1,
        "license": "CC-BY-SA-3.0",
        "copyright": COPYRIGHT,
        "size": {"x": 32, "y": 32},
        "states": states,
    }
    (output / "meta.json").write_text(json.dumps(meta, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    if len(sys.argv) != 4:
        raise SystemExit("usage: Build-HoodLongGunRsis.py ROOK_ATLAS ARROYO_ATLAS MESA_ATLAS")

    output = Path(__file__).resolve().parents[2] / "Resources/Textures/_Hood/Objects/Weapons/Guns"
    rook, arroyo, mesa = (Image.open(path) for path in sys.argv[1:])
    write_rsi(output, "rook_c9", rook, (48, 17, 318, 141), (371, 17, 640, 134), (707, 17, 977, 129), False)
    write_rsi(output, "arroyo_r12", arroyo, (33, 33, 510, 189), (525, 22, 1004, 188), (1040, 33, 1515, 170), True)
    write_rsi(output, "mesa_p12", mesa, (238, 18, 1192, 205), (238, 223, 1192, 409), None, True)


if __name__ == "__main__":
    main()
