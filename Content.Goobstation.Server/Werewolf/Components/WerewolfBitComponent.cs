namespace Content.Goobstation.Server.Werewolf.Components;

/// <summary>
/// if a guy is bit he cannot be bitten again by a werewolf, also he can mutate into a goidawolf
/// this is given when an entity is a target for the werewolfdevour
/// </summary>
[RegisterComponent]
public sealed partial class WerewolfBitComponent : Component
{
    [DataField] public TimeSpan? StartTime;
    [DataField] public TimeSpan InitialDelay = TimeSpan.FromSeconds(120);
    [DataField] public float SixtyFiveChance = 0.065f; // probably like 1 in 16

    [DataField] public bool WillBeSixtyFive;
    // todo goida popups?
}
