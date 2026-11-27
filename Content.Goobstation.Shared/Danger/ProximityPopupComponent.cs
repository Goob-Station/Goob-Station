using Content.Shared.Popups;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Danger;

/// <summary>
/// This component is used for showing pop-ups to anyone that goes near this entity, on a timer.
/// The pop-up can be selected from a list, and is picked at random.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class ProximityPopupComponent : Component
{
    /// <summary>
    /// How far the entity searches for targets to show a pop-up to.
    /// </summary>
    [DataField]
    public float PopupRange = 10f;

    /// <summary>
    /// A list of possible FTL strings for the pop-up.
    /// </summary>
    [DataField(required: true)]
    public List<LocId> Popups;

    /// <summary>
    /// Defines the type of pop-up.
    /// </summary>
    [DataField]
    public PopupType PopupType = PopupType.MediumCaution;

    /// <summary>
    /// Interval between popups.
    /// </summary>
    [DataField]
    public TimeSpan PopupInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// If not null, the system will pick a random number between PopupInterval and MaxPopupInterval.
    /// Otherwise, obviously, just defaults to PopupInterval.
    /// </summary>
    [DataField]
    public TimeSpan? MaxPopupInterval;

    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextPopup;
}
