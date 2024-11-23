using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.MalfAi;

public sealed partial class MalfAiActionComponent : Component
{
    [DataField] public float ControlPowerCost = 0;
}

public sealed partial class OpenModuleMenuEvent : InstantActionEvent { }
public sealed partial class CyborgHijackEvent : EntityTargetActionEvent { }
public sealed partial class ProgramOverrideEvent : EntityTargetActionEvent { }
public sealed partial class MachineOverloadEvent : EntityTargetActionEvent { }
