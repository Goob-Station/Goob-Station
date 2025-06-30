using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for Ascendant Broadcast ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendantBroadcastComponent : Component
{
    [DataField]
    public string Title = "Ascendant Broadcast";
}
