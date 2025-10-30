namespace Content.Goobstation.Common.Knowledge;

/// <summary>
/// Raised on a knowledge unit entity when it's added to some container entity.
/// </summary>
[ByRefEvent]
public record struct KnowledgeUnitAddedEvent(EntityUid Target);

/// <summary>
/// Raised on a knowledge unit entity when it's removed from some container entity.
/// </summary>
[ByRefEvent]
public record struct KnowledgeUnitRemovedEvent(EntityUid Target);
