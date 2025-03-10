using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingRodComponent : Component
{
    /// <summary>
    /// Higher value will make every interact more productive.
    /// </summary>
    [DataField, ViewVariables]
    public float Efficiency = 1f;

    [DataField]
    public EntProtoId FloatPrototype = "FishingLure";

    [DataField]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Fishing/fishing_lure.rsi"), "rope");

    [DataField, ViewVariables]
    public Vector2 RopeOffset = new Vector2(0.25f, 0.25f);

    [DataField, AutoNetworkedField]
    public EntityUid? FishingLure;

    [DataField]
    public EntProtoId ThrowLureActionId = "ActionStartFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? ThrowLureActionEntity;
}
