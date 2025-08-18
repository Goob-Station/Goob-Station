using System.Numerics;
using Content.Shared.Alert;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Pirate.Shared.Mage.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]//, AutoGenerateComponentState]
public sealed partial class MageComponent : Component
{
    [DataField("ExperinceCurrencyPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<CurrencyPrototype>))]
    public string ExperinceCurrencyPrototype = "Experince";

    /// <summary>
    /// The total amount of Points the revenant has. Functions
    /// as health and is regenerated.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Experince = 10;

    /// <summary>
    /// The entity's current max amount of essence. Can be increased
    /// through harvesting player souls.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxEssence")]
    public FixedPoint2 ManaRegenCap = 100;

    /// <summary>
    /// The amount of essence passively generated per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("manaPerSecond")]
    public FixedPoint2 ManaPerSecond = 1.0f;

    [ViewVariables]
    public float Accumulator = 0;


    [DataField] public EntityUid? Action;


    #region Mana level
    /// <summary>
    ///     Current amount of energy.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float ManaLevel
    {
        get => _manaLevel;
        set => _manaLevel = Math.Clamp(value, ManaLevelMin, ManaLevelMax);
    }
    public float _manaLevel = 100f;

    /// <summary>
    ///     Don't let PowerLevel go above this value.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float ManaLevelMax = ManaThresholds[ManaThreshold.Max];

    /// <summary>
    ///     Blackeyes if PowerLevel is this value.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float ManaLevelMin = ManaThresholds[ManaThreshold.Min];

    /// <summary>
    ///     How much energy is gained per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float ManaLevelGain = 1.0f;

    /// <summary>
    ///     Whether to gain power or not.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool ManaLevelGainEnabled = true;

    public static readonly Dictionary<ManaThreshold, float> ManaThresholds = new()
    {
        { ManaThreshold.Max, 100.0f },
        { ManaThreshold.Great, 80.0f },
        { ManaThreshold.Good, 60.0f },
        { ManaThreshold.Okay, 40.0f },
        { ManaThreshold.Tired, 20.0f },
        { ManaThreshold.Min, 0.0f },
    };
    #endregion

    [DataField]
    public ProtoId<AlertPrototype> Alert = "ChangelingChemicals";

}

public enum ManaThreshold : byte
{
    Max = 1 << 4,
    Great = 1 << 3,
    Good = 1 << 2,
    Okay = 1 << 1,
    Tired = 1 << 0,
    Min = 0,
}
