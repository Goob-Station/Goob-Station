using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using System.Numerics;
using Content.Server._Mono.FireControl;
using Robust.Shared.Timing;
using Content.Shared.Weapons.Ranged;

namespace Content.Server._Mono.Radar;

/// <summary>
/// System that handles radar visualization for hitscan projectiles
/// </summary>
public sealed partial class HitscanRadarSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly RadarBlipSystem _radarBlipSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // Dictionary to track entities that should be deleted after a specific time
    private readonly Dictionary<EntityUid, TimeSpan> _pendingDeletions = new();

    /// <summary>
    /// Event raised before firing the effects for a hitscan projectile.
    /// </summary>
    public sealed class HitscanFireEffectEvent : EntityEventArgs
    {
        public EntityCoordinates FromCoordinates { get; }
        public float Distance { get; }
        public Angle Angle { get; }
        public HitscanPrototype Hitscan { get; }
        public EntityUid? HitEntity { get; }
        public EntityUid? Shooter { get; }

        public HitscanFireEffectEvent(EntityCoordinates fromCoordinates, float distance, Angle angle, HitscanPrototype hitscan, EntityUid? hitEntity = null, EntityUid? shooter = null)
        {
            FromCoordinates = fromCoordinates;
            Distance = distance;
            Angle = angle;
            Hitscan = hitscan;
            HitEntity = hitEntity;
            Shooter = shooter;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HitscanFireEffectEvent>(OnHitscanEffect);
        SubscribeLocalEvent<HitscanRadarComponent, ComponentShutdown>(OnHitscanRadarShutdown);
    }

    private void OnHitscanEffect(HitscanFireEffectEvent ev)
    {
        if (ev.Shooter == null)
            return;

        // Only create hitscan radar blips for entities with FireControllable component
        if (!HasComp<FireControllableComponent>(ev.Shooter.Value))
            return;

        // Create a new entity for the hitscan radar visualization
        // Use the shooter's position to spawn the entity
        var shooterCoords = new EntityCoordinates(ev.Shooter.Value, Vector2.Zero);
        var uid = Spawn(null, shooterCoords);

        // Add the hitscan radar component
        var hitscanRadar = EnsureComp<HitscanRadarComponent>(uid);

        // Determine start position using proper coordinate transformation
        var startPos = _transform.ToMapCoordinates(ev.FromCoordinates).Position;

        // Compute end position in map space (world coordinates)
        var dir = ev.Angle.ToVec().Normalized();
        var endPos = startPos + dir * ev.Distance;

        // Set the origin grid if available
        hitscanRadar.OriginGrid = Transform(ev.Shooter.Value).GridUid;

        // Set the start and end coordinates
        hitscanRadar.StartPosition = startPos;
        hitscanRadar.EndPosition = endPos;

        // Inherit component settings from the shooter entity
        InheritShooterSettings(ev.Shooter.Value, hitscanRadar, ev.Hitscan);

        // Schedule entity for deletion after its lifetime expires
        var deleteTime = _timing.CurTime + TimeSpan.FromSeconds(hitscanRadar.LifeTime);
        _pendingDeletions[uid] = deleteTime;
    }

    /// <summary>
    /// Inherits radar settings from the shooter entity if available
    /// </summary>
    private void InheritShooterSettings(EntityUid shooter, HitscanRadarComponent hitscanRadar, HitscanPrototype hitscan)
    {
        // Try to inherit from shooter's existing HitscanRadarComponent if present
        if (TryComp<HitscanRadarComponent>(shooter, out var shooterHitscanRadar))
        {
            hitscanRadar.RadarColor = shooterHitscanRadar.RadarColor;
            hitscanRadar.LineThickness = shooterHitscanRadar.LineThickness;
            hitscanRadar.Enabled = shooterHitscanRadar.Enabled;
            hitscanRadar.LifeTime = shooterHitscanRadar.LifeTime;
        }
    }

    private void OnHitscanRadarShutdown(Entity<HitscanRadarComponent> ent, ref ComponentShutdown args)
    {
        // Only delete the entity if it's a temporary hitscan trail entity (tracked in _pendingDeletions)
        // Don't delete legitimate entities that have the component added manually
        if (_pendingDeletions.ContainsKey(ent))
        {
            // This is a temporary hitscan trail entity, safe to delete
            QueueDel(ent);
            _pendingDeletions.Remove(ent);
        }
        // For legitimate entities, just remove from pending deletions if present (shouldn't be there anyway)
        else
        {
            _pendingDeletions.Remove(ent);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Handle pending deletions
        if (_pendingDeletions.Count > 0)
        {
            var currentTime = _timing.CurTime;
            var toRemove = new List<EntityUid>();

            foreach (var (entity, deleteTime) in _pendingDeletions)
            {
                if (currentTime >= deleteTime)
                {
                    if (!Deleted(entity))
                        QueueDel(entity);
                    toRemove.Add(entity);
                }
            }

            foreach (var entity in toRemove)
            {
                _pendingDeletions.Remove(entity);
            }
        }
    }
}
