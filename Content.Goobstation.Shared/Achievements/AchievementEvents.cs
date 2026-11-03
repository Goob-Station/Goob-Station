using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Achievements;

[Serializable, NetSerializable]
public sealed record AchievementData(
    string Id,
    string? Name,
    string? Description,
    SpriteSpecifier? Icon,
    ProtoId<AchievementCategoryPrototype> Category,
    int Progress,
    int Target,
    DateTime? UnlockedAt,
    float Rarity);

[Serializable, NetSerializable]
public sealed class RequestAchievementsEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class AchievementsStateEvent(List<AchievementData> achievements) : EntityEventArgs
{
    public readonly List<AchievementData> Achievements = achievements;
}

[Serializable, NetSerializable]
public sealed class AchievementUnlockedEvent(AchievementData achievement) : EntityEventArgs
{
    public readonly AchievementData Achievement = achievement;
}
