using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.Queen;

[RegisterComponent]
public sealed partial class XenomorphPromotionComponent : Component
{
    [ViewVariables]
    public EntProtoId PromoteTo = "MobXenomorphPraetorian";

    [ViewVariables]
    public FixedPoint2 PlasmaCost = 0;

    [ViewVariables]
    public List<ProtoId<XenomorphCastePrototype>> CasteWhitelist = new();

    [ViewVariables]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);
}
