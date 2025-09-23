using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Damage;
using Content.Shared.Maps;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Wraith.Revenant;

public sealed class RevenantShockwaveSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private EntityQuery<StatusEffectsComponent> _statusEffectsQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();

        SubscribeLocalEvent<RevenantShockwaveComponent, RevenantShockwaveEvent>(OnShockwave);
    }

    private void OnShockwave(Entity<RevenantShockwaveComponent> ent, ref RevenantShockwaveEvent args)
    {
        PryAnyTiles(ent);

        var lookup = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);

        foreach (var entity in lookup)
        {
            if (_tag.HasTag(entity, ent.Comp.WallTag) || _tag.HasTag(entity, ent.Comp.WindowTag))
            {
                _damageable.TryChangeDamage(entity, ent.Comp.StructureDamage, true, origin: ent.Owner);
                continue;
            }

            if (!_statusEffectsQuery.TryComp(entity, out var statusEffect))
                continue;

            _stun.KnockdownOrStun(entity, ent.Comp.KnockdownDuration, true, statusEffect);
        }

        // args.Handled = true;
    }

    private void PryAnyTiles(Entity<RevenantShockwaveComponent> ent)
    {
        if (_net.IsClient)
            return;

        var grid = _transform.GetGrid(ent.Owner);
        if (!TryComp<MapGridComponent>(grid, out var map))
            return;

        var tiles = _mapSystem.GetTilesIntersecting(
                grid.Value,
                map,
                Box2.CenteredAround(_transform.GetWorldPosition(ent.Owner),
                    new Vector2(ent.Comp.SearchRange * 2, ent.Comp.SearchRange)))
            .ToArray();

        _random.Shuffle(tiles);

        for (var i = 0; i < ent.Comp.TilesToPry; i++)
        {
            if (!tiles.TryGetValue(i, out var value))
                continue;
            _tile.PryTile(value);
        }
    }
}
