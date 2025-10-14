using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Queen;

[RegisterComponent]
public sealed partial class XenomorphQueenComponent : Component
{
    [DataField]
    public EntProtoId PromotionActionId = "ActionXenomorphPromotion";

    [DataField]
    public EntProtoId PromotionId = "XenomorphPromotion";

    [DataField]
    public EntProtoId PromoteTo = "MobXenomorphPraetorian";

    [DataField]
    public List<ProtoId<XenomorphCastePrototype>> CasteWhitelist = new() { "Drone", "Hunter", "Sentinel", };

    [DataField]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public EntityUid? Promotion;

    [ViewVariables]
    public EntityUid? PromotionAction;
}
