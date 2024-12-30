namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player count is within a certain range.
/// </summary>
public sealed partial class ConditionPlayerCount : WhitelistCondition
{
    [DataField]
    public int MinimumPlayers  = 0;
    [DataField]
    public int MaximumPlayers = int.MaxValue;
}
