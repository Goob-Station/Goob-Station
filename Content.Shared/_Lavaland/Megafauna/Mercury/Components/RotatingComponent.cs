using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// This component can be added to an entity to make its sprite do a constant 360 degrees rotation.
/// Optionally, can have the speed increase overtime.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RotatingComponent : Component
{
    /// <summary>
    /// How quickly the entity shall rotate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RotationSpeed = 15f;

    /// <summary>
    /// The current rotation speed.
    /// </summary>
    public float CurrentSpeed;

    /// <summary>
    /// The cap on how fast the entity can spin. Most likely will break without one so don't skip this.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaximumSpeed = 100f;

    /// <summary>
    /// How quickly the rotation should speed up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float IncreaseBy = 10f;

    /// <summary>
    /// If the rotation should speed up overtime to begin with.
    /// </summary>
    public bool IncreaseOvertime;

    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan Interval;
}
