using Content.Shared.Body.Components;
using Content.Shared.Interaction;

namespace Content.Shared.Repairable._Goobstation;

/// <summary>
/// xxx
/// </summary>
[ByRefEvent]
public record struct IPCRepairableFinishedEvent(Entity<RepairableComponent> Entity, RepairFinishedEvent Event);

[ByRefEvent]
public record struct IPCRepairableCheckEvent(Entity<RepairableComponent> Entity, InteractUsingEvent Event)
{
    public bool AnythingToHeal = false;
}
