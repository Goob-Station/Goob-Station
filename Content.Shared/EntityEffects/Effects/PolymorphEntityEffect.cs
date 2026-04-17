using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class Polymorph : EntityEffectBase<Polymorph>
{
    /// <summary>
    ///     What polymorph prototype is used on effect
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> Prototype;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        // <Trauma> rewrite for goob optional polymorph.Entity
        => prototype.Index(Prototype).Configuration.Entity is {} entProto
            ? Loc.GetString("entity-effect-guidebook-make-polymorph",
                ("chance", Probability), ("entityname", prototype.Index(entProto).Name))
            : Loc.GetString("entity-effect-guidebook-random-polymorph", ("chance", Probability));
    // </Trauma>
}
