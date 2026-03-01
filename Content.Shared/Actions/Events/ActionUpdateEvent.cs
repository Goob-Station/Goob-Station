namespace Content.Shared.Actions.Events;


[ByRefEvent]
public record struct ActionUpdateEvent(EntityUid Action, bool QueueDisable = false);
