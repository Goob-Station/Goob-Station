using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField("chemicalCost")]
    public float ChemicalCost = 0;

    [DataField("useInLesserForm")]
    public bool UseWhileLesserForm = false;
}

#region Events - Basic

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : WorldTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : WorldTargetActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class EnterStasisEvent : InstantActionEvent { }
public sealed partial class ExitStasisEvent : InstantActionEvent { }

#endregion

#region Events - Combat

public sealed partial class ToggleArmbladeEvent : InstantActionEvent { }
public sealed partial class CreateBoneShardEvent : InstantActionEvent { }

#endregion

#region Events - Sting

public sealed partial class StingBlindEvent : EntityTargetActionEvent { }

#endregion

#region Events - Utility



#endregion
