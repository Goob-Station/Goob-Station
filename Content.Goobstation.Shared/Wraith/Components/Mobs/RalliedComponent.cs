namespace Content.Goobstation.Shared.Wraith.Components.Mobs;
[RegisterComponent]
public sealed partial class RalliedComponent : Component
{
    /// <summary>
    /// How long the buff lasts before self-deleting.
    /// </summary>
    [DataField]
    public TimeSpan RalliedDuration = TimeSpan.FromSeconds(25);

    /// <summary>
    /// Next tick used for deleting he component.
    /// </summary>
    public TimeSpan NextTick = TimeSpan.Zero;

    /// <summary>
    /// Damage multiplier to rallied mob.
    /// </summary>
    public float RalliedStrength = 1.5f;

    /// <summary>
    /// Attack speed multiplier to rallied mob.
    /// </summary>
    public float RalliedAttackSpeed = 1.5f;

    /// <summary>
    /// Original strength of the mob that got buffed.
    /// </summary>
    public float OriginalStrengthf;

    /// <summary>
    /// Original attack speed of the mob that got buffed.
    /// </summary>
    public float OriginalAttackSpeed;
}
