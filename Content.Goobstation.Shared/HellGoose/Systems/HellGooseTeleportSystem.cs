using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Goobstation.Common.MisandryBox;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.HellGoose;

public sealed partial class HellGooseTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;
    [Dependency] private readonly ThunderstrikeSystem _thunderstrike = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellGooseTeleportComponent, GooseTeleportEvent>(DoTeleport);
        SubscribeLocalEvent<HellGooseTeleportComponent, MapInitEvent>(OnMapInit);
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
    private void OnMapInit(EntityUid uid, HellGooseTeleportComponent component, MapInitEvent args)
    {
        Timer.Spawn(TimeSpan.FromSeconds((double) 40f), () =>
        {
            // check that entities still exist
            if (!EntityManager.EntityExists(uid))
                return;

            // perform the dash
            Smite(uid, false);
        });
    }

    private void Smite(EntityUid uid, bool? killOverride = null)
    {
        _thunderstrike.Smite(uid, kill: killOverride ?? true);
        _popup.PopupEntity(Loc.GetString("goose-reverted"), uid, PopupType.LargeCaution);
    }
}

public sealed partial class GooseTeleportEvent : InstantActionEvent;
