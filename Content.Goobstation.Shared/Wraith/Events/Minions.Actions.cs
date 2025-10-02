using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

// Here belong all action events for the wraith minions.

// Skeleton commander actions
public sealed partial class RallyEvent : InstantActionEvent;

// Void spiker actions
public sealed partial class TentacleHookEvent : EntityTargetActionEvent;
public sealed partial class SpikerShuffleEvent : InstantActionEvent;

// Void hound actions
public sealed partial class RushdownEvent : InstantActionEvent;

public sealed partial class CloakEvent : InstantActionEvent;

// Plague rat actions

public sealed partial class EatFilthEvent : EntityTargetActionEvent;

public sealed partial class RatBiteEvent : EntityTargetActionEvent;

public sealed partial class SpawnRatDenEvent : InstantActionEvent;

public sealed partial class RatSlamEvent : InstantActionEvent;
