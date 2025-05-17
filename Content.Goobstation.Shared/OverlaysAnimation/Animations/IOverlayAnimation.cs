// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

public interface IOverlayAnimation
{
    [DataField]
    public string? Name { get; set; }

    [DataField]
    public AnimationType AnimationType { get; set; }

    /// <summary>
    /// How long we should wait until playing this animation.
    /// </summary>
    [DataField]
    public float StartDelay  { get; set; }

    /// <summary>
    /// How long it takes to play the animation.
    /// </summary>
    [DataField]
    public float Duration { get; set; }

    /// <summary>
    /// If specified, will be passed into the exponent
    /// function to play animation faster or slower than normal.
    /// </summary>
    [DataField]
    public float? ExponentSpeed { get; set; }
}
