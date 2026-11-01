using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

public sealed partial class ModSuitActivateActionEvent : InstantActionEvent;

public sealed partial class ModSuitDeployActionEvent : InstantActionEvent;

public sealed partial class ModModuleActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class ModSuitToggleDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public record struct ModModuleActivatedEvent(EntityUid Suit, EntityUid? Wearer);

[ByRefEvent]
public record struct ModModuleDeactivatedEvent(EntityUid Suit, EntityUid? Wearer);

[ByRefEvent]
public record struct ModModuleUsedEvent(EntityUid Suit, EntityUid User)
{
    public bool Handled;
}

[ByRefEvent]
public record struct ModModuleActivateAttemptEvent(EntityUid Suit, EntityUid? User)
{
    public bool Cancelled;
    public string? Reason;
}

[ByRefEvent]
public record struct ModModuleInstalledEvent(EntityUid Suit);

[ByRefEvent]
public record struct ModModuleRemovedEvent(EntityUid Suit);

[ByRefEvent]
public record struct ModSuitWearerChangedEvent(EntityUid? OldWearer, EntityUid? NewWearer);

[ByRefEvent]
public record struct ModSuitUpdatedEvent;
