using Content.Shared.StatusEffect;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpiritCandleComponent : Component
{
    [ViewVariables]
    public EntityUid? AreaUid;

    [DataField]
    public EntProtoId SpiritArea = "SpiritCandleRevealArea";

    [DataField]
    public TimeSpan CorporealDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan WeakenedDuration = TimeSpan.FromSeconds(15);

    [ViewVariables]
    public ProtoId<StatusEffectPrototype> Corporeal = "Corporeal";

    [ViewVariables]
    public EntProtoId Weakened = "StatusEffectWeakenedWraith";

    #region Visuals

    [DataField] public string OneCharge = "eye";
    [DataField] public string TwoCharge = "eyes";
    #endregion
}

[Serializable, NetSerializable]
public enum SpiritCandleVisuals : byte
{
    Layer,
}
