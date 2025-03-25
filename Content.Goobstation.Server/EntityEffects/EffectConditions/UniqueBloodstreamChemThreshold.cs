using Content.Server.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects.EffectConditions;

public sealed partial class UniqueBloodstreamChemThreshold : EntityEffectCondition
{
    [DataField]
    public int Max = int.MaxValue;

    [DataField]
    public int Min = -1;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent<BloodstreamComponent>(args.TargetEntity, out var blood))
        {
            if (args.EntityManager.System<SharedSolutionContainerSystem>().ResolveSolution(args.TargetEntity, blood.ChemicalSolutionName, ref blood.ChemicalSolution, out var chemSolution))
                return chemSolution.Contents.Count > Min && chemSolution.Contents.Count < Max;
            return false;
        }
        throw new NotImplementedException();
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold",
            ("max", Max),
            ("min", Min));
    }
}
