using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic.Effects;

public sealed partial class CrucibleSoulRecallEvent : BaseAlertEvent
{
    [DataField]
    public EntProtoId EffectProto = "StatusEffectCrucibleSoul";
}
