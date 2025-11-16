// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Grab;

public sealed class GrabbingItemSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabbingItemComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<GrabbingItemComponent, AttemptMeleeEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<GrabbingItemComponent, HeldRelayedEvent<FindGrabbingItemEvent>>(OnGetGrabbingItem);
        SubscribeLocalEvent<GrabbingItemComponent, HeldRelayedEvent<StopGrabbingItemPullEvent>>(OnPullStopped);
        SubscribeLocalEvent<GrabbingItemComponent, BeforeThrowEvent>(OnBeforeThrow);
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

    private void OnBeforeThrow(Entity<GrabbingItemComponent> ent, ref BeforeThrowEvent args)
    {
        if (ent.Comp.ActivelyGrabbingEntity == null)
            return;

        args.Cancelled = true;

        _pulling.ThrowGrabbedEntity(args.PlayerUid, args.Direction);
    }

    private void OnMeleeAttempt(Entity<GrabbingItemComponent> ent, ref AttemptMeleeEvent args)
    {
        if (args.Cancelled)
            return;

        var grabbed = ent.Comp.ActivelyGrabbingEntity;

        if (grabbed == null)
            return;

        if (!args.IsHeavyAttack && (!TryComp(args.User, out PullerComponent? puller) ||
            puller.GrabStage < GrabStage.Suffocate))
            return;

        args.Cancelled = true;
        args.Message = Loc.GetString("grabbing-item-attack-fail",
            ("item", Identity.Entity(ent.Owner, EntityManager)),
            ("grabbed", Identity.Entity(grabbed.Value, EntityManager)));
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

            _pulling.TryGrab(puller.Pulling.Value, (args.User, puller), true, null, ent.Comp.EscapeAttemptModifier);
            return;
        }

        if (!_pulling.CanGrab(args.User, hitEntity))
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
