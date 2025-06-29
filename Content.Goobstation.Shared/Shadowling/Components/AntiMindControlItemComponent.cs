using Robust.Shared.GameStates;


namespace Content.Goobstation.Shared.Shadowling.Components;


/// <summary>
/// This is used for the Anti Mind Control device
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AntiMindControlItemComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(3);

    [DataField]
    public int MaxCharges;

    [DataField]
    public int CurrentCharges;
}
