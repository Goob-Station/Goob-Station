// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Server.Power.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Null Charge ability.
/// Null Charge is an ability that disables an APC until it gets fixed.
/// </summary>
public sealed class ShadowlingNullChargeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingNullChargeComponent, NullChargeEvent>(OnNullCharge);
        SubscribeLocalEvent<ShadowlingNullChargeComponent, NullChargeDoAfterEvent>(OnNullChargeAfter);
        SubscribeLocalEvent<ShadowlingNullChargeComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingNullChargeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingNullChargeComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingNullChargeComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnNullCharge(EntityUid uid, ShadowlingNullChargeComponent component, NullChargeEvent args)
    {
        if (args.Handled)
            return;

        if (!IsApcInRange(uid, component.Range))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            component.NullChargeToComplete,
            new NullChargeDoAfterEvent(),
            uid);

        _popupSystem.PopupEntity(Loc.GetString("shadowling-null-charge-start"), uid, uid, PopupType.MediumCaution);
        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnNullChargeAfter(EntityUid uid, ShadowlingNullChargeComponent component, NullChargeDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled)
            return;

        bool apcAffected = false;
        foreach (var apc in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (apcAffected)
                break;

            if (!TryComp<ApcComponent>(apc, out var apcComponent))
                continue;
            if (!TryComp<PowerNetworkBatteryComponent>(apc, out var battery))
                continue;

            if (apcComponent.MainBreakerEnabled)
            {
                apcComponent.MainBreakerEnabled = false;
                battery.CanDischarge = false;
                apcAffected = true;
            }
        }

        if (apcAffected)
            _popupSystem.PopupEntity(Loc.GetString("shadowling-null-charge-success"), uid, uid, PopupType.Medium);

        var effectEnt = Spawn(component.NullChargeEffect, _transformSystem.GetMapCoordinates(uid));
        _transformSystem.SetParent(effectEnt, uid);
        args.Handled = true;
    }

    private bool IsApcInRange(EntityUid uid, float range)
    {
        foreach (var target in _lookup.GetEntitiesInRange(uid, range))
        {
            if (HasComp<ApcComponent>(target))
                return true;
        }
        return false;
    }
}
