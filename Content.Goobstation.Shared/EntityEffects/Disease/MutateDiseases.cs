using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.Disease;

/// <summary>
/// Mutates diseases on the entity.
/// </summary>
public sealed partial class MutateDiseases : EventEntityEffect<MutateDiseases>
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

    public MutateDiseases(float mutationRate, bool scaled, float scale, float quantity)
    {
        MutationRate = mutationRate;
        Scaled = scaled;
        Scale = scale;
        Quantity = quantity;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-disease-mutate",
            ("amount", MutationRate));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<DiseaseCarrierComponent>(args.TargetEntity, out var carrier))
            return;

        var ev = new MutateDiseases(MutationRate, Scaled, Scale, Quantity);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            ev.Scale = reagentArgs.Scale.Float();
            ev.Quantity = reagentArgs.Quantity.Float();
        }

        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ev);
    }
}
