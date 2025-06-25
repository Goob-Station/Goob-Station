using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling.Thrall;


/// <summary>
/// This is used for the Guise Ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThrallGuiseComponent : Component
{
    [DataField]
    public float Timer = 10f;

    [DataField]
    public float GuiseDuration = 10f;

    public bool Active;
}
