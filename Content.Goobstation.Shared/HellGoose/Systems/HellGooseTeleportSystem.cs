using Content.Goobstation.Shared.HellGoose.Components;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.HellGoose;

public sealed partial class HellGooseTeleportSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellGooseTeleportComponent, GooseTeleportEvent>(DoTeleport);
    }

    private void DoTeleport(EntityUid uid, HellGooseTeleportComponent component, GooseTeleportEvent args)
    {
        if (args.Handled)
            return;
        // Make sure the user is valid
        if (!TryComp<TransformComponent>(args.Performer, out var performerXform))
            return;

        args.Handled = true;
        // Find the Bar beacon entity by its prototype ID
        EntityUid? barBeacon = null;
        foreach (var entity in EntityManager.GetEntities())
        {
            if (TryComp<MetaDataComponent>(entity, out var meta) &&
                meta.EntityPrototype?.ID == "DefaultStationBeaconBar")
            {
                barBeacon = entity;
                break;
            }
        }

        if (barBeacon == null)
        {
            return;
        }

        // Get beacon transform
        if (!TryComp<TransformComponent>(barBeacon.Value, out var beaconXform))
            return;

        // Teleport performer to beacon coordinates
        performerXform.Coordinates = beaconXform.Coordinates;
    }
}

public sealed partial class GooseTeleportEvent : InstantActionEvent;
