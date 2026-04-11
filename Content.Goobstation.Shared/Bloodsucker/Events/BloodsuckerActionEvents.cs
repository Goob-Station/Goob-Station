using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Goobstation.Shared.Bloodsuckers.Events;

#region Targeted

public sealed partial class BloodsuckerFeedEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerBrawlEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerLungeEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerMesmerizeEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerHasteEvent : WorldTargetActionEvent;
public sealed partial class BloodsuckerClaimEvent : EntityTargetActionEvent;
public sealed partial class BloodsuckerVassalizeEvent : EntityTargetActionEvent;
#endregion

#region Non-targeted
public sealed partial class BloodsuckerTrespassEvent : InstantActionEvent;
public sealed partial class BloodsuckerCloakEvent : InstantActionEvent;
public sealed partial class BloodsuckerFortitudeEvent : InstantActionEvent;
public sealed partial class BloodsuckerVanishingActEvent : InstantActionEvent;
public sealed partial class BloodsuckerMasqueradeEvent : InstantActionEvent;
public sealed partial class BloodsuckerAcquireScentEvent : InstantActionEvent;
public sealed partial class BloodsuckerFollowScentEvent : InstantActionEvent;
public sealed partial class BloodsuckerHelpVassalEvent : InstantActionEvent;
public sealed partial class BloodsuckerVeilEvent : InstantActionEvent;
public sealed partial class BloodsuckerMaterializeEvent : InstantActionEvent;
public sealed partial class BloodsuckerSenseEvent : InstantActionEvent;
public sealed partial class BloodsuckerGoHomeEvent : InstantActionEvent;
#endregion

#region Do-afters

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

[Serializable, NetSerializable]
public sealed partial class BloodsuckerClaimDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity Coffin;
}
[Serializable, NetSerializable]
public sealed partial class BloodsuckerVassalizeDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity Target;
}

#endregion
