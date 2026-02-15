using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class DiseaseSpreadModifier
{
    /// <summary>
    /// How much to modify spread attempts' power.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> PowerModifiers = new();

    /// <summary>
    /// By how much to multiply spread attempts' chance.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> ChanceMultipliers = new();

    public float PowerMod(ProtoId<DiseaseSpreadPrototype> proto)
    {
        return PowerModifiers.GetValueOrDefault(proto, 0f);
    }

    public float ChanceMult(ProtoId<DiseaseSpreadPrototype> proto)
    {
        return ChanceMultipliers.GetValueOrDefault(proto, 1f);
    }
}
