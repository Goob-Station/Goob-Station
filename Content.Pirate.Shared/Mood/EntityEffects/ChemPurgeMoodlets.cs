using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class ChemPurgeMoodlets : EventEntityEffect<ChemPurgeMoodlets>
{
    [DataField]
    public bool RemovePermanentMoodlets;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-purge-moodlets");
}
