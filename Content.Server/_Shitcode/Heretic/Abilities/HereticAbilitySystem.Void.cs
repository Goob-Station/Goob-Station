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
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private static readonly EntProtoId<VoidAscensionAuraComponent> VoidAuraId = "VoidAscensionAura";

    protected override void SubscribeVoid()
    {
        base.SubscribeVoid();

        SubscribeLocalEvent<HereticAscensionVoidEvent>(OnAscensionVoid);

        SubscribeLocalEvent<HereticVoidPrisonEvent>(OnVoidPrison);

        SubscribeLocalEvent<VoidPrisonComponent, PolymorphedEvent>(OnPrisonRevert);
    }

    private void OnPrisonRevert(Entity<VoidPrisonComponent> ent, ref PolymorphedEvent args)
    {
        if (!args.IsRevert)
            return;

        Spawn(ent.Comp.EndEffect, Transform(ent).Coordinates);
        Voidcurse.DoCurse(args.NewEntity);
    }

    private void OnAscensionVoid(HereticAscensionVoidEvent args)
    {
        if (!args.Negative)
            SpawnAttachedTo(VoidAuraId, args.Heretic.ToCoordinates());
        else
        {
            var childEnumerator = Transform(args.Heretic).ChildEnumerator;
            while (childEnumerator.MoveNext(out var child))
            {
                if (HasComp<VoidAscensionAuraComponent>(child))
                    QueueDel(child);
            }
        }
    }

    private void OnVoidPrison(HereticVoidPrisonEvent args)
    {
        var target = args.Target;

        if (!HasComp<PolymorphableComponent>(target) || HasComp<PolymorphedEntityComponent>(target) ||
            HasComp<VoidPrisonComponent>(target))
            return;

        if (!TryUseAbility(args))
            return;

        args.Handled = true;

        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);
        if (ev.Cancelled)
            return;

        _poly.PolymorphEntity(target, args.Polymorph);
    }
}
