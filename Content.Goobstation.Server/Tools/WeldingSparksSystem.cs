using Content.Goobstation.Common.Tools;
using Content.Shared.Tools.Systems;

namespace Content.Goobstation.Server.Tools;

public sealed class WeldingSparksSystem : EntitySystem
{
    private Dictionary<EntityUid, EntityUid> _spawnedEffects = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeldingSparksComponent, UseToolEvent>(OnUseTool);
        SubscribeLocalEvent<WeldingSparksComponent, SharedToolSystem.ToolDoAfterEvent>(OnAfterUseTool);
    }

    private void OnUseTool(Entity<WeldingSparksComponent> ent, ref UseToolEvent args)
    {
        // `target` can be ent in a few cases like welding a tile.
        // Spawning the sparks on top of the welder looks silly so just return instead.
        if (args.Target is not { } target || target == ent.Owner)
            return;

        // Prioritise the newer one
        if (_spawnedEffects.ContainsKey(target))
        {
            RemoveFromTarget(target);
        }
        _spawnedEffects.Add(target, Spawn(ent.Comp.EffectProto, Transform(target).Coordinates));
    }

    private void OnAfterUseTool(Entity<WeldingSparksComponent> ent, ref SharedToolSystem.ToolDoAfterEvent args)
    {
        if (args.OriginalTarget is not { } target)
            return;
        RemoveFromTarget(GetEntity(target));
    }

    private void RemoveFromTarget(EntityUid target)
    {
        if (!_spawnedEffects.TryGetValue(target, out var effect))
            return;

        QueueDel(effect);
        _spawnedEffects.Remove(target);
    }
}
