// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.EntityEffects;

// i dont even know if this works. if you're reading this, it likely doesn't. Change the Comp.
public sealed partial class CreateRQuantityEntityReactionEffectSystem : EntityEffectSystem<ReactiveComponent, CreateRQuantityEntityReactionEffect>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<CreateRQuantityEntityReactionEffect> args)
    {
        var quantity = _random.Next(1, args.Effect.MaxEntities + 1);

        var coords = _transform.GetMapCoordinates(entity.Owner);

        for (var i = 0; i < quantity; i++)
        {
            var uid = EntityManager.SpawnEntity(args.Effect.Entity, coords);
            _transform.AttachToGridOrMap(uid);
        }
    }
}

[DataDefinition]
public sealed partial class CreateRQuantityEntityReactionEffect : EntityEffectBase<CreateRQuantityEntityReactionEffect>
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

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-create-entity-reaction-effect",
            ("chance", Probability),
            ("entname", prototype.Index<EntityPrototype>(Entity).Name),
            ("amount", MaxEntities));
}
