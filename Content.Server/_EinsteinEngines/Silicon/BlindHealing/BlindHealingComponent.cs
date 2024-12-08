namespace Content.Server._EinsteinEngines.Silicon.BlindHealing;

[RegisterComponent]
public sealed partial class BlindHealingComponent : Component
{
    [DataField]
    public int DoAfterDelay = 3;

    /// <summary>
    ///     A multiplier that will be applied to the above if an entity is repairing themselves.
    /// </summary>
    [DataField]
    public float SelfHealPenalty = 3f;

    /// <summary>
    ///     Whether or not an entity is allowed to repair itself.
    /// </summary>
    [DataField]
    public bool AllowSelfHeal = true;

    [DataField(required: true)]
    public List<string> DamageContainers;
}

