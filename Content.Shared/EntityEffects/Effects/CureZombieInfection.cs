using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CureZombieInfection : EventEntityEffect<CureZombieInfection>
{
    [DataField]
    public bool Innoculate;

    [DataField]
    public bool CureCriticalZombies; // Goob - whether it cures zombies in a critical state or under

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if(Innoculate)
            return Loc.GetString("reagent-effect-guidebook-innoculate-zombie-infection", ("chance", Probability));

        return Loc.GetString("reagent-effect-guidebook-cure-zombie-infection", ("chance", Probability));
    }
}

