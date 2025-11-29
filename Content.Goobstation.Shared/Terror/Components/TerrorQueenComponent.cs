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

    /// <summary>
    /// The chance that a tier 2 egg will be spawned instead of a tier 1.
    /// </summary>
    [DataField]
    public float Tier2EggChance = 0.05f;

    /// <summary>
    /// The chance that a tier 3 egg will be spawned instead of a tier 1.
    /// </summary>
    [DataField]
    public float Tier3EggChance = 0.01f;

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg");
}
