using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingRodComponent : Component
{
    /// <summary>
    /// Higher value will make every interact more productive.
    /// </summary>
    [DataField]
    public float Efficiency = 1f;

    [DataField, ValidatePrototypeId<EntityPrototype>]
    public EntProtoId FloatPrototype = "FishingLure";

    [DataField, ViewVariables]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Fishing/fishing_lure.rsi"), "rope");

    [DataField, AutoNetworkedField]
    public EntityUid? FishingLure;

    [DataField]
    public EntProtoId ThrowLureActionId = "ActionStartFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? ThrowLureActionEntity;
}
