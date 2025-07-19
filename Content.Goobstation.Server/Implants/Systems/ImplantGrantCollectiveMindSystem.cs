// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class ImplantGrantCollectiveMindSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImplantGrantCollectiveMindComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<CollectiveMindComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }

    public void OnImplanted(EntityUid uid, ImplantGrantCollectiveMindComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted == null)
            return;

        var mind = EnsureComp<CollectiveMindComponent>(ev.Implanted.Value);
        mind.Channels.Add(comp.CollectiveMind);
    }

    public void OnUnimplanted(Entity<CollectiveMindComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (TryComp<ImplantGrantCollectiveMindComponent>(args.Implant, out var implant))
            ent.Comp.Channels.Remove(implant.CollectiveMind);
    }
}
