using Content.Shared.Body.Components;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBodySystem
{
    public void RestoreMissingOrgans(Entity<BodyComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false) || entity.Comp.Prototype is not { } protoId)
            return;

        var prototype = Prototypes.Index(protoId);

        foreach (var part in GetBodyChildren(entity, entity.Comp))
        {
            foreach (var slot in prototype.Slots.Values)
            {
                foreach (var (organSlot, organProto) in slot.Organs)
                {
                    var containerId = GetOrganContainerId(organSlot);
                    if (!Containers.TryGetContainer(part.Id, containerId, out var container)
                        || container.ContainedEntities.Count > 0)
                        continue;

                    PredictedSpawnInContainerOrDrop(organProto, part.Id, containerId);
                }
            }
        }
    }
}
