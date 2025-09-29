using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class TentacleHookComponent : Component
{
    [DataField]
    public EntProtoId TentacleProto = "TentacleHook";

    [ViewVariables]
    public EntityUid? Projectile;

    [DataField]
    public SpriteSpecifier Sprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Wraith/Objects/Line/tentacle.rsi"), "mid_tentacle");
}
