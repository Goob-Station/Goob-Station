using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WraithPointsComponent : Component
{
    /// <summary>
    ///  Current wraith points the entity has
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public int WraithPoints;

    /// <summary>
    /// How many wraith points the entity starts with
    /// </summary>
    [DataField(required: true)]
    public int StartingWraithPoints;

    /// <summary>
    /// The rate at which the wraith regenerates WP.
    /// </summary>
    [DataField]
    public int WpRegeneration = 1;
}
