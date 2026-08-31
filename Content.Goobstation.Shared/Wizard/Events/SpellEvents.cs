using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class ScreamForMeEvent : EntityTargetActionEvent
{
    [DataField]
    public EntProtoId Effect = "SanguineFlashEffect";
}