using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualPathBasedSpawnBehavior : RitualCustomBehavior
{
    [DataField(required: true)]
    public EntProtoId FallbackEntity;

    [DataField]
    public Dictionary<string, EntProtoId> SpawnedEntities = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        if (!args.EntityManager.TryGetComponent(args.Performer, out HereticComponent? heretic))
            return;

        var coords = args.EntityManager.GetComponent<TransformComponent>(args.Platform).Coordinates;

        if (heretic.CurrentPath != null && SpawnedEntities.TryGetValue(heretic.CurrentPath, out var toSpawn))
            args.EntityManager.SpawnEntity(toSpawn, coords);
        else
            args.EntityManager.SpawnEntity(FallbackEntity, coords);
    }
}
