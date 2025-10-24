using Content.Shared.StatusEffect;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpiritCandleComponent : Component
{
    [ViewVariables]
    public EntProtoId SpritiCandleArea = "SpiritCandleRevealArea";

    [ViewVariables]
    public EntityUid? AreaUid;

    [DataField]
    public TimeSpan CorporealDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan WeakenedDuration = TimeSpan.FromSeconds(15);

    [ViewVariables]
    public ProtoId<StatusEffectPrototype> Corporeal = "Corporeal";

    [ViewVariables]
    public EntProtoId Weakened = "StatusEffectWeakenedWraith";
}
