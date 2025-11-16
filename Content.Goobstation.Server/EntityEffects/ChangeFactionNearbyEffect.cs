using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ChangeFactionNearbyEffect : EntityEffect
{
    [DataField(required: true)] public ProtoId<NpcFactionPrototype> NewFaction;
    [DataField] public float Duration = 30f;

    public override void Effect(EntityEffectBaseArgs args)
    {

    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-faction");
}
