using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Common.Damage;
using Content.Goobstation.Shared.GridShield;
using Content.Server.Power.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Maps;
using Content.Shared.Power;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.GridShield;

public sealed class GridShieldSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<DamageableComponent> _damageQuery;
    private EntityQuery<GridShieldComponent> _shieldQuery;
    private EntityQuery<GridShieldDamageComponent> _shieldDamageQuery;
    private EntityQuery<GridShieldGeneratorComponent> _generatorQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GridShieldProtectionComponent, BeforeDamageChangedEvent>(OnBeforeDamageChangedEvent);
        SubscribeLocalEvent<GridShieldDamageComponent, BeforePassiveDamageEvent>(OnShieldRepair);
        SubscribeLocalEvent<GridShieldGeneratorComponent, PowerChangedEvent>(OnPowerChange);
        SubscribeLocalEvent<GridShieldGeneratorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GridShieldGeneratorComponent, ComponentShutdown>(OnShutdown);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _damageQuery = GetEntityQuery<DamageableComponent>();
        _shieldQuery = GetEntityQuery<GridShieldComponent>();
        _shieldDamageQuery = GetEntityQuery<GridShieldDamageComponent>();
        _generatorQuery = GetEntityQuery<GridShieldGeneratorComponent>();
    }

    private void OnPowerChange(Entity<GridShieldGeneratorComponent> ent, ref PowerChangedEvent args)
    {
        ent.Comp.Powered = args.Powered;

        var xform = Transform(ent.Owner);
        if (xform.GridUid == null)
            return;

        UpdateGridShields(xform.GridUid.Value);
    }

    private void OnShieldRepair(Entity<GridShieldDamageComponent> ent, ref BeforePassiveDamageEvent args)
    {
        var xform = Transform(ent.Owner);
        if (!this.IsPowered(xform.ParentUid, EntityManager))
        {
            args.Cancelled = true;
            return;
        }

        if (ent.Comp.LastTimeDamaged < _timing.CurTime + ent.Comp.AfterHitHealReduction)
            args.Multiplier *= ent.Comp.DebuffMultiplier;
    }

    private void OnMapInit(Entity<GridShieldGeneratorComponent> ent, ref MapInitEvent args)
    {
        var coords = new EntityCoordinates(ent.Owner, Vector2.Zero);
        ent.Comp.DamageEntity = SpawnAttachedTo(ent.Comp.DamageEntityId, coords);

        var xform = Transform(ent.Owner);
        if (xform.GridUid == null)
            return;

        UpdateGridShields(xform.GridUid.Value);
    }

    private void OnShutdown(Entity<GridShieldGeneratorComponent> ent, ref ComponentShutdown args)
    {
        var xform = Transform(ent.Owner);
        if (xform.GridUid == null)
            return;

        UpdateGridShields(xform.GridUid.Value);
    }

    private void OnBeforeDamageChangedEvent(Entity<GridShieldProtectionComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Cancelled)
            return;

        var xform = Transform(ent);
        if (!TryGetBlockingShield((ent.Owner, xform), ent.Comp.SpaceSearchRadius, out var shield))
            return;

        var shieldGen = shield.Value.Comp.ConnectedGenerator;
        DebugTools.Assert(!TerminatingOrDeleted(shieldGen), $"{ToPrettyString(shield)} has GridShieldComponent with an invalid ConnectedGenerator!");

        if (!_generatorQuery.TryComp(shieldGen, out var generatorComp)
            || !_damageQuery.TryComp(generatorComp.DamageEntity, out var damageComp)
            || !_shieldDamageQuery.TryComp(generatorComp.DamageEntity, out var shieldDamageComp))
            return;

        DebugTools.Assert(!TerminatingOrDeleted(generatorComp.DamageEntity), $"{ToPrettyString(shieldGen)} DamageEntity is invalid!");

        var gridShield = shield.Value.Comp;

        // Try to calculate the damage
        _damage.TryChangeDamage(generatorComp.DamageEntity, args.Damage);

        // Disable the shield if MaxHealth was reached
        if (damageComp.TotalDamage > generatorComp.MaxDamage)
        {
            shield.Value.Comp.Enabled = false;
            DirtyField(shield.Value.Owner, gridShield, nameof(GridShieldComponent.Enabled));
            return;
        }

        // Update values in our shield
        shieldDamageComp.LastTimeDamaged = _timing.CurTime;

        // Damage redirected successfully
        Spawn(ent.Comp.HitEffect, xform.Coordinates);

        // Update health for all clients
        gridShield.CurrentHealth = 1f - damageComp.TotalDamage.Float() / Math.Max(generatorComp.MaxDamage, 1f);
        DirtyField(shield.Value.Owner, gridShield, nameof(GridShieldComponent.CurrentHealth));

        args.Cancelled = true;
    }

    private bool TryGetBlockingShield(
        Entity<TransformComponent> structure,
        float? spaceSearchRadius,
        [NotNullWhen(true)] out Entity<GridShieldComponent>? shield)
    {
        var xform = structure.Comp;
        shield = null;
        if (xform.GridUid is not { Valid: true } gridUid
            || xform.ParentUid != xform.GridUid // Make sure EntityCoordinates has a grid as its parent
            || spaceSearchRadius != null
            && !CheckTileNearSpace(xform, spaceSearchRadius.Value)
            || !_shieldQuery.TryComp(gridUid, out var shieldGenerator)
            || !shieldGenerator.Enabled)
            return false;

        shield = (gridUid, shieldGenerator);
        return true;
    }

    private bool CheckTileNearSpace(TransformComponent xform, float radius)
    {
        if (!_gridQuery.TryComp(xform.GridUid, out var gridComp))
            return false;

        var worldPos = _transform.GetWorldPosition(xform);
        foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, gridComp, new Circle(worldPos, radius), false))
        {
            if (_turf.IsSpace(tile))
                return false;
        }

        return true;
    }

    private readonly HashSet<Entity<GridShieldGeneratorComponent>> _generators = new();

    public void UpdateGridShields(EntityUid grid)
    {
        _generators.Clear();
        _lookup.GetGridEntities(grid, _generators);
        Entity<GridShieldGeneratorComponent>? targetDrive = null;
        foreach (var (generator, comp) in _generators)
        {
            if (!comp.Powered)
                continue;

            if (comp.Priority > (targetDrive?.Comp.Priority ?? -1))
                targetDrive = (generator, comp);
        }

        if (targetDrive == null)
            return;

        var shieldComp = EnsureComp<GridShieldComponent>(grid);
        shieldComp.ConnectedGenerator = targetDrive.Value;
    }
}
