using Content.Goobstation.Shared.Body.Components;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Interaction.Events;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Body;

public sealed class OrganInsertOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganInsertOnUseComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<OrganInsertOnUseComponent, TryRemoveOrganEvent>(OnTryRemoveOrgan);
    }

    private void OnTryRemoveOrgan(Entity<OrganInsertOnUseComponent> ent, ref TryRemoveOrganEvent args)
    {
        if (ent.Comp.PreventRemoval)
            args.Cancelled = true;
    }

    private void OnUseInHand(Entity<OrganInsertOnUseComponent> ent, ref UseInHandEvent args)
    {
        args.Handled = true;
        foreach (var (uid, component) in _body.GetBodyChildrenOfType(args.User, ent.Comp.PartType))
        {
            if (!component.Organs.TryGetValue(ent.Comp.SlotId, out var organSlot) ||
                !_container.TryGetContainer(uid,
                    SharedBodySystem.GetOrganContainerId(organSlot.Id),
                    out var container) || container is not ContainerSlot slot)
                continue;

            if (slot.ContainedEntity != null && !_body.TryRemoveOrgan(slot.ContainedEntity.Value))
                continue;

            if (!_container.Insert(ent.Owner, slot, force: true))
                continue;

            return;
        }
    }
}
