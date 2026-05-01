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
        // Goob edit
        => prototype.Index(Prototype).Configuration.Entity is {} entProto
           && prototype.TryIndex(entProto, out var entityProto)
            ? Loc.GetString("entity-effect-guidebook-make-polymorph",
                ("chance", Probability), ("entityname", entityProto.Name))
            : Loc.GetString("entity-effect-guidebook-random-polymorph", ("chance", Probability));
}
