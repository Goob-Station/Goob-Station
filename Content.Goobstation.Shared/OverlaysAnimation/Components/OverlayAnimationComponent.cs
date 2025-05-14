// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.OverlaysAnimation.Animations;

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Basic component that represents one object in overlay animation.
/// </summary>
[RegisterComponent]
public sealed partial class OverlayAnimationComponent : Component
{
    /// <summary>
    /// Current playback position of this animation.
    /// </summary>
    [ViewVariables]
    public float TimePos;

    /// <summary>
    /// Position from the center of the screen.
    /// </summary>
    [DataField]
    public Vector2 Position;

    /// <summary>
    /// List of animations to complete.
    /// </summary>
    [DataField]
    public List<OverlayAnimation> Animations = new();

    [DataField]
    public Angle Angle;

    [DataField]
    public Color Color = Color.White;

    [DataField]
    public float Opacity = 1.0f;

    [DataField]
    public float Scale = 1.0f;
}
