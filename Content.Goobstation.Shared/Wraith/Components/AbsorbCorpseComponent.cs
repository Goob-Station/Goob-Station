using Content.Shared.DoAfter;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent]
public sealed partial class AbsorbCorpseComponent : Component
{
    /// <summary>
    /// The amount of time the doafter takes for the Wraith to absorb a corpse.
    /// </summary>
    public float AbsorbDuration = 5f;

    /// <summary>
    /// The cooldown for the absorb corpse action.
    /// </summary>
    public float AbsorbCooldown = 45f;

    /// <summary>
    /// The amount of time that should be reduced from the cooldown.
    /// </summary>
    public float CooldownReducer = -5f;

    /// <summary>
    /// The amount of corpses that have already been absorbed
    /// </summary>
    public int CorpsesAbsorbed;
}
