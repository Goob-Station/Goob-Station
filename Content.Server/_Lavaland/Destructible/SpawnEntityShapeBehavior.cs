using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared._Lavaland.EntityShapes;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Destructible;

[UsedImplicitly]
[DataDefinition]
public sealed partial class SpawnEntityShapeBehavior : IThresholdBehavior
{
    [DataField(required: true)]
    public EntityShape Shape;

    [DataField(required: true)]
    public EntProtoId ToSpawn;

    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        var shapeSys = system.EntityManager.System<EntityShapeSystem>();
        var pos = system.EntityManager.GetComponent<TransformComponent>(owner);
        shapeSys.SpawnEntityShape(Shape, pos.Coordinates, ToSpawn, out _);
    }
}
