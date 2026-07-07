using Content.Pirate.Shared.Fluids;
using Content.Pirate.Shared.Wetness.Systems;
using Content.Shared.Fluids.Components;
using Content.Shared.Inventory;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Pirate.Server.Fluids;

/// <summary>
/// Soaks wading mobs and drains puddles on standing water.
/// </summary>
public sealed class FloorWaterSystem : EntitySystem
{
    [Dependency] private readonly SharedWetnessSystem _wetness = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FloorWaterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FloorWaterComponent, StepTriggerAttemptEvent>(OnStepAttempt);
        SubscribeLocalEvent<FloorWaterComponent, StepTriggeredOffEvent>(OnStepTriggered);
    }

    private void OnMapInit(Entity<FloorWaterComponent> ent, ref MapInitEvent args)
    {
        // Stagger absorb scans across water tiles.
        ent.Comp.AbsorbAccumulator = _random.NextFloat(ent.Comp.AbsorbInterval);
    }

    private void OnStepAttempt(Entity<FloorWaterComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(Entity<FloorWaterComponent> ent, ref StepTriggeredOffEvent args)
    {
        // Wading soaks worn clothing and rinses stains.
        if (HasComp<InventoryComponent>(args.Tripper))
            _wetness.ImmerseInWater(args.Tripper, ent.Comp.ImmersionFlow);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FloorWaterComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.AbsorbAccumulator += frameTime;
            if (comp.AbsorbAccumulator < comp.AbsorbInterval)
                continue;

            comp.AbsorbAccumulator = 0f;
            AbsorbTilePuddles(uid);
        }
    }

    private void AbsorbTilePuddles(EntityUid uid)
    {
        var xform = Transform(uid);
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);

        foreach (var (puddle, _) in _lookup.GetEntitiesInRange<PuddleComponent>(xform.Coordinates, 0.8f))
        {
            var puddleXform = Transform(puddle);
            if (puddleXform.GridUid == gridUid && _map.TileIndicesFor(gridUid, grid, puddleXform.Coordinates) == tile)
                QueueDel(puddle);
        }
    }
}
