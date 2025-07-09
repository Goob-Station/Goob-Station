using Content.Goobstation.Shared.Atmos.Events;
using Content.Server._Lavaland.Procedural.Components;

namespace Content.Goobstation.Server.Atmos.Systems;

/// <summary>
/// System to make atmos bombs have uncapped range on lavaland.
/// </summary>
public sealed class LavalandTankRangeSystem : EntitySystem
{
    private EntityQuery<LavalandMapComponent> _lavalandQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lavalandQuery = GetEntityQuery<LavalandMapComponent>();

        SubscribeLocalEvent<TransformComponent, GasTankGetRangeEvent>(OnGetRange);
    }

    private void OnGetRange(Entity<TransformComponent> ent, ref GasTankGetRangeEvent args)
    {
        if (_lavalandQuery.HasComponent(ent.Comp.MapUid))
            args.MaxRange = float.MaxValue;
    }
}
