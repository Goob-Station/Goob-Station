using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Disease.Effects;

/// <summary>
/// Mutates diseases on the entity.
/// </summary>
public sealed partial class MutateDiseases : EntityEffect
{
    /// <summary>
    /// How much to mutate.
    /// </summary>
    [DataField]
    public float MutationRate = 0.05f;

    [DataField]
    public bool Scaled = true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-disease-mutate",
            ("amount", MutationRate));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<DiseaseCarrierComponent>(args.TargetEntity, out var carrier))
            return;
        foreach (var diseaseUid in carrier.Diseases.ContainedEntities)
        {
            if (!args.EntityManager.TryGetComponent<DiseaseComponent>(diseaseUid, out var disease))
                continue;

            var sys = args.EntityManager.System<DiseaseSystem>();
            var amt = 1f;
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                if (Scaled)
                    amt *= reagentArgs.Quantity.Float();
                amt *= reagentArgs.Scale.Float();
            }
            sys.MutateDisease((diseaseUid, disease), MutationRate * amt);
        }
    }
}
