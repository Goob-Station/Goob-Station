using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Goobstation.Shared.Bloodsuckers.Events;

public sealed partial class BloodsuckerFeedEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerBrawlEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerLungeEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerMesmerizeEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerHasteEvent : WorldTargetActionEvent;
public sealed partial class BloodsuckerTrespassEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class BloodsuckerBrawlDoAfterEvent : SimpleDoAfterEvent
{
    /// <summary>
    /// True = bashing a door, false = bashing a locker.
    /// </summary>
    public bool UserData;
}

[Serializable, NetSerializable]
public sealed partial class BloodsuckerFeedDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class BloodsuckerMesmerizeDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class BloodsuckerLungeDoAfterEvent : SimpleDoAfterEvent;
//[ByRefEvent] public record struct BloodsuckerHasteTrailEvent(Vector2 From, Vector2 To);
//[ByRefEvent] public record struct BloodsuckerTrespassMistEvent(Vector2 From, Vector2 To);

public sealed partial class BloodsuckerCloakEvent : InstantActionEvent;
