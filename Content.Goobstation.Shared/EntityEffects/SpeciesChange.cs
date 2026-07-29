using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SpeciesChange : EntityEffectBase<SpeciesChange>
{
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> NewSpecies;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species", ("species", prototype.Index(NewSpecies).Name));
}

public abstract partial class SharedSpeciesChangeEffectSystem : EntityEffectSystem<HumanoidAppearanceComponent, SpeciesChange>
{
    protected override void Effect(Entity<HumanoidAppearanceComponent> ent, ref EntityEffectEvent<SpeciesChange> args)
    {
        Polymorph(ent, args.Effect.NewSpecies);
    }

    public virtual void Polymorph(EntityUid target, ProtoId<SpeciesPrototype> id)
    {
        // this 1 thing is in shared so both species effects can stay in shared, only 1 has to have a server version
    }
}
