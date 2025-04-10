using Content.Shared.Mobs;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.TeleportOnStateChange;

public sealed partial class TeleportOnStateChangeSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TeleportOnStateChangeComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, TeleportOnStateChangeComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != comp.MobStateTrigger)
            return;

        if (comp.MapCoordinatesTeleportTo is { } mapCoords)
        {
            _transformSystem.SetMapCoordinates(uid, mapCoords);
            return;
        }

        if (comp.EntityTeleportTo is not { } entityTeleportTo)
            return;

        if (!_transformSystem.TryGetMapOrGridCoordinates(entityTeleportTo, out var entityCoordinates))
            return;

        var targetCoords = comp.CoordinatesTeleportTo ?? entityCoordinates;

        if (targetCoords != null)
            _transformSystem.SetCoordinates(uid, (EntityCoordinates) targetCoords);

        if (comp.RemoveOnTrigger)
            RemComp<TeleportOnStateChangeComponent>(uid);
    }
}
