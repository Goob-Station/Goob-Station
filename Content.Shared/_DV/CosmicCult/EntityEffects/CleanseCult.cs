using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.EntityEffects;

public sealed partial class CleanseCult : EventEntityEffect<CleanseCult> // Pirate: blood cult
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-cleanse-cultist", ("chance", Probability));
    }
}
