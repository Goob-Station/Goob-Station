namespace Content.Goobstation.Shared.Danger;

/// <summary>
/// This component is used for showing pop-ups to anyone that goes near this entity, on a timer.
/// The pop-up can be selected from a list, and is picked at random.
/// </summary>
[RegisterComponent]
public sealed partial class DangerZoneComponent : Component
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
    /// Interval between popups.
    /// </summary>
    [DataField]
    public TimeSpan PopupInterval = TimeSpan.FromSeconds(5);

    public TimeSpan NextPopup;
}
