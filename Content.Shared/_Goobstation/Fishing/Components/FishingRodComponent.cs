using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent]
public sealed partial class FishingRodComponent : Component
{
    /// <summary>
    /// Higher value will make every interact more productive.
    /// </summary>
    [DataField]
    public float Efficiency = 1f;

    [DataField]
    public EntProtoId FloatPrototype = "FishingLure";

    [DataField, ViewVariables]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Fishing/fishing_lure.rsi"), "rope");

    [DataField]
    public EntityUid? FishingLure;

    [DataField]
    public EntProtoId ThrowLureActionId = "ActionStartFishing";

    [DataField]
    public EntityUid? ThrowLureActionEntity;
}
