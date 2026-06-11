// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SpecialAnimation;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.SpecialAnimation;

public sealed class SpecialAnimationSystem : SharedSpecialAnimationSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    /// <summary>
    /// Plays a special attack animation.
    /// </summary>
    /// <param name="sprite">Entity to take the sprite from</param>
    /// <param name="player">Entity to show the animation</param>
    /// <param name="overrideText">If specified, will override the name that is located inside animation data</param>
    /// <param name="animationData">Options to show the animation</param>
    [PublicAPI]
    public override void PlayAnimationForEntity(SpriteSpecifier sprite, EntityUid player, SpecialAnimationData? animationData = null, string? overrideText = null)
    {
        animationData ??= SpecialAnimationData.DefaultAnimation;
        animationData.Sprite = sprite;

        if (overrideText != null)
            animationData = animationData.WithText(overrideText);

        var ev = new SpecialAnimationEvent { AnimationData = animationData };
        RaiseNetworkEvent(ev, player);
    }

    /// <summary>
    /// Plays a special attack animation, and loads the sprite entity
    /// in PVS for the filter for a small amount of time.
    /// </summary>
    /// <param name="sprite">Entity to take the sprite from</param>
    /// <param name="filter">Entities to show the animation for</param>
    /// <param name="overrideText">If specified, will override the name that is located inside animation data</param>
    /// <param name="animationData">Options to show the animation</param>
    [PublicAPI]
    public override void PlayAnimationFiltered(SpriteSpecifier sprite, Filter filter, SpecialAnimationData? animationData = null, string? overrideText = null)
    {
        animationData ??= SpecialAnimationData.DefaultAnimation;
        animationData.Sprite = sprite;

        if (overrideText != null)
            animationData = animationData.WithText(overrideText);

        var ev = new SpecialAnimationEvent { AnimationData = animationData };
        RaiseNetworkEvent(ev, filter);
    }

    public override void PlayAnimationFiltered(
        SpriteSpecifier sprite,
        Filter filter,
        ProtoId<SpecialAnimationPrototype>? animationDataId = null,
        string? overrideText = null)
    {
        if (!_protoMan.TryIndex(animationDataId, out var animationPrototype))
            return;

        PlayAnimationFiltered(sprite, filter, animationPrototype.Animation, overrideText);
    }
}
