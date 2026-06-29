using Content.Shared.EntityEffects;
using Content.Shared.Mood;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class ChemAddMoodlet : EventEntityEffect<ChemAddMoodlet>
{
    [DataField(required: true)]
    public ProtoId<MoodEffectPrototype> MoodPrototype;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var mood = prototype.Index(MoodPrototype);
        return Loc.GetString(
            "reagent-effect-guidebook-add-moodlet",
            ("amount", mood.MoodChange),
            ("timeout", mood.Timeout));
    }
}
