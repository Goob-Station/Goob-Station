using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// A client-side system uses this to tint the sprite to match.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GangColorComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color GangColor = Color.Transparent;
}
