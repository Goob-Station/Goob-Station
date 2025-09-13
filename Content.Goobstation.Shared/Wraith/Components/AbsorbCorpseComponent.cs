using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AbsorbCorpseComponent : Component
{
    /// <summary>
    /// The amount of time the doafter takes for the Wraith to absorb a corpse.
    /// </summary>
    [DataField]
    public float AbsorbDuration = 5f;

    /// <summary>
    /// The cooldown for the absorb corpse action.
    /// </summary>
    [DataField]
    public float AbsorbCooldown = 45f;

    /// <summary>
    /// The amount of time that should be reduced from the cooldown.
    /// </summary>
    [DataField]
    public float CooldownReducer = -5f;

    /// <summary>
    /// The amount of corpses that have already been absorbed
    /// </summary>
    [DataField]
    public int CorpsesAbsorbed;
}
