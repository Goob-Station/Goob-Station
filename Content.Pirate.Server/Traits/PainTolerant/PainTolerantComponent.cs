namespace Content.Pirate.Server.Traits.PainTolerant;

/// <summary>
/// Component that modifies the <see cref="MobThresholdsComponent"/> theshold at which entity will go into crit.
/// </summary>
[RegisterComponent]
public sealed partial class PainTolerantComponent : Component
{
    /// <summary>
    /// This value will be ADDED to the treshold at which entity will go into crit.
    /// </summary>
    [DataField]
    public float PainToleranceModifier = 0f;
}
