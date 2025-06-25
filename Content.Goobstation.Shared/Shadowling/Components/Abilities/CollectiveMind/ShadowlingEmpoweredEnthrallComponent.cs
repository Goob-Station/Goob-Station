using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Empowered Enthrall ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingEmpoweredEnthrallComponent : Component
{
    public string? ActionEmpoweredEnthrall = "ActionEmpoweredEnthrall";

    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(1.2);
}
