using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class Polymorph : EntityEffect
{
    /// <summary>
    ///     What polymorph prototype is used on effect
    /// </summary>
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<PolymorphPrototype>))]
    public string PolymorphPrototype { get; set; }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) // Goob edit
    {
        var entProto = prototype.Index<PolymorphPrototype>(PolymorphPrototype).Configuration.Entity;
        if (entProto == null)
            return null;
        var ent = prototype.Index<EntityPrototype>(entProto.Value);
        return Loc.GetString("reagent-effect-guidebook-make-polymorph", ("chance", Probability), ("entityname", ent.Name));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;
        var polySystem = entityManager.System<PolymorphSystem>();

        // Make it into a prototype
        entityManager.EnsureComponent<PolymorphableComponent>(uid);
        polySystem.PolymorphEntity(uid, PolymorphPrototype);
    }
}
