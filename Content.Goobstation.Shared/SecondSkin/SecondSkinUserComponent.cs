using Content.Shared.Humanoid;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.SecondSkin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SecondSkinUserComponent : Component
{
    [DataField]
    public string Shader = "SecondSkin";

    [DataField, AutoNetworkedField]
    public EntityUid SecondSkin;

    [DataField]
    public List<HumanoidVisualLayers> Layers = new()
    {
        HumanoidVisualLayers.Chest, HumanoidVisualLayers.Groin, HumanoidVisualLayers.LArm,
        HumanoidVisualLayers.LFoot, HumanoidVisualLayers.LHand, HumanoidVisualLayers.LLeg,
        HumanoidVisualLayers.RArm, HumanoidVisualLayers.RFoot, HumanoidVisualLayers.RHand,
        HumanoidVisualLayers.RLeg
    };
}
