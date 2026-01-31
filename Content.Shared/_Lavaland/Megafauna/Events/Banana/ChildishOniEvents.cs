using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Events.Banana;

public sealed partial class OniRampageEvent : WorldTargetActionEvent;

public sealed partial class RingOfFireEvent : InstantActionEvent
{
    [DataField]
    public float RingDistance;

    [DataField]
    public EntProtoId SkullPrototype;
}

public sealed partial class MegafaunaProjectileFlurryEvent : InstantActionEvent;
