using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Ascendant Broadcast ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendantBroadcastComponent : Component
{
    [DataField]
    public string Title = "Ascendant Broadcast";
}
