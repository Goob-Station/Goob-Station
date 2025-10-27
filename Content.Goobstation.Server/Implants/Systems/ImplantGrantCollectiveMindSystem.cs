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
        SubscribeLocalEvent<ImplantGrantCollectiveMindComponent, ImplantRemovedEvent>(OnRemoved);
    }

    public void OnImplanted(Entity<ImplantGrantCollectiveMindComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} mob)
            return;

        var mind = EnsureComp<CollectiveMindComponent>(mob);
        mind.Channels.Add(ent.Comp.CollectiveMind);
    }

    public void OnRemoved(Entity<ImplantGrantCollectiveMindComponent> ent, ref ImplantRemovedEvent args)
    {
        if (!TryComp<CollectiveMindComponent>(args.Implanted, out var comp))
            return;

        comp.Channels.Remove(ent.Comp.CollectiveMind);
        if (comp.Channels.Count == 0)
            RemComp(args.Implanted, comp);
    }
}
