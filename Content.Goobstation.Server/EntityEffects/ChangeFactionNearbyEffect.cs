using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ChangeFactionNearbyEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {

    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-faction");
}
