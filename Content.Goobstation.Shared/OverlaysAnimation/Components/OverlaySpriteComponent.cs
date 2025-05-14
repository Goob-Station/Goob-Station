// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws a sprite on AnimationsOverlay.
/// Also requires OverlayAnimationComponent and SpriteComponent on parent entity to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlaySpriteComponent : OverlayObjectComponent
{
    /// <summary>
    /// If specified, will copy SpriteComponent of this entity BEFORE initialize.
    /// </summary>
    /// <remarks>
    /// Set this value BEFORE the entity is initialized, otherwise it will be too late!!
    /// </remarks>
    [ViewVariables]
    public NetEntity? OverrideSprite;
}
