using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Network;
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
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public static readonly EntProtoId StarMarkStatusEffect = "StatusEffectStarMark";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicFieldComponent, PreventCollideEvent>(OnPreventColliede);

        SubscribeLocalEvent<StarBlastComponent, ProjectileHitEvent>(OnHit);

        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TerminatingOrDeleted(args.Target) && TryComp(args.Target, out StarMarkComponent? mark))
            RemCompDeferred(args.Target, mark);
    }

    private void OnApply(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<StarMarkComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<StarBlastComponent, TransformComponent>();
        while (query.MoveNext(out var blast, out var xform))
        {
            if (blast.NextCosmicFieldTime > _timing.CurTime)
                continue;

            blast.NextCosmicFieldTime = _timing.CurTime + blast.CosmicFieldPeriod;
            SpawnCosmicField(xform.Coordinates, 5f);
        }
    }

    private void OnHit(Entity<StarBlastComponent> ent, ref ProjectileHitEvent args)
    {
        var coords = Transform(ent).Coordinates;
        var ents = _lookup.GetEntitiesInRange<MobStateComponent>(coords, ent.Comp.StarMarkRadius, LookupFlags.Dynamic);

        foreach (var entity in ents)
        {
            if (TryComp(entity, out HereticComponent? heretic) && heretic.CurrentPath == "Cosmos")
                continue;

            _status.TrySetStatusEffectDuration(entity, StarMarkStatusEffect, ent.Comp.StarMarkDuration);
        }

        if (TryComp(args.Target, out StatusEffectsComponent? targetStatus))
            _stun.KnockdownOrStun(args.Target, ent.Comp.KnockdownTime, true, targetStatus);

        SpawnCosmicFields(coords, 1);
    }

    private void OnPreventColliede(Entity<CosmicFieldComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<StarMarkComponent>(args.OtherEntity))
            args.Cancelled = true;
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
}
