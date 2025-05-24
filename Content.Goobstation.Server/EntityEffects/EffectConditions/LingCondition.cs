using Content.Goobstation.Shared.Changeling;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Content.Goobstation.Shared.Changeling.Components;

namespace Content.Goobstation.Server.EntityEffects.EffectConditions;

public sealed partial class LingCondition : EntityEffectCondition
{
    public override bool Condition(EntityEffectBaseArgs args)
    {
        return args.EntityManager.HasComponent<ChangelingIdentityComponent>(args.TargetEntity);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-ling");
    }
}