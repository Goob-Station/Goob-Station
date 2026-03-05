using Content.Shared.EntityTable;
using Robust.Shared.Spawners;

namespace Content.Shared._Lavaland.Spawners;

public sealed class SpawnTableOnDespawnSystem : EntitySystem
{
    [Dependency] private readonly EntityTableSystem _table = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpawnTableOnDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, SpawnTableOnDespawnComponent comp, ref TimedDespawnEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var picked = _table.GetSpawns(comp.Table);
        foreach (var pick in picked)
        {
            Spawn(pick, xform.Coordinates);
        }
    }
}
