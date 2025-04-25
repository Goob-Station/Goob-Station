// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Power.PTL;
using Content.Server.Flash;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.SMES;
using Content.Server.Stack;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Radiation.Components;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Power.PTL;

public sealed partial class PTLSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    [ValidatePrototypeId<StackPrototype>] private readonly string _cretidsProt = "Credit";

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SmesSystem));
        SubscribeLocalEvent<PTLComponent, AfterInteractEvent>(OnAfterInteract);
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
                ptl.NextShotAt = _time.CurTime + TimeSpan.FromSeconds(ptl.ShootDelay);
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
        Dirty(ent);
    }

    private void Shoot(Entity<PTLComponent> ent)
    {
        var megajoule = 1e6;

        var charge = 0d;
        var spesos = 0d;

        if (TryComp<BatteryComponent>(ent, out var battery))
        {
            charge = battery.CurrentCharge / megajoule;
            // taken from paradise wiki. i don't know what these numbers are doing either just take it as given
            spesos = (int) (40 * charge / (4 * charge + 800));
        }
        if (charge < 1f) return;

        // scale damage from energy
        if (TryComp<HitscanBatteryAmmoProviderComponent>(ent, out var hitscan))
        {
            hitscan.FireCost = (float) (charge * megajoule);
            var prot = _protMan.Index<HitscanPrototype>(hitscan.Prototype);
            prot.Damage = ent.Comp.BaseBeamDamage * charge * 2f;
        }

        if (TryComp<GunComponent>(ent, out var gun))
        {
            var forward = new EntityCoordinates(ent, new Vector2(0, -1));
            _gun.AttemptShoot(ent, ent, gun, forward);
        }

        // EVIL behavior......
        if (charge >= ent.Comp.PowerEvilThreshold)
        {
            var evil = (float) (charge / ent.Comp.PowerEvilThreshold);

            if (TryComp<RadiationSourceComponent>(ent, out var rad))
                rad.Intensity = evil;

            _flash.FlashArea((ent, null), ent, evil, evil);
        }

        ent.Comp.SpesosHeld += spesos;
    }

    private void OnAfterInteract(Entity<PTLComponent> ent, ref AfterInteractEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        var enabled = $"The laser is now {(ent.Comp.Active ? "enabled" : "disabled")}.";
        _popup.PopupEntity(enabled, ent, Content.Shared.Popups.PopupType.SmallCaution);

        Dirty(ent);
    }

    private void OnAfterInteractUsing(Entity<PTLComponent> ent, ref AfterInteractUsingEvent args)
    {
        var held = args.Used;

        // if holding a screwdriver set firing frequency
        if (_tag.HasTag(held, "Screwdriver"))
        {
            var delay = ent.Comp.ShootDelay + 1;
            if (delay > ent.Comp.ShootDelayThreshold.Max)
                delay = ent.Comp.ShootDelayThreshold.Min;
            ent.Comp.ShootDelay = delay;
            _popup.PopupEntity($"Set firing frequency to {delay} seconds.", ent);
        }

        if (_tag.HasTag(held, "Multitool"))
        {
            var stackPrototype = _protMan.Index<StackPrototype>(_cretidsProt);
            _stack.Spawn((int) ent.Comp.SpesosHeld, stackPrototype, Transform(args.User).Coordinates);
            ent.Comp.SpesosHeld = 0;
        }

        Dirty(ent);
    }

    private void OnExamine(Entity<PTLComponent> ent, ref ExaminedEvent args)
    {
        var enabled = $"The laser is now [color=red]{(ent.Comp.Active ? "enabled" : "disabled")}[/color].";
        var spesos = $"It holds [color=yellow]{ent.Comp.SpesosHeld} spesos[/color]. Use [color=green]multitool[/color] to collect.";
        var screw = $"You can use a [color=green]screwdriver[/color] to change it's [color=green]firing frequency[/color].";
        args.PushMarkup($"{screw}\n{spesos}");
    }
}
