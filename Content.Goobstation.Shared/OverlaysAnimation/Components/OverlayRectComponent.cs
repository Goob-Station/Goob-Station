// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws a rectangle on AnimationsOverlay.
/// Also requires OverlayAnimationComponent to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlayRectComponent : OverlayObjectComponent
{
    [DataField]
    public Vector2 BoxSize;
}
