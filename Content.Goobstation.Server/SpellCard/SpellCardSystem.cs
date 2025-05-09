// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SpellCard;
using Content.Shared.Interaction.Events;
using JetBrains.Annotations;
using Robust.Server.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SpellCard;

public sealed class SpellCardSystem : SharedSpellCardSystem
{
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpellCardAnimationOnUseComponent, UseInHandEvent>(OnUsed);
    }

    private void OnUsed(Entity<SpellCardAnimationOnUseComponent> ent, ref UseInHandEvent args)
    {
        var xform = Transform(args.User);

        switch (ent.Comp.BroadcastType)
        {
            case SpellCardBroadcastType.Local:
                PlayAnimationForEntity(args.User, args.User, ent.Comp.AnimationData);
                break;
            case SpellCardBroadcastType.Pvs:
                PlayAnimationFiltered(args.User, Filter.Pvs(args.User, entityManager: EntityManager), ent.Comp.AnimationData);
                break;
            case SpellCardBroadcastType.Grid:
                PlayAnimationFiltered(args.User, Filter.BroadcastGrid(xform.ParentUid), ent.Comp.AnimationData);
                break;
            case SpellCardBroadcastType.Map:
                PlayAnimationFiltered(args.User, Filter.BroadcastMap(xform.MapID), ent.Comp.AnimationData);
                break;
            case SpellCardBroadcastType.Global:
                PlayAnimationFiltered(args.User, Filter.Broadcast(), ent.Comp.AnimationData);
                break;
        }
    }

    /// <summary>
    /// Plays a special attack animation.
    /// </summary>
    /// <param name="sprite">Entity to take the sprite from</param>
    /// <param name="player">Entity to show the animation</param>
    /// <param name="overrideName">If specified, will override the name that is located inside animation data</param>
    /// <param name="animationData">Options to show the animation</param>
    [PublicAPI]
    public void PlayAnimationForEntity(EntityUid sprite, EntityUid player, SpellCardAnimationData? animationData = null, string? overrideName = null)
    {
        animationData ??= SpellCardAnimationData.DefaultAnimation;
        animationData = animationData.WithSource(GetNetEntity(sprite));

        if (overrideName != null)
            animationData = animationData.WithName(overrideName);

        var ev = new SpellCardAnimationEvent { AnimationData = animationData };
        RaiseNetworkEvent(ev, player);
    }

    /// <summary>
    /// Plays a special attack animation, and loads the sprite entity
    /// in PVS for the filter for a small amount of time.
    /// </summary>
    /// <param name="sprite">Entity to take the sprite from</param>
    /// <param name="filter">Entities to show the animation for</param>
    /// <param name="overrideName">If specified, will override the name that is located inside animation data</param>
    /// <param name="animationData">Options to show the animation</param>
    [PublicAPI]
    public void PlayAnimationFiltered(EntityUid sprite, Filter filter, SpellCardAnimationData? animationData = null, string? overrideName = null)
    {
        animationData ??= SpellCardAnimationData.DefaultAnimation;
        animationData = animationData.WithSource(GetNetEntity(sprite));

        if (overrideName != null)
            animationData = animationData.WithName(overrideName);

        _pvsOverride.AddGlobalOverride(sprite);

        // 2 seconds should be enough for all clients to start processing the animation.
        Timer.Spawn(TimeSpan.FromSeconds(2),
            () =>
            {
                _pvsOverride.RemoveGlobalOverride(sprite);
            });

        var ev = new SpellCardAnimationEvent { AnimationData = animationData };
        RaiseNetworkEvent(ev, filter);
    }
}
