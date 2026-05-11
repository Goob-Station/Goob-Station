using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// This component is used for showing pop-ups to anyone that goes near this entity, on a timer.
/// The pop-up can be selected from a list, and is picked at random.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DangerZoneComponent : Component
{
    /// <summary>
    /// How far the entity searches for targets to show a pop-up to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PopUpRange = 10f;

    /// <summary>
    /// A list of possible FTL strings for the pop-up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<LocId> Popup = new();

    /// <summary>
    /// Interval between spawn attempts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Interval = TimeSpan.FromSeconds(5);

    public TimeSpan Accumulator = TimeSpan.Zero;
}
