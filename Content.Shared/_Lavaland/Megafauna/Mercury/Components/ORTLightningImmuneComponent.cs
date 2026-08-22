namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Used for Ether Drinker immunity. Prevents the user from taking damage from lightning.
/// Technically also includes the lightning used in the boss fight.
/// Which means if the player gets their hand on this item through other means, such as through an elite fauna
/// They may have an edge in the fight against the Spider of Mercury.
/// </summary>

[RegisterComponent]
public sealed partial class ORTLightningImmuneComponent : Component;
