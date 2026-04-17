// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared._White.Grab;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.Grab;

public sealed class GrabbingItemSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly GrabIntentSystem _grabbing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabbingItemComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<GrabbingItemComponent, HeldRelayedEvent<FindGrabbingItemEvent>>(OnGetGrabbingItem);
        SubscribeLocalEvent<GrabbingItemComponent, HeldRelayedEvent<StopGrabbingItemPullEvent>>(OnPullStopped);
    }

    private void OnPullStopped(Entity<GrabbingItemComponent> ent, ref HeldRelayedEvent<StopGrabbingItemPullEvent> args)
    {
        if (ent.Comp.ActivelyGrabbingEntity != args.Args.PulledUid)
            return;

        ent.Comp.ActivelyGrabbingEntity = null;
        Dirty(ent);
    }

    private void OnGetGrabbingItem(Entity<GrabbingItemComponent> ent, ref HeldRelayedEvent<FindGrabbingItemEvent> args)
    {
        if (args.Args.Grabbed == null || ent.Comp.ActivelyGrabbingEntity != args.Args.Grabbed)
            return;

        args.Args.GrabbingItem = ent;
    }

    private void OnMeleeHitEvent(Entity<GrabbingItemComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Direction != null || args.HitEntities.Count != 1)
            return;

        if (!TryComp(args.User, out PullerComponent? puller))
            return;

        var hitEntity = args.HitEntities[0];

        if (puller.Pulling != null)
        {
            if (puller.Pulling.Value != ent.Comp.ActivelyGrabbingEntity)
            {
                ent.Comp.ActivelyGrabbingEntity = null;
                Dirty(ent);
                return;
            }

            _grabbing.TryGrab(puller.Pulling.Value, args.User, true, null, ent.Comp.EscapeAttemptModifier);
            return;
        }

        if (!_grabbing.CanGrab(args.User, hitEntity))
            return;

        ent.Comp.ActivelyGrabbingEntity = hitEntity;
        if (!_pulling.TryStartPull(args.User,
                hitEntity,
                puller,
                grabStageOverride: ent.Comp.GrabStageOverride,
                escapeAttemptModifier: ent.Comp.EscapeAttemptModifier))
            ent.Comp.ActivelyGrabbingEntity = null;
        Dirty(ent);
    }
}
