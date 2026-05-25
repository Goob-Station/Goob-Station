using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Component for afterimage entities spawned by Sandevistan users.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanAfterimageComponent : Component
{
    /// <summary>
    /// The entity that spawned this afterimage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid SourceEntity;

    /// <summary>
    /// The hue for the rainbow color cycle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Hue;

    /// <summary>
    /// The direction the user's sprite was facing when the afterimage was spawned.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Direction DirectionOverride;
}
