using Content.Server.Chat;
using Content.Shared.Body.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SuicideCondition : AchievementCondition;

public sealed partial class SuicideConditionSystem : AchievementConditionSystem<SuicideCondition>
{
    [Dependency] private TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> CannotSuicideTag = "CannotSuicide";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, SuicideGhostEvent>(OnSuicideGhost, before: [typeof(SuicideSystem)]);
    }

    private void OnSuicideGhost(Entity<BodyComponent> ent, ref SuicideGhostEvent args)
    {
        if (args.Handled || _tag.HasTag(ent, CannotSuicideTag))
            return;

        ProgressAll(ent);
    }
}
