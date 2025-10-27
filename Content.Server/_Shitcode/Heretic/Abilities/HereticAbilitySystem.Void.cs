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

using Content.Goobstation.Shared.Body.Components;
using Content.Server.Atmos.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Magic;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Movement.Components;
using Content.Shared.Slippery;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature.Components;
using System.Linq;
using Content.Goobstation.Common.BlockTeleport;
using Content.Shared.Interaction;

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
        var ev = new TeleportAttemptEvent(false);
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return;

        if (!TryUseAbility(ent, args))
            return;

        var target = _transform.ToMapCoordinates(args.Target);
        if (!_examine.InRangeUnOccluded(ent, target, SharedInteractionSystem.MaxRaycastRange))
        {
            // can only dash if the destination is visible on screen
            Popup.PopupEntity(Loc.GetString("dash-ability-cant-see"), ent, ent);
            return;
        }

        var people = GetNearbyPeople(ent, args.Radius, ent.Comp.CurrentPath);
        var xform = Transform(ent);

        Spawn(args.InEffect, xform.Coordinates);
        _transform.SetCoordinates(ent, xform, args.Target);
        Spawn(args.OutEffect, args.Target);

        var condition = ent.Comp.CurrentPath == "Void";

        people.AddRange(GetNearbyPeople(ent, args.Radius, ent.Comp.CurrentPath));
        foreach (var pookie in people.ToHashSet())
        {
            if (condition)
                _voidcurse.DoCurse(pookie);
            _dmg.TryChangeDamage(pookie,
                args.Damage,
                true,
                origin: ent,
                targetPart: TargetBodyPart.All,
                canMiss: false);
        }

        args.Handled = true;
    }

    private void OnVoidPull(Entity<HereticComponent> ent, ref HereticVoidPullEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var path = ent.Comp.CurrentPath;
        var topPriority = GetNearbyPeople(ent, args.DamageRadius, path);
        var midPriority = GetNearbyPeople(ent, args.StunRadius, path);
        var farPriority = GetNearbyPeople(ent, args.Radius, path);

        // damage closest ones
        foreach (var pookie in topPriority)
        {
            // apply gaming.
            _dmg.TryChangeDamage(pookie,
                args.Damage,
                true,
                origin: ent,
                targetPart: TargetBodyPart.All,
                canMiss: false);
        }

        var condition = ent.Comp.CurrentPath == "Void";

        // stun close-mid range
        foreach (var pookie in midPriority)
        {
            _stun.TryStun(pookie, args.StunTime, true);
            _stun.TryKnockdown(pookie, args.KnockDownTime, true);

            if (condition)
                _voidcurse.DoCurse(pookie);
        }

        var coords = Transform(ent).Coordinates;

        // pull in farthest ones
        foreach (var pookie in farPriority)
        {
            _throw.TryThrow(pookie, coords);
        }

        Spawn(args.InEffect, coords);

        args.Handled = true;
    }
}
