using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Teleports this megafauna back to original spawning place/place where it was activated
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaBlinkInactiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId MarkerId = "TetherEntity"; // Just any dummy entity by default.

    /// <summary>
    /// If true, will spawn its marker entity on mapinit and will always try to teleport to it.
    /// Useful for bosses that shouldn't leave their arena.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FixedPosition;

    /// <summary>
    /// Marker to which we try to teleport on megafauna shutdown.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Marker;
}
