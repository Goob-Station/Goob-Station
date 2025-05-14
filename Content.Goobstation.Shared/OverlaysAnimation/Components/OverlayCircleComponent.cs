// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws a circle on AnimationsOverlay.
/// Also requires OverlayAnimationComponent to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlayCircleComponent : OverlayObjectComponent
{
    [DataField]
    public float Radius;
}
