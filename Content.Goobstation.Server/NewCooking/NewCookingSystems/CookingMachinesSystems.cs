// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Moony33 <ultimoprmo@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.NewCooking.NewCookingComponent;
using Content.Server.Construction;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Content.Goobstation.Prototypes;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Content.Server.Storage.Components;
using System.Linq;

public sealed class CookingMachinesSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ConstructionSystem _constructionSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingMachinesComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CookingMachinesComponent, AfterInteractEvent>(OnOvenAfterInteract);
        SubscribeLocalEvent<CookingMachinesComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);
        SubscribeLocalEvent<CookingMachinesComponent, EntRemovedFromContainerMessage>(OnEntityRemoved);
        SubscribeLocalEvent<CookingMachinesComponent, ComponentStartup>(OnStartup);
    }

    private void OnGetAltVerbs(EntityUid uid, CookingMachinesComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        // Add an alt-click verb, for example: "Use machine" or "Toggle machine"
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () =>
            {
                var afterInteractEvent = new AfterInteractEvent(
                user: args.User,
                target: uid,
                used: EntityUid.Invalid,
                clickLocation: default,
                canReach: true
                );
                RaiseLocalEvent(uid, afterInteractEvent);
            }
        });
    }
    private void OnStartup(EntityUid uid, CookingMachinesComponent comp, ComponentStartup args)
    {
        UpdateContainsFood(uid, comp);
    }

    private void OnEntityInserted(EntityUid uid, CookingMachinesComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        // Check if the container where the entity was inserted belongs to this entity
        if (containerManager.Containers.Values.Contains(args.Container))
        {
            UpdateContainsFood(uid, comp);
        }
    }

    private void OnEntityRemoved(EntityUid uid, CookingMachinesComponent comp, EntRemovedFromContainerMessage args)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        // Check if the container where the entity was removed belongs to this entity
        if (containerManager.Containers.Values.Contains(args.Container))
        {
            UpdateContainsFood(uid, comp);
        }
    }

    private void UpdateContainsFood(EntityUid uid, CookingMachinesComponent comp)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        foreach (var container in containerManager.Containers.Values)
        {
            if (container.ContainedEntities.Count > 0)
            {
                comp.ContainsFood = true;
                return;
            }
        }

        comp.ContainsFood = false;
    }
    private void OnOvenAfterInteract(EntityUid uid, CookingMachinesComponent comp, AfterInteractEvent args)
    {
        if (!comp.IsOn || !comp.IsOven)
            return;

        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        // Create a multimap of available ingredients
        var available = new Dictionary<EntProtoId, List<EntityUid>>();
        foreach (var container in containerManager.Containers.Values)
        {
            foreach (var ent in container.ContainedEntities)
            {
                if (_entMan.TryGetComponent<MetaDataComponent>(ent, out var meta) &&
                    meta.EntityPrototype?.ID is { } id)
                {
                    var protoId = (EntProtoId) id;
                    if (!available.ContainsKey(protoId))
                        available[protoId] = new();
                    available[protoId].Add(ent);
                }
            }
        }

        foreach (var recipe in _prototype.EnumeratePrototypes<CookingRecipePrototype>())
        {
            if (!recipe.RequiredMachine.Equals("Oven", StringComparison.OrdinalIgnoreCase))
                continue;

            var canCraft = true;
            foreach (var req in recipe.Input)
            {
                if (!available.TryGetValue(req.ID, out var list) || list.Count < req.Amount)
                {
                    canCraft = false;
                    break;
                }
            }

            if (!canCraft)
                continue;

            // Remove required ingredients
            foreach (var req in recipe.Input)
            {
                for (int i = 0; i < req.Amount; i++)
                {
                    var ent = available[req.ID][i];
                    QueueDel(ent);
                }
            }

            Spawn(recipe.Output, Transform(uid).Coordinates);
            break;
        }
    }
}