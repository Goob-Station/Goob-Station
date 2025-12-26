using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PrincessTerrorComponent : Component
{
    /// <summary>
    /// The chance that a tier 2 egg will be spawned instead of a tier 1.
    /// </summary>
    [DataField]
    public float Tier2EggChance = 0.08f;

    /// <summary>
    /// The chance that a tier 3 egg will be spawned instead of a tier 1.
    /// </summary>
    [DataField]
    public float Tier3EggChance = 0.02f;
}
