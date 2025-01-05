namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player has played for a certain amount of time.
/// </summary>
public sealed partial class ConditionPlaytime : WhitelistCondition
{
    [DataField]
    public int MinimumPlaytime = 0; // In minutes
}
