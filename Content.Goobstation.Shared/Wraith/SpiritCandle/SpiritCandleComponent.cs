using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
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

    /// <summary>
    /// The entity that holds the area
    /// </summary>
    [ViewVariables]
    public EntityUid? Holder;

    /// <summary>
    /// Whether the candle has been lit or not
    /// </summary>
    [ViewVariables]
    public bool Active;

    [DataField]
    public SoundSpecifier SuccessSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithwhisper1.ogg");

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
