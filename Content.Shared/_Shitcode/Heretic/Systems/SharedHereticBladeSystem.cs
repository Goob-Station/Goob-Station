// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using System.Numerics;
using System.Text;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Content.Shared.Atmos.Rotting;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Teleportation;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedHereticBladeSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedHereticCombatMarkSystem _combatMark = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSanguineStrikeSystem _sanguine = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticBladeComponent, UseInHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticBladeComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HereticBladeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HereticBladeComponent, AfterInteractEvent>(OnAfterInteract);
    }

    // Void seeking blade
    private void OnAfterInteract(Entity<HereticBladeComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == ent || ent.Comp.Path != "Void" || !TryComp(args.User, out HereticComponent? heretic) ||
            !TryComp(args.User, out CombatModeComponent? combat) ||
            heretic is not { CurrentPath: "Void", PathStage: >= 7 } || !HasComp<MobStateComponent>(args.Target) ||
            !TryComp(ent, out MeleeWeaponComponent? melee) ||
            melee.NextAttack + TimeSpan.FromSeconds(0.5) > _timing.CurTime)
            return;

        var xform = Transform(args.User);
        var targetXform = Transform(args.Target.Value);

        if (xform.MapID != targetXform.MapID)
            return;

        var coords = _xform.GetWorldPosition(xform);
        var targetCoords = _xform.GetWorldPosition(targetXform);

        var dir = targetCoords - coords;
        var len = dir.Length();
        if (len is <= 0f or >= 16f)
            return;

        var normalized = new Vector2(dir.X / len, dir.Y / len);
        var ray = new CollisionRay(coords,
            normalized,
            (int) (CollisionGroup.Impassable | CollisionGroup.InteractImpassable));
        var result = _physics.IntersectRay(xform.MapID, ray, len, args.User).FirstOrNull();
        if (result != null && result.Value.HitEntity != args.Target.Value)
            return;

        var newPos = result?.HitPos ?? targetCoords - normalized * 0.5f;

        _audio.PlayPredicted(ent.Comp.DepartureSound, xform.Coordinates, args.User);
        _xform.SetWorldPosition(args.User, newPos);
        var combatMode = _combat.IsInCombatMode(args.User, combat);
        _combat.SetInCombatMode(args.User, true, combat);
        if (!_melee.AttemptLightAttack(args.User, ent.Owner, melee, args.Target.Value))
            melee.NextAttack += TimeSpan.FromSeconds(1f / _melee.GetAttackRate(ent, args.User, melee));
        _combat.SetInCombatMode(args.User, combatMode, combat);
        _audio.PlayPredicted(ent.Comp.ArrivalSound, xform.Coordinates, args.User);
        args.Handled = true;
    }

    public void ApplySpecialEffect(EntityUid performer, EntityUid target, MeleeHitEvent args)
    {
        if (!TryComp<HereticComponent>(performer, out var hereticComp))
            return;

        switch (hereticComp.CurrentPath)
        {
            case "Ash":
                ApplyAshBladeEffect(target);
                break;

            case "Blade":
                // check event handler
                break;

            case "Flesh":
                // ultra bleed
                ApplyFleshBladeEffect(target);
                break;

            case "Lock":
                break;

            case "Void":
                ApplyVoidBladeEffect(target);
                break;

            case "Rust":
                if (_mobState.IsDead(target))
                    _rotting.ReduceAccumulator(target, -TimeSpan.FromMinutes(1f));
                break;

            default:
                return;
        }
    }

    private void OnInteract(Entity<HereticBladeComponent> ent, ref UseInHandEvent args)
    {
        if (!HasComp<HereticComponent>(args.User))
            return;

        if (!TryComp<RandomTeleportComponent>(ent, out var rtp))
            return;

        RandomTeleport(args.User, ent, rtp);
        _audio.PlayPredicted(ent.Comp.ShatterSound, args.User, args.User);
        args.Handled = true;
    }

    private void OnExamine(Entity<HereticBladeComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<HereticComponent>(args.Examiner))
            return;

        var canBreak = HasComp<RandomTeleportComponent>(ent);

        if (!canBreak)
            return;

        var sb = new StringBuilder();

        if (canBreak)
            sb.AppendLine(Loc.GetString("heretic-blade-examine"));

        args.PushMarkup(sb.ToString());
    }

    private void OnMeleeHit(Entity<HereticBladeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || string.IsNullOrWhiteSpace(ent.Comp.Path))
            return;

        if (ent.Comp.Path == "Flesh" && HasComp<GhoulComponent>(args.User))
            args.BonusDamage += args.BaseDamage * 0.5f; // "ghouls can use bloody blades effectively... so real..."

        if (!TryComp<HereticComponent>(args.User, out var hereticComp))
            return;

        if (ent.Comp.Path != hereticComp.CurrentPath)
            return;

        if (hereticComp.PathStage >= 7)
        {
            switch (hereticComp.CurrentPath)
            {
                case "Rust":
                    args.BonusDamage += new DamageSpecifier
                    {
                        DamageDict =
                        {
                            { "Poison", 8f },
                        },
                    };
                    break;
                case "Blade":
                    args.BonusDamage += new DamageSpecifier
                    {
                        DamageDict =
                        {
                            { "Structural", 10f },
                        },
                    };
                    break;
            }
        }

        var aliveMobsCount = 0;

        foreach (var hit in args.HitEntities)
        {
            // does not work on other heretics (Edit: yes it does)
            // if (HasComp<HereticComponent>(hit))
            //    continue;

            if (hit == args.User)
                continue;

            if (TryComp(hit, out MobStateComponent? mobState) && mobState.CurrentState != MobState.Dead)
                aliveMobsCount++;

            if (TryComp<HereticCombatMarkComponent>(hit, out var mark))
                _combatMark.ApplyMarkEffect(hit, mark, ent.Comp.Path, args.User);

            if (hereticComp.PathStage >= 7)
                ApplySpecialEffect(args.User, hit, args);
        }

        // blade path exclusive.
        if (HasComp<SilverMaelstromComponent>(args.User))
        {
            args.BonusDamage += args.BaseDamage * 0.5f;
            if (aliveMobsCount > 0 && TryComp<DamageableComponent>(args.User, out var dmg))
            {
                var baseHeal = args.BaseDamage.GetTotal();
                var bonusHeal = HasComp<MansusInfusedComponent>(ent) ? baseHeal / 2f : baseHeal / 4f;
                bonusHeal *= aliveMobsCount;

                _sanguine.LifeSteal(args.User, bonusHeal, dmg);
            }
        }
    }

    protected virtual void ApplyAshBladeEffect(EntityUid target) { }

    protected virtual void ApplyFleshBladeEffect(EntityUid target) { }

    protected virtual void ApplyVoidBladeEffect(EntityUid target) { }

    protected virtual void RandomTeleport(EntityUid user, EntityUid blade, RandomTeleportComponent comp) { }
}
