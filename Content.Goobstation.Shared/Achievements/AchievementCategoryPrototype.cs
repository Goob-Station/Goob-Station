using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Achievements;

[Prototype]
public sealed partial class AchievementCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public int Priority;
}
