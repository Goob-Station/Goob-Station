// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Server.Power.Components;
using Content.Server.Power.SMES;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using JetBrains.FormatRipper.Dmg;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Power.PTL;

public sealed partial class PTLSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SmesSystem));
        SubscribeLocalEvent<PTLComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<PTLComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<PTLComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<PTLComponent>();

        while (eqe.MoveNext(out var uid, out var ptl))
        {
            if (_time.CurTime > ptl.NextShotAt)
            {
                ptl.NextShotAt = _time.CurTime + ptl.ShootDelay;
                Tick((uid, ptl));
            }
        }
    }

    private void Tick(Entity<PTLComponent> ent)
    {
        if (!TryComp<BatteryComponent>(ent, out var battery)
        || battery.CurrentCharge < ent.Comp.MinShootPower)
            return;

        Shoot(ent);
    }

    private void Shoot(Entity<PTLComponent> ent)
    {
        var charge = 0f;
        var spesos = 0f;

        if (TryComp<BatteryComponent>(ent, out var battery))
        {
            charge = battery.CurrentCharge / 1000000; // in mj
            // taken from paradise wiki
            spesos = (int)(40 * charge / (4 * charge + 800));
        }

        if (charge < 1f) return;

        if (TryComp<HitscanBatteryAmmoProviderComponent>(ent, out var hitscan))
        {
            // scale it up
            hitscan.FireCost = charge;

            var prot = _protMan.Index<HitscanPrototype>(hitscan.Prototype);
            var spec = new DamageSpecifier { DamageDict = { { "Heat", charge * 10f } } };
            prot.Damage = spec;
        }

        if (TryComp<GunComponent>(ent, out var gun))
        {
            var forward = new EntityCoordinates(ent, new Vector2(0, -1));
            _gun.AttemptShoot(ent, ent, gun, forward);
            ent.Comp.SpesosHeld += spesos;
        }
    }

    private void OnChargeChanged(Entity<PTLComponent> ent, ref ChargeChangedEvent args)
    {

    }

    private void OnAfterInteractUsing(Entity<PTLComponent> ent, ref AfterInteractUsingEvent args)
    {
        // if holding a wrench rotate it
    }

    private void OnExamine(Entity<PTLComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"It holds [color=yellow]{ent.Comp.SpesosHeld} spesos[/color]. Alt-click to collect!");
    }
}
