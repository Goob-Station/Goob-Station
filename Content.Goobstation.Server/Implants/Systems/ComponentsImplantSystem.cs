// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class ComponentsImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentsImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ComponentsImplantComponent, ImplantRemovedEvent>(OnRemoved);
    }

    public void OnImplanted(Entity<ComponentsImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} mob)
            return;

        if (ent.Comp.Added is {} added)
            EntityManager.AddComponents(mob, added);
        if (ent.Comp.Removed is {} removed)
            EntityManager.RemoveComponents(mob, removed);
    }

    public void OnRemoved(Entity<ComponentsImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        var mob = args.Implanted;
        if (ent.Comp.Removed is {} removed)
            EntityManager.AddComponents(mob, removed);
        if (ent.Comp.Added is {} added)
            EntityManager.RemoveComponents(mob, added);
    }
}
