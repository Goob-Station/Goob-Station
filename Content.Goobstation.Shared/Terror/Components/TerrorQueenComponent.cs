using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorQueenComponent : Component
{
    /// <summary>
    /// Keeps track of how many creatures the hive as a whole has wrapped. Used to multiply the chance of a tier 2 or above to spawn.
    /// </summary>
    [DataField]
    public int HiveTotalWrappedAmount;

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg");

    /// <summary>
    /// Chance for a terror spider to gib upon the queen dying.
    /// </summary>
    [DataField]
    public float DeathGibChance = 0.5f;
}
