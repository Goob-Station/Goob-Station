using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HauntedComponent : Component
{
    /// <summary>
    /// How long the Haunted component lasts until it deletes itself.
    /// </summary>
    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// The deletion time for the component, set automatically on map init.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DeletionTime = TimeSpan.Zero;
}
