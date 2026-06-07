// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Events;
using Content.Shared.Climbing.Components;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.TableSlam;

/// <summary>
/// This handles the slamming of individuals onto the furniture known as tables.
/// </summary>
public sealed class TableSlamSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ContestsSystem _contestsSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GrabIntentComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<GrabbableComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<PostTabledComponent, DisarmAttemptEvent>(OnDisarmAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<PostTabledComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime >= comp.PostTabledShovableTime)
                RemComp<PostTabledComponent>(uid);
        }
    }

    private void OnDisarmAttempt(Entity<PostTabledComponent> ent, ref DisarmAttemptEvent args)
    {
        var rand = new Random(SharedRandomExtensions.HashCodeCombine(new List<int> { (int) _gameTiming.CurTick.Value, GetNetEntity(ent).Id }));
        if (!rand.Prob(ent.Comp.ParalyzeChance) || !TryComp<GrabbableComponent>(ent, out var grabbable))
            return;

        _stunSystem.TryUpdateParalyzeDuration(ent, TimeSpan.FromSeconds(grabbable.PostTabledDuration));
        RemComp<PostTabledComponent>(ent);
    }

    private void OnMeleeHit(Entity<GrabIntentComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Direction != null || args.HitEntities.Count != 1)
            return;
        var target = args.HitEntities[0];

        if (ent.Comp.GrabStage < ent.Comp.TableSlamRequiredStage
            || !TryComp<PullerComponent>(ent, out var puller)
            || puller.Pulling == null
            || !HasComp<BonkableComponent>(target))
            return;

        var massRatio = _contestsSystem.MassContest(ent.Owner, puller.Pulling.Value, bypassClamp: true);
        var chance = Math.Clamp(massRatio, 0f, 1f);
        var rand = new Random(SharedRandomExtensions.HashCodeCombine(new List<int> { (int) _gameTiming.CurTick.Value, GetNetEntity(ent).Id }));
        if (rand.Prob(chance))
            TryTableSlam(puller.Pulling.Value, ent.Owner, target);
    }

    public void TryTableSlam(
        Entity<PullableComponent?, GrabbableComponent?> pullable,
        Entity<PullerComponent?, GrabIntentComponent?> puller,
        EntityUid table)
    {
        if (!Resolve(pullable, ref pullable.Comp1, ref pullable.Comp2)
            || !Resolve(puller, ref puller.Comp1, ref puller.Comp2)
            || !_transformSystem.InRange(pullable.Owner.ToCoordinates(), table.ToCoordinates(), puller.Comp2.TableSlamRange))
            return;

        _standing.Down(pullable.Owner);
        _pullingSystem.TryStopPull(pullable.Owner, pullable.Comp1, puller.Owner, ignoreGrab: true);
        _throwingSystem.TryThrow(pullable.Owner, table.ToCoordinates(), pullable.Comp2.BasedTabledForceSpeed, animated: false, doSpin: false);

        puller.Comp2.NextStageChange = _gameTiming.CurTime + TimeSpan.FromSeconds(puller.Comp2.TableSlamCooldown);
        pullable.Comp2.BeingTabled = true;
        Dirty(puller.Owner, puller.Comp2);
        Dirty(pullable.Owner, pullable.Comp2);
    }

    private void OnStartCollide(Entity<GrabbableComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.BeingTabled || !HasComp<BonkableComponent>(args.OtherEntity))
            return;

        var stunDuration = TimeSpan.FromSeconds(ent.Comp.PostTabledDuration);

        if (TryComp<GlassTableComponent>(args.OtherEntity, out var glass))
        {
            _damageableSystem.TryChangeDamage(args.OtherEntity, glass.TableDamage, origin: ent, targetPart: TargetBodyPart.Chest);
            _damageableSystem.TryChangeDamage(args.OtherEntity, glass.ClimberDamage, origin: ent);
            stunDuration *= 2;
        }
        else
        {
            var bluntDamage = new DamageSpecifier { DamageDict = new() { { "Blunt", ent.Comp.TabledDamage } } };
            _damageableSystem.TryChangeDamage(ent.Owner, bluntDamage, targetPart: TargetBodyPart.Chest);
            _damageableSystem.TryChangeDamage(ent.Owner, bluntDamage);
        }

        _staminaSystem.TakeStaminaDamage(ent, ent.Comp.TabledStaminaDamage, applyResistances: true);
        _stunSystem.TryKnockdown(ent.Owner, stunDuration, false);

        var postTabled = EnsureComp<PostTabledComponent>(ent);
        postTabled.PostTabledShovableTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.PostTabledDuration);

        ent.Comp.BeingTabled = false;
        Dirty(ent.Owner, ent.Comp);
    }
}
