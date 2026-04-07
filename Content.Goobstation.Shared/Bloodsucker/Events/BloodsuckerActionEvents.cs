using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodsuckers.Events;

public sealed partial class BloodsuckerCloakEvent : InstantActionEvent;

public sealed partial class BloodsuckerFeedEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class BloodsuckerFeedDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity NetTarget;

    public BloodsuckerFeedDoAfterEvent(NetEntity netTarget)
    {
        NetTarget = netTarget;
    }

    public override DoAfterEvent Clone() => this;
}
