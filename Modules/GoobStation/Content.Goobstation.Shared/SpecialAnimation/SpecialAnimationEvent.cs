// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SpecialAnimation;

/// <summary>
/// Raised on some client to play a spell card animation.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public sealed partial class SpecialAnimationEvent : EntityEventArgs
{
    public SpecialAnimationEvent(SpecialAnimationData animationData)
    {
        AnimationData = animationData;
    }

    public SpecialAnimationData AnimationData;
}
