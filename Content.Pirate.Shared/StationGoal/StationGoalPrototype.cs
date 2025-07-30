using Robust.Shared.Prototypes;

namespace Content.Pirate.Shared.StationGoal;

[Prototype]
public sealed class StationGoalPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    public string Text => Loc.GetString($"station-goal-{ID.ToLower()}");
}
