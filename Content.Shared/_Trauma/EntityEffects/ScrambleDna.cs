using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// Scrambles the target entity's DNA.
/// Does the same thing as the DNA scrambler implant etc.
/// </summary>
public sealed partial class ScrambleDna : EntityEffectBase<ScrambleDna>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-scramble-dna", ("chance", Probability));
}

public sealed partial class ScrambleDnaEntityEffectSystem : EntityEffectSystem<HumanoidAppearanceComponent, ScrambleDna>
{
    [Dependency] private DnaScrambleOnTriggerSystem _scramble = default!;

    protected override void Effect(Entity<HumanoidAppearanceComponent> ent, ref EntityEffectEvent<ScrambleDna> args)
    {
        _scramble.Scramble(ent, ent.Comp);
    }
}
