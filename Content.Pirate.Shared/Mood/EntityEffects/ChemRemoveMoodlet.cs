using Content.Shared.EntityEffects;
using Content.Shared.Mood;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class ChemRemoveMoodlet : EventEntityEffect<ChemRemoveMoodlet>
{
    [DataField(required: true)]
    public ProtoId<MoodEffectPrototype> MoodPrototype;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-remove-moodlet", ("name", prototype.Index(MoodPrototype).Description));
}
