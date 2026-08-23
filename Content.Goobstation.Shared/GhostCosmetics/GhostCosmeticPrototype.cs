using Content.Goobstation.Shared.Particles;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.GhostCosmetics;

public enum GhostCosmeticCategory : byte
{
    Particles,
    Hat,
    Mask,
}

[Prototype]
public sealed partial class GhostCosmeticPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; }

    [DataField(required: true)]
    public GhostCosmeticCategory Category { get; private set; }

    [DataField]
    public SpriteSpecifier? Sprite { get; private set; }

    [DataField]
    public ProtoId<ParticleEffectPrototype>? ParticleEffect { get; private set; }
}
