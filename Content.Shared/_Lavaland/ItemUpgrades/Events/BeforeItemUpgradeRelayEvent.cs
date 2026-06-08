namespace Content.Shared._Lavaland.ItemUpgrades.Events;

[ByRefEvent]
public record struct BeforeItemUpgradeRelayEvent(bool Cancelled = false);
