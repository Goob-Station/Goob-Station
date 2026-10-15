using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Vehicles;

public sealed partial class HornActionEvent : InstantActionEvent;
public sealed partial class SirenActionEvent : InstantActionEvent;

[ByRefEvent]
public readonly record struct VehicleHornedEvent(EntityUid Driver);
