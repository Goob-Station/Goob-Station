using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;
[RegisterComponent, NetworkedComponent]
public sealed partial class RushdownComponent : Component
{
    /// <summary>
    /// How far you will jump (in tiles).
    /// </summary>
    [DataField]
    public float JumpDistance = 5f;

    /// <summary>
    /// Basic “throwing” speed for TryThrow method.
    /// </summary>
    [DataField]
    public float JumpThrowSpeed = 10f;

    /// <summary>
    /// Whether this entity is mid-leap or not. Used to prevent collisions being accidentally triggered outside of the leap.
    /// </summary>
    [DataField]
    public bool IsLeaping = false;

    /// <summary>
    /// The duration of the stun on whoever gets hit by the jump action. Gets applied to the hound itself if they miss.
    /// </summary>
    [DataField]
    public TimeSpan CollideKnockdown = TimeSpan.FromSeconds(2);

    /// <summary>
    /// This gets played whenever the jump action is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? JumpSound = new SoundCollectionSpecifier("Werewolf_Attack");

    /// <summary>
    /// Status effect to make you stunned.
    /// </summary>
    [DataField]
    public string Stunned = "Stun";
}
