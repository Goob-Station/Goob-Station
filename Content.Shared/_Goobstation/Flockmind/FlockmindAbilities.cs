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


public sealed partial class PartitionMind : InstantActionEvent { }
public sealed partial class RadioStun : InstantActionEvent { }
public sealed partial class RepairBurst : InstantActionEvent { }
public sealed partial class GateCrash : InstantActionEvent { }
public sealed partial class DiffractDrone : EntityTargetActionEvent { }
public sealed partial class ControlDrone : EntityTargetActionEvent { }


public sealed partial class DroneEject : InstantActionEvent { }

