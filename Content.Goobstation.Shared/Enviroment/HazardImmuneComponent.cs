namespace Content.Goobstation.Shared.Hazards;

/// <summary>
/// This is used to make simple mobs immune to shock sources that bypass insulation, toxic mist, damaging tiles, and other enviromental effects. This way, only players are fucked over by environmental hazards.
/// </summary>

[RegisterComponent]
public sealed partial class HazardImmuneComponent : Component;
