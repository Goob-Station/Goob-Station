using Content.Goobstation.Shared.MartialArts;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class MartialArtKnowledgeEntityConditionSystem : EntityConditionSystem<TransformComponent, MartialArtKnowledgeCondition>
{
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    protected override void Condition(Entity<TransformComponent> ent, ref EntityConditionEvent<MartialArtKnowledgeCondition> args)
    {
        var art = _proto.Index(args.Condition.MartialArt);

        if (args.Condition.CheckKnowledge)
            args.Result = _martialArts.HasMartialArt(ent.Owner, art);
        else
            args.Result = _martialArts.HasMartialArtActive(ent.Owner, art);
    }
}

public sealed partial class MartialArtKnowledgeCondition : EntityConditionBase<MartialArtKnowledgeCondition>
{
    [DataField(required: true)]
    public ProtoId<MartialArtPrototype> MartialArt;

    /// <summary>
    /// Checks whether they know it at all instead of it being active.
    /// Otherwise checks if they know it at all like kitchen cqc
    /// </summary>
    [DataField]
    public bool CheckKnowledge = false;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return "Placeholder";
    }
}
