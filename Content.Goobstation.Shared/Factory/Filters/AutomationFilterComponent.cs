using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Marker component for filter items.
/// Only used for whitelisting, does nothing on its own.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AutomationFilterComponent : Component;

/// <summary>
/// Event raised on a filter to determine if it should block an item.
/// </summary>
[ByRefEvent]
public record struct AutomationFilterEvent(EntityUid Item, bool Allowed = false);
