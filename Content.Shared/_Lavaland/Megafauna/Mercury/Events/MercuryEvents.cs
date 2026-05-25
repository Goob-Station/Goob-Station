using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Events;

public sealed partial class EtherDrainEvent : InstantActionEvent;

public sealed partial class CardinalSpawnerActionEvent : InstantActionEvent;

public sealed class CardinalSpawnedEvent : EntityEventArgs;

public sealed partial class AddComponentActionEvent : InstantActionEvent
{
    /// <summary>
    /// Component to add.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry TargetComponent = [];

    /// <summary>
    /// If it should be removed after a timer.
    /// </summary>
    [DataField]
    public bool RemoveAfterTimer;

    /// <summary>
    /// What that timer is.
    /// </summary>
    [DataField]
    public TimeSpan TimeToRemoval = TimeSpan.FromSeconds(5);
}

public sealed partial class RemoveComponentActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ComponentRegistry TargetComponent = [];
}

public sealed partial class CosmicRayCirculatorActionEvent : InstantActionEvent;
