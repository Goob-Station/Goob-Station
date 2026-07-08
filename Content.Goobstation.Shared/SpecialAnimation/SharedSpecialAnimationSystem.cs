// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialAnimation;

/// <summary>
/// Works only on server-side, has methods to access special animations API.
/// </summary>
public abstract class SharedSpecialAnimationSystem : EntitySystem
{
    public virtual void PlayAnimationForEntity(
        SpriteSpecifier sprite,
        EntityUid player,
        SpecialAnimationData? animationData = null,
        string? overrideText = null)
    {
    }

    public virtual void PlayAnimationFiltered(
        SpriteSpecifier sprite,
        Filter filter,
        SpecialAnimationData? animationData = null,
        string? overrideText = null)
    {
    }

    public virtual void PlayAnimationFiltered(
        SpriteSpecifier sprite,
        Filter filter,
        ProtoId<SpecialAnimationPrototype>? animationData = null,
        string? overrideText = null)
    {
    }
}
