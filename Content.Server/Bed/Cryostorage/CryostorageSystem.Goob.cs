using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;

namespace Content.Server.Bed.Cryostorage;

public sealed partial class CryostorageSystem
{
    private void EjectContainedMobs(EntityUid sleeper, EntityUid cryostorage)
    {
        var mobs = new List<(EntityUid Mob, BaseContainer Container)>();
        CollectContainedMobs(sleeper, mobs);

        var coords = Transform(cryostorage).Coordinates;
        foreach (var (mob, container) in mobs)
        {
            _container.Remove(mob, container, destination: coords);
        }
    }

    private void CollectContainedMobs(EntityUid uid, List<(EntityUid Mob, BaseContainer Container)> mobs)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var manager))
            return;

        foreach (var container in _container.GetAllContainers(uid, manager))
        {
            foreach (var contained in container.ContainedEntities)
            {
                if (HasComp<MobStateComponent>(contained))
                {
                    mobs.Add((contained, container));
                    continue;
                }

                CollectContainedMobs(contained, mobs);
            }
        }
    }
}
