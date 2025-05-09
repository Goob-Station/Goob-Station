// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SpecialAnimation;
using Content.Shared.Interaction.Events;
using JetBrains.Annotations;
using Robust.Server.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SpecialAnimation;

public sealed class SpecialAnimationSystem : SharedSpecialAnimationSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpecialAnimationOnUseComponent, UseInHandEvent>(OnUsed);
    }

    private void OnUsed(Entity<SpecialAnimationOnUseComponent> ent, ref UseInHandEvent args)
    {
        var xform = Transform(args.User);
        args.ApplyDelay = true;

        var animation = SpecialAnimationData.DefaultAnimation;
        if (_protoMan.TryIndex(ent.Comp.AnimationDataId, out var animationProto))
            animation = animationProto.Animation;

        if (ent.Comp.OverrideText != null)
            animation = animation.WithText(ent.Comp.OverrideText);

        switch (ent.Comp.BroadcastType)
        {
            case SpecialAnimationBroadcastType.Local:
                PlayAnimationForEntity(args.User, args.User, animation);
                break;
            case SpecialAnimationBroadcastType.Pvs:
                PlayAnimationFiltered(args.User, Filter.Pvs(args.User, entityManager: EntityManager), animation);
                break;
            case SpecialAnimationBroadcastType.Grid:
                PlayAnimationFiltered(args.User, Filter.BroadcastGrid(xform.ParentUid), animation);
                break;
            case SpecialAnimationBroadcastType.Map:
                PlayAnimationFiltered(args.User, Filter.BroadcastMap(xform.MapID), animation);
                break;
            case SpecialAnimationBroadcastType.Global:
                PlayAnimationFiltered(args.User, Filter.Broadcast(), animation);
                break;
        }
    }

    /// <summary>
    /// Plays a special attack animation.
    /// </summary>
    /// <param name="sprite">Entity to take the sprite from</param>
    /// <param name="player">Entity to show the animation</param>
    /// <param name="overrideText">If specified, will override the name that is located inside animation data</param>
    /// <param name="animationData">Options to show the animation</param>
    [PublicAPI]
    public override void PlayAnimationForEntity(EntityUid sprite, EntityUid player, SpecialAnimationData? animationData = null, string? overrideText = null)
    {
        animationData ??= SpecialAnimationData.DefaultAnimation;
        animationData = animationData.WithSource(GetNetEntity(sprite));

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
    public override void PlayAnimationFiltered(EntityUid sprite, Filter filter, SpecialAnimationData? animationData = null, string? overrideText = null)
    {
        animationData ??= SpecialAnimationData.DefaultAnimation;
        animationData = animationData.WithSource(GetNetEntity(sprite));

        if (overrideText != null)
            animationData = animationData.WithText(overrideText);

        _pvsOverride.AddGlobalOverride(sprite);

        // 2 seconds should be enough for all clients to start processing the animation.
        Timer.Spawn(TimeSpan.FromSeconds(2),
            () =>
            {
                _pvsOverride.RemoveGlobalOverride(sprite);
            });

        var ev = new SpecialAnimationEvent { AnimationData = animationData };
        RaiseNetworkEvent(ev, filter);
    }

    public override void PlayAnimationFiltered(
        EntityUid sprite,
        Filter filter,
        ProtoId<SpecialAnimationPrototype>? animationDataId = null,
        string? overrideText = null)
    {
        if (!_protoMan.TryIndex(animationDataId, out var animationPrototype))
            return;

        PlayAnimationFiltered(sprite, filter, animationPrototype.Animation, overrideText);
    }
}
