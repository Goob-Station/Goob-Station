using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.BloodCult.UI;

[NetSerializable, Serializable]
public enum BloodRitesUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class BloodRitesUiState(Dictionary<EntProtoId, float> crafts, FixedPoint2 storedBlood)
    : BoundUserInterfaceState
{
    public Dictionary<EntProtoId, float> Crafts = crafts;
    public FixedPoint2 StoredBlood = storedBlood;
}

[Serializable, NetSerializable]
public sealed class BloodRitesMessage(EntProtoId selectedProto) : BoundUserInterfaceMessage
{
    public EntProtoId SelectedProto = selectedProto;
}
