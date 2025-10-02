using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class DiseasedRatComponent : Component
{
    /// <summary>
    /// Used to keep track of how much trash the rat has eaten.
    /// </summary>
    [DataField]
    public int FilthConsumed;

    /// <summary>
    /// Once the FilthConsumed reaches this number, the young rat will evolve.
    /// </summary>
    [DataField]
    public int MediumFilthThreshold = 4;

    /// <summary>
    /// Once the FilthConsumed reaches this number, the medium rat will evolve.
    /// </summary>
    [DataField]
    public int GiantFilthThreshold = 8;
}
