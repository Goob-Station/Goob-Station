// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Server.Atmos.Components;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Implants;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class SpaceProofImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceProofImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<SpaceProofImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    private void OnImplant(Entity<SpaceProofImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var user = args.Implanted.Value;

        EnsureComp<BreathingImmunityComponent>(user);
        EnsureComp<PressureImmunityComponent>(user); // Add the temperature slow immune comp from the ling update when thats here.

    }

    private void OnUnimplanted(Entity<SpaceProofImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        var user = args.Container.Owner;

        if (!TerminatingOrDeleted(user))
            RemCompDeferred<BreathingImmunityComponent>(user);
        if (!TerminatingOrDeleted(user))
            RemCompDeferred<PressureImmunityComponent>(user);
    }
}
