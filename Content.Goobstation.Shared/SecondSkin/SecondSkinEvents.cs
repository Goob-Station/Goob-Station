using Content.Shared.Actions;

namespace Content.Goobstation.Shared.SecondSkin;

public sealed partial class ActionActivateSecondSkin : InstantActionEvent
{
}

[ByRefEvent]
public record struct AccumulateDisgustEvent(float LevelIncrease = 0f);
