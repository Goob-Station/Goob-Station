using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

// Here belong all action events for the wraith minions.

public sealed partial class RallyEvent : InstantActionEvent;

public sealed partial class TentacleHookEvent : EntityTargetActionEvent;
public sealed partial class SpikerShuffleEvent : InstantActionEvent;

public sealed partial class RushdownEvent : InstantActionEvent;
