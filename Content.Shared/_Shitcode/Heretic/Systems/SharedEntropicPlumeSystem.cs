// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Religion;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard.TimeStop;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Administration;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.CombatMode;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract class SharedEntropicPlumeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _weapon = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeComponent, StartCollideEvent>(OnStartCollide);

        UpdatesOutsidePrediction = true;
    }

    private void OnStartCollide(Entity<EntropicPlumeComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.AffectedEntities.Contains(args.OtherEntity))
            return;

        if (!HasComp<MobStateComponent>(args.OtherEntity) || HasComp<HereticComponent>(args.OtherEntity) ||
            HasComp<GhoulComponent>(args.OtherEntity))
            return;

        var ev = new BeforeCastTouchSpellEvent(args.OtherEntity, false);
        RaiseLocalEvent(args.OtherEntity, ev, true);
        if (ev.Cancelled)
            return;

        ent.Comp.AffectedEntities.Add(args.OtherEntity);

        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(args.OtherEntity,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(ent.Comp.Duration),
            true);

        var affected = EnsureComp<EntropicPlumeAffectedComponent>(args.OtherEntity);
        affected.Duration = MathF.Max(affected.Duration, ent.Comp.Duration);

        var solution = new Solution();
        foreach (var reagent in ent.Comp.Reagents)
        {
            solution.AddReagent(reagent.Key, reagent.Value);
        }

        if (!_solution.TryGetInjectableSolution(args.OtherEntity, out var targetSolution, out _))
            return;

        _solution.TryAddSolution(targetSolution.Value, solution);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var rand = new System.Random((int) _timing.CurTick.Value);
        var query = EntityQueryEnumerator<EntropicPlumeAffectedComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var affected, out var mobState, out var xform))
        {
            Amok();

            if (_net.IsClient)
                continue;

            affected.Duration -= frameTime;

            if (affected.Duration > 0)
                continue;

            RemCompDeferred(uid, affected);

            continue;

            void Amok()
            {
                if (_net.IsClient && _player.LocalEntity != uid)
                    return;

                var curTime = _timing.CurTime;

                if (curTime < affected.NextAttack)
                    return;

                if (!TryComp(uid, out CombatModeComponent? combat))
                    return;

                if (_mobState.IsIncapacitated(uid, mobState))
                    return;

                if (HasComp<StunnedComponent>(uid) || HasComp<FrozenComponent>(uid) ||
                    HasComp<AdminFrozenComponent>(uid) || HasComp<IceCubeComponent>(uid))
                    return;

                _gun.TryGetGun(uid, out var gun, out var gunComp);
                _weapon.TryGetWeapon(uid, out var weapon, out var meleeComp);

                float range;
                float attackRate;

                if (gunComp != null)
                {
                    if (gunComp.NextFire > curTime)
                        return;

                    attackRate = gunComp.FireRate;
                    range = 3f;
                }
                else if (meleeComp != null)
                {
                    if (meleeComp.NextAttack > curTime)
                        return;

                    attackRate = meleeComp.AttackRate;
                    range = meleeComp.Range;
                }
                else
                    return;

                if (attackRate == 0f)
                    return;

                var targets = FindPotentialTargets((uid, xform), range);
                if (targets.Count == 0)
                    return;

                affected.NextAttack = curTime + TimeSpan.FromSeconds(1f / attackRate);
                Dirty(uid, affected);

                _combat.SetInCombatMode(uid, true, combat);

                var target = rand.Pick(targets);
                var coords = Transform(target).Coordinates;

                if (gunComp != null)
                    _gun.AttemptShoot(uid, gun, gunComp, coords, target);
                else if (meleeComp != null)
                    _weapon.AttemptLightAttack(uid, weapon, meleeComp, target);
            }
        }

        if (!_timing.IsFirstTimePredicted)
            return;

        // Prevent it from behaving weirdly on moving shuttles
        var plumeQuery = EntityQueryEnumerator<EntropicPlumeComponent, PhysicsComponent>();
        while (plumeQuery.MoveNext(out var uid, out _, out var physics))
        {
            if (physics.BodyStatus != BodyStatus.OnGround)
                _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround);
        }
    }

    private List<EntityUid> FindPotentialTargets(Entity<TransformComponent> attacker, float range)
    {
        List<EntityUid> result = new();
        var ents = _lookup.GetEntitiesInRange<MobStateComponent>(attacker.Comp.Coordinates, range, LookupFlags.Dynamic);
        foreach (var ent in ents)
        {
            if (ent.Owner == attacker.Owner)
                continue;

            if (HasComp<HereticComponent>(ent.Owner) || HasComp<GhoulComponent>(ent.Owner))
                continue;

            if (_examine.InRangeUnOccluded(attacker, ent, range + 1f))
                result.Add(ent);
        }

        return result;
    }
}
