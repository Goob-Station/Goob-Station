using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Component for afterimage entities spawned by Sandevistan users.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SandevistanAfterimageComponent : Component
{
    /// <summary>
    /// The entity that spawned this afterimage.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid SourceEntity;

    [DataField, AutoNetworkedField]
    public Color Color;

    /// <summary>
    /// The direction the user's sprite was facing when the afterimage was spawned.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Direction DirectionOverride;

    [ViewVariables, AutoNetworkedField]
    public int Order;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? DespawnAt;

    [DataField]
    public TimeSpan FadeDuration = TimeSpan.FromSeconds(0.5);

    [DataField]
    public float BaseAlpha = 0.85f;
}
