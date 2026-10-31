using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Marks an entity as a territory anchor.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangTerritoryComponent : Component
{
    /// <summary>
    /// Networked so the client overlay can read it.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color GangColor = Color.Transparent;

    [DataField, AutoNetworkedField]
    public float TerritoryRadius = 4f;

    /// <summary>
    /// Buffed healing for the gang locker
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BuffedHealing;
}
