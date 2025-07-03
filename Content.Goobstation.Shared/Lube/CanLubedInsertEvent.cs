using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Lube;

/// <Goobstation>
/// Raised on a lubed entity when there's an attempt to insert it into a container.
/// Set CanInsert to true to allow it to be inserted.
/// </Goobstation>
[ByRefEvent]
public record struct CanLubedInsertEvent(ref readonly BaseContainer Into, bool CanInsert = false);
