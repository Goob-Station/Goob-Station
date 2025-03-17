using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared.Disease;

[DataDefinition, Serializable, NetSerializable]
public partial class DiseaseSpreadModifier
{
    /// <summary>
    /// How much to modify spread attempts' power.
    /// <summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> PowerModifiers = new();

    /// <summary>
    /// By how much to multiply spread attempts' chance.
    /// <summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> ChanceMultipliers = new();

    public float PowerMod(ProtoId<DiseaseSpreadPrototype> proto)
    {
        return PowerModifiers.ContainsKey(proto) ? PowerModifiers[proto] : 0f;
    }

    public float ChanceMult(ProtoId<DiseaseSpreadPrototype> proto)
    {
        return ChanceMultipliers.ContainsKey(proto) ? ChanceMultipliers[proto] : 1f;
    }
}
