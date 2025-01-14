using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Flockmind;

[RegisterComponent, NetworkedComponent]
public sealed partial class FlockmindActionComponent : Component
{
    [DataField] public float ResourceCost = 0;

    [DataField] public float ComputeCost = 0;
}

public sealed partial class SummonRiftEvent : InstantActionEvent { }
public sealed partial class PartitionMindEvent : InstantActionEvent { }
public sealed partial class RadioStunEvent : InstantActionEvent { }
public sealed partial class RepairBurstEvent : InstantActionEvent { }
public sealed partial class GateCrashEvent : InstantActionEvent { }
public sealed partial class DiffractDroneEvent : EntityTargetActionEvent { }
public sealed partial class ControlDroneEvent : EntityTargetActionEvent { }


public sealed partial class DroneEjectEvent : InstantActionEvent { }

