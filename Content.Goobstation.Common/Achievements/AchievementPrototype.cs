using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Achievements;

[Prototype("achievement")]
public sealed class AchievementPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public string Name { get; } = string.Empty;

    [DataField]
    public string Description { get; } = string.Empty;

    [DataField]
    public SpriteSpecifier Icon { get; } = default!;

    [DataField]
    public bool Hidden { get; } = false;

    [DataField]
    public AchievementDifficulty Difficulty { get; } = AchievementDifficulty.Normal;
}

public enum AchievementDifficulty
{
    Easy,
    Normal,
    Hard,
    Expert
}
