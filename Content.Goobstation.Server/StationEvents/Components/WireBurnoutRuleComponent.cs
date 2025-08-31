namespace Content.Goobstation.Server.StationEvents.Components;

/// <summary>
/// used for the wire burnout game event rule
/// </summary>
[RegisterComponent]
public sealed partial class WireBurnoutRuleComponent : Component
{
    [DataField]
    public int Max = 10;

    [DataField]
    public int Min = 1;

    [DataField]
    public bool DeleteAndReplace = false;

    [DataField]
    public string ReplaceWith = "Ash";
}
