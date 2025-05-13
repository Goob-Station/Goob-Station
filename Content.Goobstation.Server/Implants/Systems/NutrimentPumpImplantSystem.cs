// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class NutrimentPumpImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NutrimentPumpImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<NutrimentPumpImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    private void OnImplant(Entity<NutrimentPumpImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var comp = ent.Comp;
        var user = args.Implanted.Value;

        if (HasComp<HungerComponent>(user))
        {
            RemCompDeferred<HungerComponent>(user);
            comp.HadHunger = true;
        }

        if (HasComp<ThirstComponent>(user))
        {
            RemCompDeferred<ThirstComponent>(user);
            comp.HadThirst = true;
        }

    }

    private void OnUnimplanted(Entity<NutrimentPumpImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        var user = args.Container.Owner;
        var comp = ent.Comp;

        if (comp.HadHunger && !TerminatingOrDeleted(user))
            EnsureComp<HungerComponent>(user);
        if (comp.HadThirst && !TerminatingOrDeleted(user))
            EnsureComp<ThirstComponent>(user);
    }
}
