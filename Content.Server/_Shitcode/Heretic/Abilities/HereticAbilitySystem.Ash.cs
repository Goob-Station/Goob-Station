// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MJSailor <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Server.Atmos.Components;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Shared.Body.Components;
using Content.Shared._Shitmed.Targeting;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    protected override void SubscribeAsh()
    {
        base.SubscribeAsh();

        SubscribeLocalEvent<HereticComponent, EventHereticAshenShift>(OnJaunt);
        SubscribeLocalEvent<GhoulComponent, EventHereticAshenShift>(OnJauntGhoul);

        SubscribeLocalEvent<HereticComponent, EventHereticNightwatcherRebirth>(OnNWRebirth);
        SubscribeLocalEvent<HereticComponent, EventHereticFlames>(OnFlames);
        SubscribeLocalEvent<HereticComponent, EventHereticCascade>(OnCascade);

        SubscribeLocalEvent<HereticComponent, HereticAscensionAshEvent>(OnAscensionAsh);
    }

    private void OnJaunt(Entity<HereticComponent> ent, ref EventHereticAshenShift args)
    {
        if (TryUseAbility(ent, args) && TryDoJaunt(ent, args.Jaunt))
            args.Handled = true;
    }

    private void OnJauntGhoul(Entity<GhoulComponent> ent, ref EventHereticAshenShift args)
    {
        if (TryUseAbility(ent, args) && TryDoJaunt(ent, args.Jaunt))
            args.Handled = true;
    }

    private bool TryDoJaunt(EntityUid ent, string proto)
    {
        Spawn("PolymorphAshJauntAnimation", Transform(ent).Coordinates);
        var urist = _poly.PolymorphEntity(ent, proto);
        if (urist == null)
            return false;

        return true;
    }

    private void OnNWRebirth(Entity<HereticComponent> ent, ref EventHereticNightwatcherRebirth args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (ent.Comp is not { Ascended: true, CurrentPath: "Ash" })
            _flammable.Extinguish(ent);

        var lookup = GetNearbyPeople(ent, args.Range, ent.Comp.CurrentPath);
        var toHeal = 0f;

        foreach (var look in lookup)
        {
            if (!TryComp<FlammableComponent>(look, out var flam) || !flam.OnFire ||
                !TryComp<MobStateComponent>(look, out var mobstate) || mobstate.CurrentState == MobState.Dead)
                continue;

            if (mobstate.CurrentState == MobState.Critical)
                _mobstate.ChangeMobState(look, MobState.Dead, mobstate);

            toHeal += args.HealAmount;

            _flammable.AdjustFireStacks(look, args.FireStacks, flam, true);
            _dmg.TryChangeDamage(look, args.Damage, true, targetPart: TargetBodyPart.All);
        }

        args.Handled = true;

        if (toHeal >= 0)
            return;

        // heals everything by base + power for each burning target
        _stam.TryTakeStamina(ent, toHeal);
        IHateWoundMed(ent.Owner, AllDamage * toHeal, toHeal, toHeal, toHeal);
    }

    private void OnFlames(Entity<HereticComponent> ent, ref EventHereticFlames args)
    {
        if (!TryUseAbility(ent, args))
            return;

        EnsureComp<HereticFlamesComponent>(ent);

        if (ent.Comp.Ascended)
            _flammable.AdjustFireStacks(ent, 20f, ignite: true);

        args.Handled = true;
    }

    private void OnCascade(Entity<HereticComponent> ent, ref EventHereticCascade args)
    {
        if (!TryUseAbility(ent, args) || !Transform(ent).GridUid.HasValue)
            return;

        CombustArea(ent, 9, false);

        if (ent.Comp.Ascended)
            _flammable.AdjustFireStacks(ent, 20f, ignite: true);

        args.Handled = true;
    }


    private void OnAscensionAsh(Entity<HereticComponent> ent, ref HereticAscensionAshEvent args)
    {
        EnsureComp<SpecialLowTempImmunityComponent>(ent);
        EnsureComp<SpecialHighTempImmunityComponent>(ent);
        EnsureComp<SpecialBreathingImmunityComponent>(ent);
        EnsureComp<SpecialPressureImmunityComponent>(ent);

        // fire immunity
        var flam = EnsureComp<FlammableComponent>(ent);
        flam.Damage = new(); // reset damage dict
        // this does NOT protect you against lasers and whatnot. for now. when i figure out THIS STUPID FUCKING LIMB SYSTEM!!!
        // regards.
    }

    #region Helper methods

    [ValidatePrototypeId<EntityPrototype>] private static readonly EntProtoId FirePrototype = "HereticFireAA";

    public async Task CombustArea(EntityUid ent, int range = 1, bool hollow = true)
    {
        // we need this beacon in order for damage box to not break apart
        var beacon = Spawn(null, _xform.GetMapCoordinates((EntityUid) ent));

        for (int i = 0; i <= range; i++)
        {
            SpawnFireBox(beacon, range: i, hollow);
            await Task.Delay((int) 500f);
        }

        EntityManager.DeleteEntity(beacon); // cleanup
    }

    public void SpawnFireBox(EntityUid relative, int range = 0, bool hollow = true)
    {
        if (range == 0)
        {
            Spawn(FirePrototype, Transform(relative).Coordinates);
            return;
        }

        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // get tile position of our entity
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return;

        // make a box
        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(pos, pos).Enlarged(range);
        var box = _map.GetLocalTilesIntersecting(relative, grid, confines).ToList();

        // hollow it out if necessary
        if (hollow)
        {
            var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
            var boxS = _map.GetLocalTilesIntersecting(relative, grid, confinesS).ToList();
            box = box.Where(b => !boxS.Contains(b)).ToList();
        }

        // fill the box
        foreach (var tile in box)
        {
            Spawn(FirePrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile.GridIndices));
        }
    }

    #endregion
}
