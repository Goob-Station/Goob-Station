namespace Content.Goobstation.Server.Shadowling.Rules;

[RegisterComponent, Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField]
    public ShadowlingWinCondition WinCondition = ShadowlingWinCondition.Draw;

    public readonly List<EntityUid> ShadowlingMinds = new();
}

public enum ShadowlingWinCondition : byte
{
    Draw,
    Win,
    Failure
}
