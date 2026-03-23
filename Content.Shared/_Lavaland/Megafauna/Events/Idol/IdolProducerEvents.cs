using Content.Shared.Actions;

namespace Content.Shared._Lavaland.Megafauna.Events.Idol;

/// <summary>
/// Fired when the Producer uses its single-idol summon action.
/// Spawns one randomly chosen idol from the pool that is not already alive.
/// </summary>
public sealed partial class ProducerSingleIdolSpawnEvent : InstantActionEvent;

/// <summary>
/// Fired when the Producer uses its idol-group summon action.
/// Spawns a randomly chosen group of complementary idols simultaneously.
/// </summary>
public sealed partial class ProducerGroupIdolSpawnEvent : InstantActionEvent;

/// <summary>
/// Fired when the Producer uses its replenish action.
/// Fills the field back up to the configured maximum number of live idols.
/// </summary>
public sealed partial class ProducerReplenishIdolSpawnEvent : InstantActionEvent;
