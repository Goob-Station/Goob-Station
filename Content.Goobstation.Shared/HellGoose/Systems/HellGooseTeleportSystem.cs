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

        // Make sure the user has a transform
        if (!EntityManager.TryGetComponent<TransformComponent>(args.Performer, out var performerXform))
            return;

        args.Handled = true;

        HellGooseBeaconTeleportComponent? targetBeacon = null;

        // Find the first beacon component
        var query = EntityQueryEnumerator<HellGooseBeaconTeleportComponent>();
        while (query.MoveNext(out _, out var beaconComp))
        {
            if (beaconComp != null)
            {
                targetBeacon = beaconComp;
                break;
            }
        }

        // If no beacon found, abort
        if (targetBeacon == null)
            return;

        // Get the beacon's transform
        if (!EntityManager.TryGetComponent<TransformComponent>(targetBeacon.Owner, out var beaconXform))
            return;

        // Teleport performer using TransformSystem
        performerXform.Coordinates = beaconXform.Coordinates;
    }
}

public sealed partial class GooseTeleportEvent : InstantActionEvent;
