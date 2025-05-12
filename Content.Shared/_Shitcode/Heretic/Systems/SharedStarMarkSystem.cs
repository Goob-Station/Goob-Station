using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Heretic;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarMarkSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicFieldComponent, PreventCollideEvent>(OnPreventColliede);

        SubscribeLocalEvent<StarBlastComponent, ProjectileHitEvent>(OnHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<CosmicTrailComponent, TransformComponent>();
        while (query.MoveNext(out var trail, out var xform))
        {
            if (trail.NextCosmicFieldTime > _timing.CurTime)
                continue;

            trail.NextCosmicFieldTime = _timing.CurTime + trail.CosmicFieldPeriod;
            SpawnCosmicField(xform.Coordinates, 5f);
        }
    }

    private void OnHit(Entity<StarBlastComponent> ent, ref ProjectileHitEvent args)
    {
        var coords = Transform(ent).Coordinates;

        ApplyStarMarkInRange(coords, ent.Comp.StarMarkRadius);

        if (TryComp(args.Target, out StatusEffectsComponent? targetStatus))
            _stun.KnockdownOrStun(args.Target, ent.Comp.KnockdownTime, true, targetStatus);

        SpawnCosmicFieldLine(coords, DirectionFlag.North, -1, 1, 0);
        SpawnCosmicFieldLine(coords, DirectionFlag.East, -1, 1, 1);
    }

    private void OnPreventColliede(Entity<CosmicFieldComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<StarMarkComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    public void SpawnCosmicFieldLine(EntityCoordinates coords,
        DirectionFlag directions,
        int start,
        int end,
        int centerSkipRadius,
        float lifetime = 30f)
    {
        if (start > end)
            return;

        var x = (directions & DirectionFlag.West) != 0 ? -1 : (directions & DirectionFlag.East) != 0 ? 1 : 0;
        var y = (directions & DirectionFlag.South) != 0 ? -1 : (directions & DirectionFlag.North) != 0 ? 1 : 0;

        for (var i = start; i <= end; i++)
        {
            if (centerSkipRadius > 0 && Math.Abs(i) < centerSkipRadius)
                continue;

            SpawnCosmicField(coords.Offset(new Vector2i(x * i, y * i)), lifetime);
        }
    }

    public void SpawnCosmicFields(EntityCoordinates coords, int range, float lifetime = 30f)
    {
        if (range < 0)
            return;

        for (var y = -range; y <= range; y++)
        {
            for (var x = -range; x <= range; x++)
            {
                SpawnCosmicField(coords.Offset(new Vector2i(x, y)), lifetime);
            }
        }
    }

    public void SpawnCosmicField(EntityCoordinates coords, float lifetime = 30f)
    {
        if (_net.IsClient)
            return;

        var spawnCoords = coords.SnapToGrid(EntityManager, _mapMan);

        var lookup = _lookup.GetEntitiesInRange<CosmicFieldComponent>(spawnCoords, 0.1f, LookupFlags.Static);
        if (lookup.Count > 0)
        {
            foreach (var lookEnt in lookup)
            {
                if (TryComp(lookEnt, out TimedDespawnComponent? despawn) && despawn.Lifetime < lifetime)
                    despawn.Lifetime = lifetime;
            }

            return;
        }

        var field = Spawn("WallFieldCosmic", spawnCoords);
        _transform.AttachToGridOrMap(field);
        EnsureComp<TimedDespawnComponent>(field).Lifetime = lifetime;
    }

    public void ApplyStarMarkInRange(EntityCoordinates coords, float range)
    {
        var ents = _lookup.GetEntitiesInRange<StatusEffectsComponent>(coords, range, LookupFlags.Dynamic);
        foreach (var (entity, status) in ents)
        {
            TryApplyStarMark(entity, status);
        }
    }

    public bool TryApplyStarMark(EntityUid entity, StatusEffectsComponent? status = null)
    {
        if (!Resolve(entity, ref status))
            return false;

        if (HasComp<HereticComponent>(entity) || HasComp<GhoulComponent>(entity))
            return false;

        return _status.TryAddStatusEffect<StarMarkComponent>(entity,
            "StarMark",
            TimeSpan.FromSeconds(30),
            true,
            status);
    }
}
