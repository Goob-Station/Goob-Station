using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CloakComponent : Component
{
    /// <summary>
    /// How long the buff lasts before expiring.
    /// </summary>
    [DataField]
    public TimeSpan CloakDuration = TimeSpan.FromSeconds(20);

    /// <summary>
    /// If true, cloak is currently active and ticking down.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    /// <summary>
    /// The timestamp when the cloak effect should end.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;
}
