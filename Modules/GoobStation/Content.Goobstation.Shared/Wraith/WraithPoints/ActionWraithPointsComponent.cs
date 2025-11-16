using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActionWraithPointsComponent : Component
{
    /// <summary>
    /// The amount of WP to reduce from the entity on action use
    /// </summary>
    [DataField(required: true)]
    public int WpConsume;

    [DataField]
    public LocId Popup = "wraith-action-generic-fail";
}
