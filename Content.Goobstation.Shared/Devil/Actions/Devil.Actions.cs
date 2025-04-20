using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Devil.Actions;

[RegisterComponent, NetworkedComponent]
public sealed partial class DevilActionComponent : Component
{
    [DataField]
    public float SoulsRequired = 0;
}

public sealed partial class CreateContractEvent : InstantActionEvent { }

public sealed partial class CreateRevivalContractEvent : InstantActionEvent { }

public sealed partial class ShadowJauntEvent : InstantActionEvent { }

public sealed partial class DevilPossessionEvent : EntityTargetActionEvent { }
