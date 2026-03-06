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

public sealed partial class SpawnFarSideHandEvent : WorldTargetActionEvent
{
}

public sealed partial class SpawnCloseSideHandEvent : WorldTargetActionEvent
{
}

public sealed partial class SpawnFarBarrageEvent : WorldTargetActionEvent
{
}

public sealed partial class SpawnCloseBarrageEvent : WorldTargetActionEvent
{
}
