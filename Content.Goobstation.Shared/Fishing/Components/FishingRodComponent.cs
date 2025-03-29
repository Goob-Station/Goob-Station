using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Goobstation.Shared.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingRodComponent : Component
{
    /// <summary>
    /// Higher value will make every interact more productive.
    /// </summary>
    [DataField]
    public float Efficiency = 1f;

    /// <summary>
    /// If lure moves bigger than this distance away from the rod,
    /// it will force it to reel instantly.
    /// </summary>
    [DataField]
    public float BreakOnDistance = 8f;

    [DataField]
    public EntProtoId FloatPrototype = "FishingLure";

    [DataField]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Fishing/fishing_lure.rsi"), "rope");

    [DataField, ViewVariables]
    public Vector2 RopeUserOffset = new (0f, 0f);

    [DataField, ViewVariables]
    public Vector2 RopeLureOffset = new (0f, 0f);

    [DataField, AutoNetworkedField]
    public EntityUid? FishingLure;

    [DataField]
    public EntProtoId ThrowLureActionId = "ActionStartFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? ThrowLureActionEntity;

    [DataField]
    public EntProtoId PullLureActionId = "ActionStopFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? PullLureActionEntity;
}
