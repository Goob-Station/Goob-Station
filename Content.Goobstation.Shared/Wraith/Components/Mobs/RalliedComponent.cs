using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
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
    [DataField]
    public TimeSpan NextTick = TimeSpan.Zero;

    /// <summary>
    /// Damage multiplier to rallied mob.
    /// </summary>
    [DataField]
    public float RalliedStrength = 1.5f;

    /// <summary>
    /// Attack speed multiplier to rallied mob.
    /// </summary>
    [DataField]
    public float RalliedAttackSpeed = 1.5f;
}
