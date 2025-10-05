using Content.Shared._CorvaxGoob.Events;

namespace Content.Server._CorvaxGoob.Events.Components;

[RegisterComponent]
public sealed partial class ExecuteTargetEventsOnTriggerComponent : Component
{
    [DataField]
    public List<BaseTargetEvent> Events;
}
