using Content.Shared.Destructible.Thresholds;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Shizophrenia;

/// <summary>
/// Entry for mob spawning hallucinations
/// </summary>
public sealed partial class MobHallucinationsEntry : HallucinationsEntry
{
    public List<EntProtoId> Prototypes = new();

    public MinMax Range = new();

    public MinMax SpawnCount = new();

    protected override void Perform(EntityUid source, IEntityManager entMan, IRobustRandom random)
    {
        var xform = entMan.GetComponent<TransformComponent>(source);
        var transform = entMan.System<TransformSystem>();
        var schizophrenia = entMan.System<SchizophreniaSystem>();

        if (!xform.GridUid.HasValue)
            return;

        var count = SpawnCount.Next(random);

        for (var i = 0; i < count; i++)
        {
            var x = xform.Coordinates.X + Range.Next(random) * (random.Prob(0.5f) ? -1 : 1);
            var y = xform.Coordinates.Y + Range.Next(random) * (random.Prob(0.5f) ? -1 : 1);

            var coords = new EntityCoordinates(xform.GridUid.Value, new(x, y));
            var ent = entMan.Spawn(random.Pick(Prototypes), transform.ToMapCoordinates(coords));
            schizophrenia.AddAsHallucination(source, ent);
        }
    }
}
