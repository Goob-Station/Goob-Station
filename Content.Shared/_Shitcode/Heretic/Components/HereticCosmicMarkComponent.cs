using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCosmicMarkComponent : Component
{
    [DataField]
    public EntityUid? CosmicDiamondUid;

    [DataField]
    public EntProtoId CosmicDiamond = "EffectCosmicDiamond";

    [DataField]
    public EntProtoId CosmicCloud = "EffectCosmicCloud";

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(2);
}
