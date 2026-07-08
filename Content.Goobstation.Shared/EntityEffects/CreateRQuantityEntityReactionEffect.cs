// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.EntityEffects;

[DataDefinition]
public sealed partial class CreateRQuantityEntityReactionEffect : EntityEffect
{
    /// <summary>
    ///     What entity to create.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Entity = default!;

    /// <summary>
    ///     What is our maximum allowed entities to be spawned?
    /// </summary>
    [DataField]
    public int MaxEntities = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-create-entity-reaction-effect",
            ("chance", Probability),
            ("entname", IoCManager.Resolve<IPrototypeManager>().Index<EntityPrototype>(Entity).Name),
            ("amount", MaxEntities));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var transform = args.EntityManager.GetComponent<TransformComponent>(args.TargetEntity);
        var transformSystem = args.EntityManager.System<SharedTransformSystem>();
        var randomSystem = IoCManager.Resolve<IRobustRandom>();
        var quantity = randomSystem.Next(1, MaxEntities + 1);

        for (var i = 0; i < quantity; i++)
        {
            var uid = args.EntityManager.SpawnEntity(Entity, transformSystem.GetMapCoordinates(args.TargetEntity, xform: transform));
            transformSystem.AttachToGridOrMap(uid);
        }
    }
}
