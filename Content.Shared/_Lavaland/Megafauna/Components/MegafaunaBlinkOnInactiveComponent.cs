using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Teleports this megafauna back to original spawning place/place where it was activated
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaBlinkInactiveComponent : Component
{
    /// <summary>
    /// Marker to which we try to teleport on megafauna shutdown.
    /// </summary>
    [ViewVariables]
    public EntityUid? Marker;

    /// <summary>
    /// If true, will spawn its marker entity on mapinit and will always try to teleport to it.
    /// Useful for bosses that shouldn't leave their arena.
    /// </summary>
    [DataField]
    public bool FixedPosition;
}
