using Content.Server.StationEvents.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents;

[ByRefEvent]
public record struct StationEventSelectionAttemptEvent(
    IReadOnlyDictionary<EntityPrototype, StationEventComponent> Candidates)
{
    public bool Handled;

    public bool ConsumeSchedule;
}
