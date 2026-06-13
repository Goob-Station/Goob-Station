using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.Disease;

/// <summary>
/// Mutates diseases on the entity.
/// </summary>
public sealed partial class MutateDiseasesSystem
    : EntityEffectSystem<DiseaseCarrierComponent, MutateDiseases>
{
    protected override void Effect(Entity<DiseaseCarrierComponent> entity, ref EntityEffectEvent<MutateDiseases> args)
    {
        var effect = args.Effect;

        var ev = new MutateDiseases(
            effect.MutationRate,
            effect.Scaled,
            args.Scale,
            effect.Quantity
        );

        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class MutateDiseases : EntityEffectBase<MutateDiseases>
{
    /// <summary>
    /// How much to mutate.
    /// </summary>
    [DataField]
    public float MutationRate = 0.05f;

    [DataField]
    public bool Scaled = true;

    [DataField]
    public float Scale = 1f;

    [DataField]
    public float Quantity = 1f;

    public MutateDiseases() { }

    public MutateDiseases(float mutationRate, bool scaled, float scale, float quantity)
    {
        MutationRate = mutationRate;
        Scaled = scaled;
        Scale = scale;
        Quantity = quantity;
    }

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-disease-mutate",
            ("amount", MutationRate));
    }
}
