namespace Content.Goobstation.Server.Werewolf.Components;

/// <summary>
/// if a guy is bit he cannot be bitten again by a werewolf, also he can mutate into a goidawolf
/// this is given when an entity is a target for the werewolfdevour
/// </summary>
[RegisterComponent]
public sealed partial class WerewolfBitComponent : Component{}
