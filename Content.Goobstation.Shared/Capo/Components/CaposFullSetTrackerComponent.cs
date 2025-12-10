using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Capo.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CaposFullSetTrackerComponent : Component
{
    /// <summary>
    /// Number of capo outfit pieces the player is currently wearing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Count;
}
