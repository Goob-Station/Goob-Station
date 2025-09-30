using Content.Goobstation.Shared.HellGoose.Components;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.HellGoose;

public sealed partial class HellGooseTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellGooseTeleportComponent, GooseTeleportEvent>(DoTeleport);
    }

    private void DoTeleport(EntityUid uid, HellGooseTeleportComponent component, GooseTeleportEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        TransformComponent? beaconXform = null;
        HellGooseBeaconTeleportComponent? targetBeacon = null;

        // Find the first beacon with a transform
        var query = EntityQueryEnumerator<HellGooseBeaconTeleportComponent, TransformComponent>();
        while (query.MoveNext(out var beaconUid, out var beaconComp, out var xform))
        {
            targetBeacon = beaconComp;
            beaconXform = xform;
            break;
        }

        // If no beacon found, abort
        if (targetBeacon == null || beaconXform == null)
            return;

        // Teleport
        _sharedTransformSystem.SetCoordinates(args.Performer, beaconXform.Coordinates);
    }
}

public sealed partial class GooseTeleportEvent : InstantActionEvent;
