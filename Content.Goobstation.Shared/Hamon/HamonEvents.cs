using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Hamon;

[ByRefEvent]
public sealed partial class HamonNextLineEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class InfuseWithHamonEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class OverdriveActivateEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class AssPullEvent : InstantActionEvent;

[ByRefEvent]
public record struct GotInfusedWithHamonEvent;

[ByRefEvent]
public record struct GotUnInfusedWithHamonEvent;
