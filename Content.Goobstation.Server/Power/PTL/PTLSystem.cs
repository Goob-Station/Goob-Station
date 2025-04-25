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
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;
using System.Text;

namespace Content.Goobstation.Server.Power.PTL;

public sealed partial class PTLSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly AudioSystem _aud = default!;

    [ValidatePrototypeId<StackPrototype>] private readonly string _stackCredits = "Credit";
    [ValidatePrototypeId<TagPrototype>] private readonly string _tagScrewdriver = "Screwdriver";
    [ValidatePrototypeId<TagPrototype>] private readonly string _tagMultitool = "Multitool";

    private readonly SoundPathSpecifier _soundKaching = new("/Audio/Effects/kaching.ogg");
    private readonly SoundPathSpecifier _soundSparks = new("/Audio/Effects/sparks4.ogg");
    private readonly SoundPathSpecifier _soundPower = new("/Audio/Effects/tesla_consume.ogg");

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SmesSystem));
        SubscribeLocalEvent<PTLComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<PTLComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<PTLComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<PTLComponent>();

        while (eqe.MoveNext(out var uid, out var ptl))
        {
            if (!ptl.Active) continue;

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
            // some random formula i found in bounty thread i popped it into desmos i think it looks good
            spesos = (int) (charge / (Math.Log(charge * 2) + 1));
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

    private void OnInteractHand(Entity<PTLComponent> ent, ref InteractHandEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        var enabled = Loc.GetString("ptl-interact-enabled", ("enabled", ent.Comp.Active));
        _popup.PopupEntity(enabled, ent, Content.Shared.Popups.PopupType.SmallCaution);
        _aud.PlayPvs(_soundPower, ent);

        Dirty(ent);
    }

    private void OnAfterInteractUsing(Entity<PTLComponent> ent, ref AfterInteractUsingEvent args)
    {
        var held = args.Used;

        if (_tag.HasTag(held, _tagScrewdriver))
        {
            var delay = ent.Comp.ShootDelay + 1;
            if (delay > ent.Comp.ShootDelayThreshold.Max)
                delay = ent.Comp.ShootDelayThreshold.Min;
            ent.Comp.ShootDelay = delay;
            _popup.PopupEntity(Loc.GetString("ptl-interact-screwdriver", ("delay", ent.Comp.ShootDelay)), ent);
            _aud.PlayPvs(_soundSparks, ent);
        }

        if (_tag.HasTag(held, _tagMultitool))
        {
            var stackPrototype = _protMan.Index<StackPrototype>(_stackCredits);
            _stack.Spawn((int) ent.Comp.SpesosHeld, stackPrototype, Transform(args.User).Coordinates);
            ent.Comp.SpesosHeld = 0;
            _popup.PopupEntity(Loc.GetString("ptl-interact-spesos"), ent);
            _aud.PlayPvs(_soundKaching, ent);
        }

        Dirty(ent);
    }

    private void OnExamine(Entity<PTLComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("ptl-examine-enabled", ("enabled", ent.Comp.Active)));
        sb.AppendLine(Loc.GetString("ptl-examine-spesos", ("spesos", ent.Comp.SpesosHeld)));
        sb.AppendLine(Loc.GetString("ptl-examine-screwdriver"));
        args.PushMarkup(sb.ToString());
    }
}
