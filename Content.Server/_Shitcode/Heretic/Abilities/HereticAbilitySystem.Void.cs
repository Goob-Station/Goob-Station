// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Atmos.Components;
using Content.Goobstation.Shared.Body.Components;
using Content.Goobstation.Shared.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Magic;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Movement.Components;
using Content.Shared.Slippery;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using System.Linq;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private void SubscribeVoid()
    {
        SubscribeLocalEvent<HereticComponent, HereticAristocratWayEvent>(OnAristocratWay);
        SubscribeLocalEvent<HereticComponent, HereticAscensionVoidEvent>(OnAscensionVoid);

        SubscribeLocalEvent<HereticComponent, HereticVoidBlastEvent>(OnVoidBlast);
        SubscribeLocalEvent<HereticComponent, HereticVoidBlinkEvent>(OnVoidBlink);
        SubscribeLocalEvent<HereticComponent, HereticVoidPullEvent>(OnVoidPull);
    }

    private void OnAristocratWay(Entity<HereticComponent> ent, ref HereticAristocratWayEvent args)
    {
        EnsureComp<SpecialLowTempImmunityComponent>(ent);
        EnsureComp<SpecialBreathingImmunityComponent>(ent);
    }
    private void OnAscensionVoid(Entity<HereticComponent> ent, ref HereticAscensionVoidEvent args)
    {
        EnsureComp<SpecialHighTempImmunityComponent>(ent);
        EnsureComp<SpecialPressureImmunityComponent>(ent);
        EnsureComp<AristocratComponent>(ent);

        EnsureComp<MovementIgnoreGravityComponent>(ent);
        EnsureComp<NoSlipComponent>(ent); // :godo:

        // fire immunity
        var flam = EnsureComp<FlammableComponent>(ent);
        flam.Damage = new(); // reset damage dict

        // the hunt begins
        var voidVision = new HereticVoidVisionEvent();
        RaiseLocalEvent(ent, voidVision);
    }

    private void OnVoidBlast(Entity<HereticComponent> ent, ref HereticVoidBlastEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var rod = Spawn("ImmovableVoidRod", Transform(ent).Coordinates);
        if (TryComp<ImmovableVoidRodComponent>(rod, out var vrod))
            vrod.User = ent;

        if (TryComp(rod, out PhysicsComponent? phys))
        {
            _phys.SetLinearDamping(rod, phys, 0f);
            _phys.SetFriction(rod, phys, 0f);
            _phys.SetBodyStatus(rod, phys, BodyStatus.InAir);

            var xform = Transform(rod);
            var vel = Transform(ent).WorldRotation.ToWorldVec() * 15f;

            _phys.SetLinearVelocity(rod, vel, body: phys);
            xform.LocalRotation = Transform(ent).LocalRotation;
        }

        args.Handled = true;
    }

    private void OnVoidBlink(Entity<HereticComponent> ent, ref HereticVoidBlinkEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var condition = ent.Comp.CurrentPath == "Void";

        var power = condition ? 1.5f + ent.Comp.PathStage / 5f : 1.5f;

        _aud.PlayPvs(new SoundPathSpecifier("/Audio/Effects/tesla_consume.ogg"), ent);

        foreach (var pookie in GetNearbyPeople(ent, power))
            _stun.KnockdownOrStun(pookie, TimeSpan.FromSeconds(power), true);

        _transform.SetCoordinates(ent, args.Target);

        // repeating for both sides
        _aud.PlayPvs(new SoundPathSpecifier("/Audio/Effects/tesla_consume.ogg"), ent);

        foreach (var pookie in GetNearbyPeople(ent, power))
        {
            _stun.KnockdownOrStun(pookie, TimeSpan.FromSeconds(power), true);
            if (condition) _voidcurse.DoCurse(pookie);
        }

        args.Handled = true;
    }

    private void OnVoidPull(Entity<HereticComponent> ent, ref HereticVoidPullEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var power = ent.Comp.CurrentPath == "Void" ? 10f + ent.Comp.PathStage * 2 : 10f;
        var rangeMult = 1f;

        if (HasComp<AristocratComponent>(ent)) // epic boost from epic ascension
        {
            power *= 1.25f;
            rangeMult *= 2f;
        }

        var topPriority = GetNearbyPeople(ent, 1.5f * rangeMult);
        var midPriority = GetNearbyPeople(ent, 2.5f * rangeMult);
        var farPriority = GetNearbyPeople(ent, 5f * rangeMult);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Cold", power);

        // damage closest ones
        foreach (var pookie in topPriority)
        {
            // apply gaming.
            _dmg.TryChangeDamage(pookie, damage, true, targetPart: TargetBodyPart.All);
        }

        // stun close-mid range
        foreach (var pookie in midPriority)
        {
            _stun.TryStun(pookie, TimeSpan.FromSeconds(2.5f), true);
            _stun.TryKnockdown(pookie, TimeSpan.FromSeconds(2.5f), true);

            if (ent.Comp.CurrentPath == "Void")
                _voidcurse.DoCurse(pookie);
        }

        // pull in farthest ones
        foreach (var pookie in farPriority)
            _throw.TryThrow(pookie, Transform(ent).Coordinates);

        args.Handled = true;
    }
}
