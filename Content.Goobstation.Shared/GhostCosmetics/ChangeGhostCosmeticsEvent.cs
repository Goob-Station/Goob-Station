using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.GhostCosmetics;

[Serializable, NetSerializable]
public sealed class ChangeGhostCosmeticsEvent : EntityEventArgs
{
    public ProtoId<GhostCosmeticPrototype>? Particles;
    public ProtoId<GhostCosmeticPrototype>? Hat;
    public ProtoId<GhostCosmeticPrototype>? Mask;
}
