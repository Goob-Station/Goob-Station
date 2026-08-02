namespace Content.Goobstation.Common.NPC;

/// <summary>
/// Raised when an NPC has finished retaliating against a target
/// </summary>
[ByRefEvent]
public readonly record struct NPCRetaliatedOverEvent(EntityUid Target);
