// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Mech.Malfunctions;
using Content.Goobstation.Shared.Mech;
using Content.Server.Atmos.Components;
using Content.Server.Mech.Systems;
using Content.Server.Repairable;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Mech.Components;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Mech;

/// <inheritdoc/>
public sealed partial class MalfunctionSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly MechSystem _mech = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MechComponent, ShortCircuitEvent>(OnShortCircuit);
        SubscribeLocalEvent<MechComponent, CabinOnFireEvent>(OnCabinOnFire);
        SubscribeLocalEvent<MechComponent, EngineBrokenEvent>(OnEngineBroken);
        SubscribeLocalEvent<MechComponent, CabinBreachEvent>(OnCabinBreach);
        SubscribeLocalEvent<MechComponent, EquipmentLossEvent>(OnEquipmentLoss);
        SubscribeLocalEvent<MechComponent, RepairedEvent>(OnRepaired);
    }
    private void OnCabinOnFire(EntityUid uid, MechComponent component, CabinOnFireEvent args)
    {
        EnsureComp<CabinOnFireComponent>(uid);
        if (!TryComp<FlammableComponent>(uid, out var flammable))
            return;
        flammable.OnFire = true;
        _popup.PopupEntity(Loc.GetString("goobstation-mech-cabin-on-fire"), uid);
        if (!TryComp<MechMalfunctionComponent>(uid, out var malfunction))
            return;
        _mech.Ignite(uid, component, malfunction.MechFirestacks, malfunction.FirestacksPilotMultiplier);
    }
    private void OnEngineBroken(EntityUid uid, MechComponent component, EngineBrokenEvent args)
    {
        EnsureComp<EngineBrokenComponent>(uid);
        _actionBlocker.UpdateCanMove(uid);
        _popup.PopupEntity(Loc.GetString("goobstation-mech-engine-broken"), uid);
    }
    private void OnCabinBreach(EntityUid uid, MechComponent component, CabinBreachEvent args)
    {
        if (component.Airtight)
        {
            component.Airtight = false;
            EnsureComp<CabinBreachComponent>(uid);
            _popup.PopupEntity(Loc.GetString("goobstation-mech-cabin-breach"), uid);
        }
    }
    private void OnShortCircuit(EntityUid uid, MechComponent component, ShortCircuitEvent args)
    {
        component.Energy -= args.EnergyLoss;
        _popup.PopupEntity(Loc.GetString("goobstation-mech-short-circuit"), uid);
        if (component.PilotSlot.ContainedEntity == null)
            return;
        DamageSpecifier shock = new DamageSpecifier();
        shock.DamageDict.Add("Shock", 15);
        _damageable.TryChangeDamage(component.PilotSlot.ContainedEntity, shock);
    }
    private void OnEquipmentLoss(EntityUid uid, MechComponent component, EquipmentLossEvent args)
    {
        var allEquipment = component.EquipmentContainer.ContainedEntities.Concat(component.ArmorContainer.ContainedEntities).ToList();
        if (allEquipment.Count == 0)
            return;
        var randompick = _random.Pick(allEquipment);
        _mech.RemoveEquipment(uid, randompick, component, forced: true);
        var range = args.Range;
        var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
        _throwingSystem.TryThrow(randompick, direction, range);
        _popup.PopupEntity(Loc.GetString("goobstation-mech-equipment-loss"), uid);
    }
    private void OnRepaired(EntityUid uid, MechComponent comp, RepairedEvent args)
    {
        RemComp<EngineBrokenComponent>(uid);
        if (HasComp<CabinBreachComponent>(uid))
        {
            comp.Airtight = true;
            RemComp<CabinBreachComponent>(uid);
        }
    }
}
