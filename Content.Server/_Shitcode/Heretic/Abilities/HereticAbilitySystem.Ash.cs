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
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    protected override void SubscribeAsh()
    {
        base.SubscribeAsh();

        SubscribeLocalEvent<EventHereticAshenShift>(OnJaunt);
        SubscribeLocalEvent<EventHereticNightwatcherRebirth>(OnNWRebirth);
        SubscribeLocalEvent<EventHereticFlames>(OnFlames);
        SubscribeLocalEvent<EventHereticCascade>(OnCascade);
    }

    private void OnJaunt(EventHereticAshenShift args)
    {
        if (!TryUseAbility(args))
            return;

        Spawn("PolymorphAshJauntAnimation", Transform(args.Performer).Coordinates);
        _poly.PolymorphEntity(args.Performer, args.Jaunt);
    }


    private void OnNWRebirth(EventHereticNightwatcherRebirth args)
    {
        if (!TryUseAbility(args))
            return;

        Heretic.TryGetHereticComponent(args.Performer, out var heretic, out _);

        if (heretic is not { Ascended: true, CurrentPath: "Ash" })
            _flammable.Extinguish(args.Performer);

        var lookup = GetNearbyPeople(args.Performer, args.Range, heretic?.CurrentPath ?? "Ash");
        var toHeal = 0f;

        foreach (var look in lookup)
        {
            if (!TryComp<FlammableComponent>(look, out var flam) || !flam.OnFire ||
                !TryComp<MobStateComponent>(look, out var mobstate) || mobstate.CurrentState == MobState.Dead)
                continue;

            if (mobstate.CurrentState == MobState.Critical)
                _mobstate.ChangeMobState(look, MobState.Dead, mobstate);

            toHeal += args.HealAmount;

            _flammable.AdjustFireStacks(look, args.FireStacks, flam, true, args.FireProtectionPenetration);
            _dmg.TryChangeDamage(look,
                args.Damage * _body.GetVitalBodyPartRatio(look),
                true,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAll);
        }

        if (toHeal >= 0)
            return;

        // heals everything by base + power for each burning target
        _stam.TryTakeStamina(args.Performer, toHeal);
        IHateWoundMed(args.Performer, AllDamage * toHeal, toHeal, toHeal, toHeal, 0, 0);
    }

    private void OnFlames(EventHereticFlames args)
    {
        if (!TryUseAbility(args))
            return;

        EnsureComp<HereticFlamesComponent>(args.Performer);
    }

    private void OnCascade(EventHereticCascade args)
    {
        if (!Transform(args.Performer).GridUid.HasValue || !TryUseAbility(args))
            return;

        CombustArea(args.Performer, 9, false);
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
