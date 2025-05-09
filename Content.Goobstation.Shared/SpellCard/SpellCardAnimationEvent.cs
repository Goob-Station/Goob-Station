// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SpellCard;

/// <summary>
/// Raised on some client to play a spell card animation.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public sealed partial class SpellCardAnimationEvent : EntityEventArgs
{
    public SpellCardAnimationEvent(SpellCardAnimationData animationData)
    {
        AnimationData = animationData;
    }

    public SpellCardAnimationData AnimationData;
}
