// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Body.Organ;
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Containers;

public sealed class ContentContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerManagerComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);
    }

    private void OnBeforePolymorphed(Entity<ContainerManagerComponent> ent, ref BeforePolymorphedEvent args)
    {
        // Recursively drop all entities with MindContainerComponent in all containers on the entity
        // This prevents players from being sent to null-space when carried by a polymorphing entity
        var stack = new Stack<EntityUid>();
        var found = new List<EntityUid>(); // Goobstation - Fuck you deltanedas.
        stack.Push(ent);

        while (stack.Count > 0)
        {
            var currentUid = stack.Pop();

            if (!HasComp<ContainerManagerComponent>(currentUid))
                continue;

            foreach (var entity in _container.GetAllContainers(currentUid).SelectMany(container => container.ContainedEntities))
            {
                if (HasComp<MindContainerComponent>(entity)
                    && !HasComp<OrganComponent>(entity)) // Goobstation - Dont teleport brains/other organs.
                {
                    found.Add(entity);
                    continue;
                }

                if (stack.Count < 1000) // Unlikely to have over 1000 nested entities unless there is an infinite loop.
                    stack.Push(entity); // Process this entity's containers next
            }
        }

        foreach (var entity in found) // Goobstation - Fuck you deltanedas.
            _transform.AttachToGridOrMap(entity);
    }
}
