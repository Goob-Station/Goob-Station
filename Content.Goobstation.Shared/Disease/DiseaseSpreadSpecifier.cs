using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class DiseaseSpreadSpecifier
{
    /// <summary>
    /// If the infection attempt gets through, chance for it to actually work
    /// </summary>
    [DataField]
    public float Chance = 1f;

    /// <summary>
    /// Power of the infection attempt, determines how well it gets through infection protection
    /// </summary>
    [DataField]
    public float Power = 1f;

    [DataField("spreadType")]
    public ProtoId<DiseaseSpreadPrototype> Type = "Debug";

    public DiseaseSpreadSpecifier(float chance, float power, ProtoId<DiseaseSpreadPrototype> type)
    {
        Chance = chance;
        Power = power;
        Type = type;
    }
}
