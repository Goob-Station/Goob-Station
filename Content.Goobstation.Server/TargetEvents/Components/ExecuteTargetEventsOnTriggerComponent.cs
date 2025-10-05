using Content.Goobstation.Common.TargetEvents;

namespace Content.Goobstation.Server.TargetEvents.Components;

[RegisterComponent]
public sealed partial class ExecuteTargetEventsOnTriggerComponent : Component
{
    [DataField]
    public List<BaseTargetEvent> Events;
}
