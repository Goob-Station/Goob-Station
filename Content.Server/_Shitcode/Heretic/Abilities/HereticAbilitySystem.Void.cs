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

using Content.Server.Atmos.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Magic;
using Content.Shared.Heretic;
using Content.Shared.Movement.Components;
using Content.Shared.Slippery;
using Robust.Shared.Physics.Components;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Common.Religion;
using Content.Server.Polymorph.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates;
using Content.Shared.Movement.Events;
using Content.Shared.Physics.Controllers;
using Content.Shared.Polymorph;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private static readonly EntProtoId VoidAuraId = "VoidAscensionAura";

    protected override void SubscribeVoid()
    {
        base.SubscribeVoid();

        SubscribeLocalEvent<HereticComponent, HereticAscensionVoidEvent>(OnAscensionVoid);

        SubscribeLocalEvent<HereticComponent, HereticVoidBlastEvent>(OnVoidBlast);
        SubscribeLocalEvent<HereticComponent, HereticVoidPrisonEvent>(OnVoidPrison);

        SubscribeLocalEvent<VoidPrisonComponent, PolymorphedEvent>(OnPrisonRevert);
    }

    private void OnPrisonRevert(Entity<VoidPrisonComponent> ent, ref PolymorphedEvent args)
    {
        if (!args.IsRevert)
            return;

        Spawn(ent.Comp.EndEffect, Transform(ent).Coordinates);
        Voidcurse.DoCurse(args.NewEntity);
    }

    private void OnAscensionVoid(Entity<HereticComponent> ent, ref HereticAscensionVoidEvent args)
    {
        EnsureComp<SpecialHighTempImmunityComponent>(ent);
        EnsureComp<SpecialPressureImmunityComponent>(ent);
        EnsureComp<AristocratComponent>(ent);

        EnsureComp<MovementIgnoreGravityComponent>(ent);
        EnsureComp<CanMoveInAirComponent>(ent);
        EnsureComp<NoSlipComponent>(ent); // :godo:

        // fire immunity
        var flam = EnsureComp<FlammableComponent>(ent);
        flam.Damage = new(); // reset damage dict

        // the hunt begins
        var voidVision = new HereticVoidVisionEvent();
        RaiseLocalEvent(ent, voidVision);

        SpawnAttachedTo(VoidAuraId, ent.Owner.ToCoordinates());
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

    private void OnVoidPrison(Entity<HereticComponent> ent, ref HereticVoidPrisonEvent args)
    {
        var target = args.Target;

        if (!HasComp<PolymorphableComponent>(target) || HasComp<PolymorphedEntityComponent>(target) ||
            HasComp<VoidPrisonComponent>(target))
            return;

        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);
        if (ev.Cancelled)
            return;

        _poly.PolymorphEntity(target, args.Polymorph);
    }
}
