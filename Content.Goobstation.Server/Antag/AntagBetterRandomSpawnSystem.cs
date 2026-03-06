using Content.Goobstation.Server.Antag.Components;
using Content.Server.Antag;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking.Rules;
using Content.Server.SurveillanceCamera;
using Content.Server.Storage.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Antag;

/// <summary>
/// System for AntagBetterRandomSpawn that finds safe spawn locations avoiding cameras and solid objects.
/// </summary>
public sealed class AntagBetterRandomSpawnSystem : GameRuleSystem<AntagBetterRandomSpawnComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagBetterRandomSpawnComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    protected override void Added(EntityUid uid, AntagBetterRandomSpawnComponent comp, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, comp, gameRule, args);

        if (TryFindSafeRandomLocation(comp, out var coords))
            comp.Coords = coords;
        else if (TryFindRandomTile(out _, out _, out _, out var fallbackCoords))
            comp.Coords = fallbackCoords;
    }

    private void OnSelectLocation(Entity<AntagBetterRandomSpawnComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (ent.Comp.Coords != null)
        {
            args.Coordinates.Add(_transform.ToMapCoordinates(ent.Comp.Coords.Value));
            return;
        }

        if (TryFindSafeRandomLocation(ent.Comp, out var coords))
            args.Coordinates.Add(_transform.ToMapCoordinates(coords));
        else if (TryFindRandomTile(out _, out _, out _, out var fallbackCoords))
            args.Coordinates.Add(_transform.ToMapCoordinates(fallbackCoords));
    }

    /// <summary>
    /// Attempts to find a safe random location that avoids cameras and solid objects.
    /// </summary>
    private bool TryFindSafeRandomLocation(AntagBetterRandomSpawnComponent comp, out EntityCoordinates coords)
    {
        coords = EntityCoordinates.Invalid;

        for (var attempt = 0; attempt < comp.MaxAttempts; attempt++)
        {
            if (!TryFindRandomTile(out var tile, out _, out var targetGrid, out var tileCoords))
                continue;

            var gridXform = Transform(targetGrid);
            var mapUid = gridXform.MapUid;

            if (mapUid != null && _atmosphere.IsTileSpace(targetGrid, mapUid, tile))
                continue;

            if (mapUid != null && !_atmosphere.IsTileMixtureProbablySafe(targetGrid, mapUid.Value, tile))
                continue;

            if (HasCameraLineOfSight(tileCoords, comp.CameraCheckRange))
                continue;

            if (IsInsideSolidObject(tileCoords))
                continue;

            coords = tileCoords;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if any active surveillance cameras have line of sight.
    /// </summary>
    private bool HasCameraLineOfSight(EntityCoordinates coords, float range)
    {
        var mapCoords = _transform.ToMapCoordinates(coords);

        foreach (var camera in _lookup.GetEntitiesInRange(mapCoords, range))
        {
            if (!TryComp<SurveillanceCameraComponent>(camera, out var cam) || !cam.Active)
                continue;

            if (_interaction.InRangeUnobstructed(camera, mapCoords, range, CollisionGroup.Opaque))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the given coordinates are inside a solid/impassable object.
    /// </summary>
    private bool IsInsideSolidObject(EntityCoordinates coords)
    {
        var mapCoords = _transform.ToMapCoordinates(coords);
        var checkPos = mapCoords.Position;

        var entities = _lookup.GetEntitiesInRange(mapCoords, 0.5f, LookupFlags.Static | LookupFlags.Dynamic | LookupFlags.Sundries);

        foreach (var entity in entities)
        {
            if (!TryComp<PhysicsComponent>(entity, out var physics))
                continue;

            if (!physics.CanCollide || !physics.Hard)
                continue;

            if ((physics.CollisionLayer & (int) CollisionGroup.Impassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.MidImpassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.HighImpassable) == 0
                && (physics.CollisionLayer & (int) CollisionGroup.TabletopMachineLayer) == 0)
                continue;

            if (!TryComp<FixturesComponent>(entity, out var fixturesComp))
                continue;

            var entityTransform = _physics.GetPhysicsTransform(entity);

            foreach (var fixture in fixturesComp.Fixtures.Values)
                if (_fixtures.TestPoint(fixture.Shape, entityTransform, checkPos))
                    return true;
        }

        return false;
    }
}
