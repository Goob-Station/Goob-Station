using Content.Goobstation.Shared.Achievements;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Achievements;

[Prototype]
public sealed partial class AchievementPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public LocId Description;

    [DataField(required: true)]
    public ProtoId<AchievementCategoryPrototype> Category;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField]
    public bool Hidden;

    [DataField(required: true)]
    public AchievementCondition Condition = default!;
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class AchievementCondition
{
    [DataField]
    public int Count = 1;

    [DataField]
    public EntityCondition[]? Requirements;
}
