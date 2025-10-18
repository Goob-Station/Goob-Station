using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wrestler.Events;

public sealed partial class WrestlerKickEvent : EntityTargetActionEvent;

public sealed partial class WrestlerStrikeEvent : EntityTargetActionEvent;

public sealed partial class WrestlerDropEvent : EntityTargetActionEvent;

public sealed partial class WrestlerThrowEvent : EntityTargetActionEvent;

public sealed partial class WrestlerSlamEvent : EntityTargetActionEvent;



[Serializable, NetSerializable]
public sealed partial class WrestlerDropDoAfterEvent : SimpleDoAfterEvent;
