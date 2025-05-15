// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class FadeOverlayAnimation : IOverlayAnimation
{
    [DataField]
    public float StartOpacity = 1.0f;

    [DataField]
    public float EndOpacity = 1.0f;

    [DataField]
    public string? Name { get; set; }

    [DataField]
    public AnimationType AnimationType { get; set; }

    [DataField]
    public float StartDelay { get; set; }

    [DataField]
    public float Duration { get; set; }

    [DataField]
    public float? ExponentSpeed { get; set; }
}
