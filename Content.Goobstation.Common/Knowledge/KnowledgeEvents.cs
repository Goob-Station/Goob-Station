using Content.Goobstation.Common.Knowledge.Components;

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

/// <summary>
/// Raised on all children of some entity to try to find an entity with <see cref="KnowledgeContainerComponent"/>
/// </summary>
[ByRefEvent] // Im not sure if it's the right way to do a relay, but whatever, it works.
public record struct KnowledgeContainerRelayEvent(EntityUid Target, EntityUid? Found = null, bool Handled = false);
