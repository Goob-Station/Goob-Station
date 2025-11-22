using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.GridShield;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class GridShieldComponent : Component
{
    /// <summary>
    /// If false, the shield is down and can't be used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Used for shield visuals and some UI,
    /// represents the percentage of health that this shield currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentHealth = 1.0f;

    /// <summary>
    /// Field generator that currently provides actual stats for the shield.
    /// </summary>
    [DataField]
    public EntityUid ConnectedGenerator;
}
